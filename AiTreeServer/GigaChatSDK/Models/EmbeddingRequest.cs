using System.Text.Json.Serialization;

namespace AiTreeServer.GigaChatSDK.Models;

public class EmbeddingRequest
{
    [JsonPropertyName("models")]
    public string Models { get; set; }

    [JsonPropertyName("input")]
    public List<string> Input { get; set; }

    public EmbeddingRequest(string models = "Embeddings", List<string> input = null)
    {
        List<string> inputs = new List<string>();
        Models = models;
        Input = input ?? inputs;
    }
}