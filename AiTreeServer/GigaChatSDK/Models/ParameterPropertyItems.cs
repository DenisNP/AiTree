using System.Text.Json.Serialization;

namespace AiTreeServer.GigaChatSDK.Models;

public record ParameterPropertyItems
{
    [JsonPropertyName("type")]
    public required string Type { get; set; }
}