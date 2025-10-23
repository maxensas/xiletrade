using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Models.Application.Serialization.Converter;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Serialization.SourceGeneration;
using Xiletrade.Library.Services;

namespace Xiletrade.Library.Models.Application.Serialization;

/// <summary>Helper class used for JSON serialization.</summary>
/// <remarks>using System.Text.Json</remarks>
public sealed class JsonHelper : StringCache
{
    private readonly SourceGenerationContext _defaultContext;
    private readonly SourceGenerationContext _nocacheContext;

    // .NET System.Text.Json is not perfect
    private static readonly List<(byte[] source, byte[] target)> _serializeReplacements = new()
    {
        (Encoding.UTF8.GetBytes("\\u00A0"), Encoding.UTF8.GetBytes("\u00A0")),
        (Encoding.UTF8.GetBytes("\\u3000"), Encoding.UTF8.GetBytes("\u3000")),
        (Encoding.UTF8.GetBytes("\\u007F"), Encoding.UTF8.GetBytes("\u007F")),
        (Encoding.UTF8.GetBytes("\\u0022"), Encoding.UTF8.GetBytes("\\\"")),
        (Encoding.UTF8.GetBytes("\\u0027"), Encoding.UTF8.GetBytes("\u0027"))
        //(Encoding.UTF8.GetBytes("\\u0026"), Encoding.UTF8.GetBytes("\u0026"))
    };

    // Handle bad stash names, cryptisk does not resolve :
    private static readonly List<(byte[] source, byte[] target)> _deserializeReplacements = new()
    {
        (Encoding.UTF8.GetBytes("\\\\\","), Encoding.UTF8.GetBytes("\",")),
        (Encoding.UTF8.GetBytes("name:,"), Encoding.UTF8.GetBytes("\"name\":\"\","))
    };

    // List of types for which we want to disable the cache
    // You can easily add more here
    private static readonly HashSet<Type> _excludedTypesFromCache =
        [ typeof(ConfigData), typeof(BulkData)];

    public JsonHelper()
    {
        var optionsNoCache = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            AllowTrailingCommas = true
        };

        _nocacheContext = new(optionsNoCache);
    }

    public JsonHelper(DataManagerService dataManager) : this()
    {
        ArgumentNullException.ThrowIfNull(dataManager);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            AllowTrailingCommas = true
        };

        options.Converters.Add(new InterningStringConverter(dataManager));
        _defaultContext = new(options);
    }

    public string Serialize<T>(object obj, bool replace = true) where T : class
    {
        var context = GetContextOrThrow<T>();
        if (!replace)
        {
            return JsonSerializer.Serialize(obj, typeof(T), context);
        }

        using var memoryStream = new MemoryStream(capacity: 4096);
        using (var writer = new Utf8JsonWriter(memoryStream))
        {
            JsonSerializer.Serialize(writer, obj, typeof(T), context);
        }

        ReadOnlySpan<byte> utf8Json = memoryStream.GetBuffer().AsSpan(0, (int)memoryStream.Length);

        int maxLength = utf8Json.Length * 2;
        byte[] buffer = ArrayPool<byte>.Shared.Rent(maxLength);
        int pos = 0;

        for (int i = 0; i < utf8Json.Length; i++)
        {
            bool matched = false;

            foreach (var (sourceBytes, targetBytes) in _serializeReplacements)
            {
                if (i + sourceBytes.Length <= utf8Json.Length &&
                    utf8Json.Slice(i, sourceBytes.Length).SequenceEqual(sourceBytes))
                {
                    if (pos + targetBytes.Length > buffer.Length)
                    {
                        throw new InvalidOperationException("Buffer too small for replacement");
                    }
                    targetBytes.CopyTo(buffer.AsSpan(pos));
                    pos += targetBytes.Length;
                    i += sourceBytes.Length - 1;
                    matched = true;
                    break;
                }
            }

            if (!matched)
            {
                if (pos >= buffer.Length)
                    throw new InvalidOperationException("Buffer too small for copy");
                buffer[pos++] = utf8Json[i];
            }
        }

        string result = Encoding.UTF8.GetString(buffer, 0, pos);
        ArrayPool<byte>.Shared.Return(buffer, clearArray: true);
        return result;
    }

    public T Deserialize<T>(ReadOnlySpan<char> strData, bool replace = false) where T : class
    {
        var context = GetContextOrThrow<T>();
        if (!replace)
        {
            return JsonSerializer.Deserialize(strData, typeof(T), context) as T
                   ?? throw new InvalidOperationException($"Deserialization returned null for type {typeof(T)}");
        }

        int maxByteCount = Encoding.UTF8.GetMaxByteCount(strData.Length);
        byte[] inputBuffer = ArrayPool<byte>.Shared.Rent(maxByteCount);

        int byteCount = Encoding.UTF8.GetBytes(strData, inputBuffer);
        ReadOnlySpan<byte> utf8Input = inputBuffer.AsSpan(0, byteCount);

        int maxLength = utf8Input.Length * 2;
        byte[] outputBuffer = ArrayPool<byte>.Shared.Rent(maxLength);
        int pos = 0;

        for (int i = 0; i < utf8Input.Length;)
        {
            bool matched = false;

            foreach (var (source, target) in _deserializeReplacements)
            {
                if (i + source.Length <= utf8Input.Length &&
                    utf8Input.Slice(i, source.Length).SequenceEqual(source))
                {
                    if (pos + target.Length > outputBuffer.Length)
                    {
                        throw new InvalidOperationException("Output buffer too small for replacement");
                    }
                    target.CopyTo(outputBuffer.AsSpan(pos));
                    pos += target.Length;
                    i += source.Length;
                    matched = true;
                    break;
                }
            }

            if (!matched)
            {
                if (pos >= outputBuffer.Length)
                    throw new InvalidOperationException("Output buffer too small for copy");
                outputBuffer[pos++] = utf8Input[i];
                i++;
            }
        }

        ReadOnlySpan<byte> cleanedJsonUtf8 = outputBuffer.AsSpan(0, pos);
        T result = JsonSerializer.Deserialize(cleanedJsonUtf8, typeof(T), context) as T;

        ArrayPool<byte>.Shared.Return(inputBuffer, clearArray: true);
        ArrayPool<byte>.Shared.Return(outputBuffer, clearArray: true);

        return result ?? throw new InvalidOperationException($"Deserialization returned null for type {typeof(T)}");
    }

    private SourceGenerationContext GetContextOrThrow<T>() where T : class
    {
        if (_defaultContext is null)
        {
            return _nocacheContext;
        }
        return (ShouldUseInterning<T>() ? _defaultContext : _nocacheContext)
            ?? throw new InvalidOperationException("Json not initialized. Call Json.Initialize(...) first.");
    }

    private static bool ShouldUseInterning<T>() => !_excludedTypesFromCache.Contains(typeof(T));
}
