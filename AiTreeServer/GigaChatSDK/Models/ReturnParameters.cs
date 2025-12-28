using System.Text.Json.Serialization;

namespace AiTreeServer.GigaChatSDK.Models;

public class ReturnParameters
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("properties")]
    public Dictionary<string, ReturnProperty> Properties { get; set; }

    [JsonPropertyName("required")]
    public List<string> Required { get; set; }
}