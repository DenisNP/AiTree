using System.Text.Json.Serialization;

namespace AiTreeServer.GigaChatSDK.Models;

public class FunctionCall
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("arguments")]
    public Dictionary<string, object> Arguments { get; set; }

    public FunctionCall(string name, Dictionary<string, object> arguments)
    {
        Name = name;
        Arguments = arguments;
    }
}