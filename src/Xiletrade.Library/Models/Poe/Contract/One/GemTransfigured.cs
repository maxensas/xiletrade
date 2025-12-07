using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract.One;

public sealed class GemTransfigured(string option, string discriminator)
{
    [JsonPropertyName("option")]
    public string Option { get; set; } = option;

    [JsonPropertyName("discriminator")]
    public string Discriminator { get; set; } = discriminator;
}
