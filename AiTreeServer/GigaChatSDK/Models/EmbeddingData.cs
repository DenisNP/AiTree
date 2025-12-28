using System.Text.Json.Serialization;

namespace AiTreeServer.GigaChatSDK.Models;

public class EmbeddingData
{
    [JsonPropertyName("object")]
    public string Object { get; set; }

    [JsonPropertyName("embedding")]
    public List<float> Embedding { get; set; }

    [JsonPropertyName("index")]
    public int Index { get; set; }

    public EmbeddingData(string @object = "embedding", List<float> embedding = null, int index = 0)
    {
        List<float> embeddings = new List<float>();
        Object = @object;
        Embedding = embedding ?? embeddings;
        Index = index;
    }
}