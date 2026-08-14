using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract.One;

public sealed class HeistFilters
{
    [JsonPropertyName("heist_objective_value")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt ObjectiveValue { get; set; } // moderate, high, precious, priceless

    [JsonPropertyName("heist_escape_routes")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax EscapeRoutes { get; set; }

    [JsonPropertyName("heist_max_escape_routes")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax MaxEscapeRoutes { get; set; }

    [JsonPropertyName("heist_reward_rooms")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax RewardRooms { get; set; }

    [JsonPropertyName("heist_max_reward_rooms")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax MaxRewardRooms { get; set; }

    [JsonPropertyName("heist_wings")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Wings { get; set; }

    [JsonPropertyName("heist_max_wings")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax MaxWings { get; set; }

    [JsonPropertyName("heist_agility")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Agility { get; set; }

    [JsonPropertyName("heist_counter_thaumaturgy")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Thaumaturgy { get; set; }

    [JsonPropertyName("heist_engineering")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Engineering { get; set; }

    [JsonPropertyName("heist_lockpicking")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Lockpicking { get; set; }

    [JsonPropertyName("heist_perception")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Perception { get; set; }

    [JsonPropertyName("heist_brute_force")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax BruteForce { get; set; }

    [JsonPropertyName("heist_deception")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Deception { get; set; }

    [JsonPropertyName("heist_demolition")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Demolition { get; set; }

    [JsonPropertyName("heist_trap_disarmament")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Disarmament { get; set; }
}
