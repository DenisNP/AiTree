using System.Text.Json.Serialization;

namespace AiTreeServer.GigaChatSDK.Models;

public class ReturnProperty
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}