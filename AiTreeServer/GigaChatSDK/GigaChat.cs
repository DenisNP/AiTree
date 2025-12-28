using System.Text;
using System.Text.Json;
using AiTreeServer.GigaChatSDK.Interfaces;
using AiTreeServer.GigaChatSDK.Models;

namespace AiTreeServer.GigaChatSDK;

public class GigaChat(ITokenService tokenService, IHttpService httpService) : IGigaChat
{
    private const string BaseUrl = "https://gigachat.devices.sberbank.ru/api/v1/";

    private readonly ITokenService _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
    private readonly IHttpService _httpService = httpService ?? throw new ArgumentNullException(nameof(httpService));

    /// <summary>
    /// Отправление запроса к модели
    /// </summary>
    /// <returns>Response с ответом модели с учетом переданных сообщений..</returns>
    /// <param name="query">Запрос к модели в виде объекта запроса.</param>
    public async Task<Response?> CompletionsAsync(MessageQuery query)
    {
        await ValidateToken();

        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}chat/completions") 
        {
            Headers = { { "Authorization", $"Bearer {_tokenService.Token!.AccessToken}" } },
            Content = new StringContent(JsonSerializer.Serialize(query), Encoding.UTF8, "application/json")
        };

        string responseBody = await _httpService.SendAsync(request);
        return JsonSerializer.Deserialize<Response>(responseBody);
    }

    /// <summary>
    /// Отправление запроса к модели
    /// </summary>
    /// <returns>Response с ответом модели с учетом переданных сообщений..</returns>
    /// <param name="role">Роль собеседника</param>
    /// <param name="message">Запрос к модели в виде 1 строки с непосредственно с запросом.</param>
    public async Task<Response?> CompletionsAsync(string role, string message)
    {
        await ValidateToken();
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Message cannot be empty.", nameof(message));
        }

        var query = new MessageQuery();
        query.Messages.Add(new MessageContent(role, message));
        return await CompletionsAsync(query);
    }

    private async Task ValidateToken()
    {
        if (_tokenService.Token == null ||
            _tokenService.ExpiresAt < ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds())
        {
            await _tokenService.CreateTokenAsync();
        }
    }
}