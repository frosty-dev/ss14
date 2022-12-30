using System.Text.Json.Serialization;

namespace Content.Server.UtkaIntegration;

public sealed class FromDiscordMessage
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    [JsonPropertyName("ckey")]
    public string? Ckey { get; set; }

    [JsonPropertyName("command")]
    public string? Command { get; set; }

    [JsonPropertyName("message")]
    public List<string>? Message { get; set; }

}
