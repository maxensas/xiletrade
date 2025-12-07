using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

public sealed class ItemClassData
{
    [JsonPropertyName("item_class")]
    public ItemClass[] ItemClass { get; set; } = null;
}