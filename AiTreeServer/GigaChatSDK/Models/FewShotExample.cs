using System.Text.Json.Serialization;

namespace AiTreeServer.GigaChatSDK.Models;

public class FewShotExample
{
    [JsonPropertyName("request")]
    public string? Request { get; set; }

    [JsonPropertyName("params")]
    public Dictionary<string, object>? Params { get; set; }
}