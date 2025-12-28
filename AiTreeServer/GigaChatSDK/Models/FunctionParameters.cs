using System.Text.Json.Serialization;

namespace AiTreeServer.GigaChatSDK.Models;

public class FunctionParameters
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("properties")]
    public Dictionary<string, ParameterProperty>? Properties { get; set; }

    [JsonPropertyName("required")]
    public List<string>? Required { get; set; }
}