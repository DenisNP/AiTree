using System.Text.Json.Serialization;

namespace AiTreeServer.GigaChatSDK.Models;

public class Response
{
    [JsonPropertyName("choices")]
    public List<Choice>? Choices { get; set; }

    [JsonPropertyName("created")]
    public int Created { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("usage")]
    public Usage? Usage { get; set; }

    [JsonPropertyName("object")]
    public string? Object { get; set; }
}