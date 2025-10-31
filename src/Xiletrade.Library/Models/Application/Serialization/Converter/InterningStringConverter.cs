using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xiletrade.Library.Services;

namespace Xiletrade.Library.Models.Application.Serialization.Converter;

public sealed class InterningStringConverter : JsonConverter<string>
{
    private static IServiceProvider _serviceProvider;

    public InterningStringConverter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string value = reader.GetString();

        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return _serviceProvider.GetRequiredService<DataManagerService>().Json.Intern(value.AsSpan());
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}