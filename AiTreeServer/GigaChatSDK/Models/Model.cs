using System.Text.Json.Serialization;

namespace AiTreeServer.GigaChatSDK.Models;

public class Model
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    [JsonPropertyName("object")]
    public string? Object { get; set; }
    [JsonPropertyName("owned_by")]
    public string? OwnedBy { get; set; }
}