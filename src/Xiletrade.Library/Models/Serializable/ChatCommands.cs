using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract()]
public sealed class ChatCommands
{
    [DataMember(Name = "id")]
    public int Id { get; set; }

    [DataMember(Name = "command")]
    public string Command { get; set; } = string.Empty;
}
