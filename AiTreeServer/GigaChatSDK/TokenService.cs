using System.Net.Http.Headers;
using System.Text.Json;
using AiTreeServer.GigaChatSDK.Interfaces;
using AiTreeServer.GigaChatSDK.Models;

namespace AiTreeServer.GigaChatSDK;

public class TokenService(IHttpService httpService, bool isCommercial) : ITokenService
{
    private const string TokenHost = "https://ngw.devices.sberbank.ru:9443/api/v2/oauth";
    private readonly IHttpService _httpService = httpService ?? throw new ArgumentNullException(nameof(httpService));

    /// <summary>
    /// Авторизационные данные
    /// </summary>
    private readonly string _secretKey = Environment.GetEnvironmentVariable("GIGACHAT_TOKEN") 
                                         ?? throw new ArgumentException("No GIGACHAT_TOKEN environment variable set.");

    /// <summary>
    /// Версия API, к которой предоставляется доступ. Нужное значение параметра scope вы найдете в личном кабинете после создания проекта.
    /// GIGACHAT_API_PERS — доступ для физических лиц.
    /// GIGACHAT_API_CORP — доступ для юридических лиц.
    /// </summary>
    private readonly bool _isCommercial = isCommercial;
    public long? ExpiresAt { get; private set; }
    public Token? Token { get; private set; }

    public async Task<Token> CreateTokenAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_secretKey))
            {
                throw new InvalidOperationException("Secret key is missing.");
            }

            var request = new HttpRequestMessage(HttpMethod.Post, TokenHost);
            request.Headers.Add("Authorization", "Bearer " + _secretKey);
            request.Headers.Add("RqUID", Guid.NewGuid().ToString());

            if (_isCommercial)
            {
                request.Content = new StringContent("scope=GIGACHAT_API_CORP");
            }
            else
            {
                request.Content = new StringContent("scope=GIGACHAT_API_PERS");
            }
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            string responseBody = await _httpService.SendAsync(request);
            Token = JsonSerializer.Deserialize<Token>(responseBody)
                    ?? throw new JsonException("Failed to deserialize token response.");

            ExpiresAt = Token.ExpiresAt;
            return Token;
        }
        catch (JsonException ex)
        {
            throw new ApplicationException("Ошибка десериализации ответа токена.", ex);
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Неизвестная ошибка при создании токена: {ex.Message}", ex);
        }
    }

    public void SetToken(Token token)
    {
        Token = token ?? throw new ArgumentNullException(nameof(token));
        ExpiresAt = token.ExpiresAt;
    }
}