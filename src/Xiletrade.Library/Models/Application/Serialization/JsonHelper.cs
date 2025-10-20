using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Models.Application.Serialization.Converter;
using Xiletrade.Library.Models.Poe.Contract.One;
using Xiletrade.Library.Models.Poe.Contract.Two;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.Models.Serialization.SourceGeneration;
using Xiletrade.Library.Services;

namespace Xiletrade.Library.Models.Application.Serialization;

/// <summary>Helper class used for JSON serialization.</summary>
/// <remarks>using System.Text.Json</remarks>
internal sealed class JsonHelper : StringCache
{
    private readonly SourceGenerationContext _defaultContext;
    private readonly SourceGenerationContext _nocacheContext;
    private readonly DataManagerService _dm;

    // .NET System.Text.Json is not perfect
    private static List<(byte[] source, byte[] target)> _replacements = new()
    {
        (Encoding.UTF8.GetBytes("\\u00A0"), Encoding.UTF8.GetBytes("\u00A0")),
        (Encoding.UTF8.GetBytes("\\u3000"), Encoding.UTF8.GetBytes("\u3000")),
        (Encoding.UTF8.GetBytes("\\u007F"), Encoding.UTF8.GetBytes("\u007F")),
        (Encoding.UTF8.GetBytes("\\u0022"), Encoding.UTF8.GetBytes("\\\"")),
        (Encoding.UTF8.GetBytes("\\u0027"), Encoding.UTF8.GetBytes("\u0027"))
        //(Encoding.UTF8.GetBytes("\\u0026"), Encoding.UTF8.GetBytes("\u0026"))
    };

    // Handle bad stash names, cryptisk does not resolve :
    private static List<(byte[] source, byte[] target)> _replaceBadStash = new()
    {
        (Encoding.UTF8.GetBytes("\\\\\","), Encoding.UTF8.GetBytes("\",")),
        (Encoding.UTF8.GetBytes("name:,"), Encoding.UTF8.GetBytes("\"name\":\"\","))
    };

    internal JsonHelper(DataManagerService dataManager)
    {
        if (dataManager is null)
        {
            throw new ArgumentNullException(nameof(dataManager));
        }
        _dm = dataManager;

        var optionsNoCache = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            AllowTrailingCommas = true
        };

        _nocacheContext = new SourceGenerationContext(optionsNoCache);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            AllowTrailingCommas = true
        };

        options.Converters.Add(new InterningStringConverter(dataManager));
        _defaultContext = new SourceGenerationContext(options);
    }

    internal string Serialize<T>(object obj) where T : class
    {
        var context = (ShouldUseInterning<T>() ? _defaultContext : _nocacheContext) ?? throw new InvalidOperationException("Json not initialized. Call Json.Initialize(...) first.");

        using var memoryStream = new MemoryStream(capacity: 4096);
        using (var writer = new Utf8JsonWriter(memoryStream))
        {
            JsonSerializer.Serialize(writer, obj, typeof(T), context);
        }

        ReadOnlySpan<byte> utf8Json = memoryStream.GetBuffer().AsSpan(0, (int)memoryStream.Length);
        var resultBytes = new List<byte>(utf8Json.Length);

        for (int i = 0; i < utf8Json.Length; i++)
        {
            bool matched = false;

            foreach (var (sourceBytes, targetBytes) in _replacements)
            {
                if (i + sourceBytes.Length <= utf8Json.Length &&
                    utf8Json.Slice(i, sourceBytes.Length).SequenceEqual(sourceBytes))
                {
                    resultBytes.AddRange(targetBytes);
                    i += sourceBytes.Length - 1;
                    matched = true;
                    break;
                }
            }

            if (!matched)
            {
                resultBytes.Add(utf8Json[i]);
            }
        }

        return Encoding.UTF8.GetString([.. resultBytes]);
    }

    internal T Deserialize<T>(ReadOnlySpan<char> strData) where T : class
    {
        var context = (ShouldUseInterning<T>() ? _defaultContext : _nocacheContext) ?? throw new InvalidOperationException("Json not initialized. Call Json.Initialize(...) first.");

        return JsonSerializer.Deserialize(strData, typeof(T), context) as T
            ?? throw new InvalidOperationException($"Deserialization returned null for type {typeof(T)}");
    }

    internal T DeserializeFetch<T>(ReadOnlySpan<char> strData) where T : class
    {
        var context = (ShouldUseInterning<T>() ? _defaultContext : _nocacheContext)
            ?? throw new InvalidOperationException("Json not initialized. Call Json.Initialize(...) first.");

        // Encoder en UTF-8 sans allouer de string intermédiaire
        int maxByteCount = Encoding.UTF8.GetMaxByteCount(strData.Length);
        byte[] buffer = ArrayPool<byte>.Shared.Rent(maxByteCount);

        int byteCount = Encoding.UTF8.GetBytes(strData, buffer);
        ReadOnlySpan<byte> utf8Input = buffer.AsSpan(0, byteCount);

        var outputBytes = new List<byte>(utf8Input.Length);

        for (int i = 0; i < utf8Input.Length;)
        {
            bool matched = false;

            foreach (var (source, target) in _replaceBadStash)
            {
                if (i + source.Length <= utf8Input.Length &&
                    utf8Input.Slice(i, source.Length).SequenceEqual(source))
                {
                    outputBytes.AddRange(target);
                    i += source.Length;
                    matched = true;
                    break;
                }
            }

            if (!matched)
            {
                outputBytes.Add(utf8Input[i]);
                i++;
            }
        }

        ReadOnlySpan<byte> cleanedJsonUtf8 = CollectionsMarshal.AsSpan(outputBytes);
        T result = JsonSerializer.Deserialize(cleanedJsonUtf8, typeof(T), context) as T;

        ArrayPool<byte>.Shared.Return(buffer);

        return result ?? throw new InvalidOperationException($"Deserialization returned null for type {typeof(T)}");
    }

    internal string GetSerialized(XiletradeItem xiletradeItem, ItemData item, bool useSaleType, string market)
    {
        var isPoe2 = _dm.Config.Options.GameVersion is 1;
        if (isPoe2)
        {
            var jsonDataTwo = new JsonDataTwo(_dm, xiletradeItem, item, useSaleType, market);
            return Serialize<JsonDataTwo>(jsonDataTwo);
        }
        var jsonData = new JsonData(_dm, xiletradeItem, item, useSaleType, market);
        return Serialize<JsonData>(jsonData);
    }

    private static bool ShouldUseInterning<T>() where T : class
    {
        // List of types for which we want to disable the cache
        // You can easily add more here
        return typeof(T) != typeof(ConfigData);
    }
}
