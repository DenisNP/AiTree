using System.Text.Json.Serialization;

namespace AiTreeServer.GigaChatSDK.Models;

public class ParameterProperty
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("items")]
    public ParameterPropertyItems? Items { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}