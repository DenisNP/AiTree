using AiTreeServer.GigaChatSDK.Models;

namespace AiTreeServer.GigaChatSDK.Interfaces;

public interface IGigaChat
{
    Task<Response?> CompletionsAsync(MessageQuery query);
    Task<Response?> CompletionsAsync(string role, string message);
}