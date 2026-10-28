using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Models.Application.Serialization.Converter;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Serialization.SourceGeneration;

namespace Xiletrade.Library.Models.Application.Serialization;

/// <summary>Helper class used for JSON serialization.</summary>
/// <remarks>using System.Text.Json</remarks>
public sealed class JsonHelper : StringCache
{
    private readonly SourceGenerationContext _converterContext;
    private readonly SourceGenerationContext _defaultContext;

    /// <summary>
    /// Default Context that does not use string cache (InterningStringConverter)
    /// </summary>
    public SourceGenerationContext DefaultContext => _defaultContext;

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
        optionsNoCache.Converters.Add(new FlexibleNullableDecimalConverter());
        optionsNoCache.Converters.Add(new HashMapConverter());

        // following are needed for unit tests
        optionsNoCache.Converters.Add(new ValueTupleListConverter());
        optionsNoCache.Converters.Add(new IntegerJsonConverter());
        optionsNoCache.Converters.Add(new DoubleJsonConverter());

        _defaultContext = new(optionsNoCache);
    }

    public JsonHelper(bool useCache) : this()
    {
        if (!useCache)
        {
            return;
        }
        
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            AllowTrailingCommas = true
        };

        //options.Converters.Add(new QueryTypeJsonConverter(serviceProvider));
        //options.Converters.Add(new ArrayStringJsonConverter());
        //options.Converters.Add(new FlexibleStringConverter());
        options.Converters.Add(new ModAffixConverter(_defaultContext));
        options.Converters.Add(new ItemExtendedOrEmptyArrayConverter(_defaultContext));
        options.Converters.Add(new ValueTupleListConverter());
        options.Converters.Add(new HashMapConverter());
        options.Converters.Add(new IntegerJsonConverter());
        options.Converters.Add(new FlexibleNullableDecimalConverter());
        options.Converters.Add(new DoubleJsonConverter());
        options.Converters.Add(new StatusConverter(_defaultContext));

        options.Converters.Add(new InterningStringConverter(this)); // cache
        _converterContext = new(options);
    }

    /// <summary>
    /// Serializes an object to UTF-8 json (no string conversion).
    /// </summary>
    /// <param name="replace">Fixes the characters which .NET escapes and the api does not expect.</param>
    public byte[] SerializeToUtf8<T>(object obj, bool replace = true) where T : class
    {
        var context = GetContextOrThrow<T>();
        if (!replace)
        {
            return JsonSerializer.SerializeToUtf8Bytes(obj, typeof(T), context);
        }

        using var memoryStream = new MemoryStream(capacity: 4096);
        using (var writer = new Utf8JsonWriter(memoryStream, 
            new JsonWriterOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            }))
        {
            JsonSerializer.Serialize(writer, obj, typeof(T), context);
        }

        // The replacements are done in place, in the buffer of the stream : each target is shorter than its source,
        // so the write position never goes past the read position. No second buffer (nor pooled one) is needed.
        byte[] buffer = memoryStream.GetBuffer();
        int length = ApplyReplacements(buffer.AsSpan(0, (int)memoryStream.Length), buffer, _serializeReplacements);
        return buffer.AsSpan(0, length).ToArray();
    }

    /// <summary>
    /// Deserializes UTF-8 json, as received from the network (no string conversion).
    /// </summary>
    /// <param name="utf8Json">Json text, a byte order mark at the beginning is skipped.</param>
    /// <param name="replace">Fixes the known bad data of the api before the deserialization.</param>
    public T Deserialize<T>(ReadOnlySpan<byte> utf8Json, bool replace = false) where T : class
    {
        var context = GetContextOrThrow<T>();
        if (utf8Json.StartsWith(Utf8Bom))
        {
            utf8Json = utf8Json[Utf8Bom.Length..];
        }

        if (!replace || !ContainsReplacement(utf8Json))
        {
            return JsonSerializer.Deserialize(utf8Json, typeof(T), context) as T
                   ?? throw new InvalidOperationException($"Deserialization returned null for type {typeof(T)}");
        }

        // exact length first, then the text is built on the stack when it is small (heap otherwise, no pooled buffer)
        int length = ApplyReplacements(utf8Json, default, _deserializeReplacements);
        Span<byte> output = length <= 1024 ? stackalloc byte[length] : new byte[length];
        ApplyReplacements(utf8Json, output, _deserializeReplacements);
        return JsonSerializer.Deserialize(output, typeof(T), context) as T
               ?? throw new InvalidOperationException($"Deserialization returned null for type {typeof(T)}");
    }

    private static ReadOnlySpan<byte> Utf8Bom => [0xEF, 0xBB, 0xBF];

    private static bool ContainsReplacement(ReadOnlySpan<byte> utf8Json)
    {
        foreach (var (source, _) in _deserializeReplacements)
        {
            if (utf8Json.IndexOf(source) >= 0)
            {
                return true;
            }
        }
        return false;
    }

    // copies the text into the buffer (it can be the same memory when every target is shorter than its source), the earliest source found is replaced by its target each time
    // with an empty buffer, nothing is copied and the length of the result is returned
    private static int ApplyReplacements(ReadOnlySpan<byte> utf8Json, Span<byte> buffer, List<(byte[] source, byte[] target)> replacements)
    {
        int length = 0;
        while (!utf8Json.IsEmpty)
        {
            int next = -1, nextIndex = int.MaxValue;
            for (int i = 0; i < replacements.Count; i++)
            {
                int index = utf8Json.IndexOf(replacements[i].source);
                if (index >= 0 && index < nextIndex)
                {
                    next = i;
                    nextIndex = index;
                }
            }

            if (next < 0)
            {
                if (!buffer.IsEmpty)
                {
                    utf8Json.CopyTo(buffer[length..]);
                }
                return length + utf8Json.Length;
            }

            var (source, target) = replacements[next];
            if (!buffer.IsEmpty)
            {
                utf8Json[..nextIndex].CopyTo(buffer[length..]);
                target.CopyTo(buffer[(length + nextIndex)..]);
            }
            length += nextIndex + target.Length;
            utf8Json = utf8Json[(nextIndex + source.Length)..];
        }
        return length;
    }

    /// <summary>
    /// Deserializes a UTF-8 json stream, without building the text in memory.
    /// </summary>
    /// <remarks>A UTF-8 byte order mark at the beginning of the stream is skipped (the stream must be seekable to detect it).</remarks>
    public T Deserialize<T>(Stream utf8Json) where T : class
    {
        var context = GetContextOrThrow<T>();
        SkipUtf8Bom(utf8Json);
        return JsonSerializer.Deserialize(utf8Json, typeof(T), context) as T
               ?? throw new InvalidOperationException($"Deserialization returned null for type {typeof(T)}");
    }

    private static void SkipUtf8Bom(Stream stream)
    {
        if (!stream.CanSeek)
        {
            return;
        }
        Span<byte> bom = stackalloc byte[3];
        int read = stream.Read(bom);
        if (read != 3 || bom[0] != 0xEF || bom[1] != 0xBB || bom[2] != 0xBF)
        {
            stream.Position = 0;
        }
    }

    internal SourceGenerationContext GetContextOrThrow<T>() where T : class
    {
        if (_converterContext is null)
        {
            return _defaultContext;
        }
        return (ShouldUseInterning<T>() ? _converterContext : _defaultContext)
            ?? throw new InvalidOperationException("Json not initialized. Call Json.Initialize(...) first.");
    }

    private static bool ShouldUseInterning<T>() => !_excludedTypesFromCache.Contains(typeof(T));
}
