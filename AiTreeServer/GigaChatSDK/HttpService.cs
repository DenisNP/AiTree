using AiTreeServer.GigaChatSDK.Interfaces;

namespace AiTreeServer.GigaChatSDK;

public class HttpService : IHttpService
{
    private readonly HttpClient _client;

    public HttpService(bool ignoreTls)
    {
        _client = new HttpClient(CreateHttpClientHandler(ignoreTls));
    }

    /// <param name="ignoreTls">true - включает игнорирование сертификатов безопасности.
    /// Необходимо для систем имеющих проблемы с сертификатами МинЦифр.</param>
    public HttpClientHandler CreateHttpClientHandler(bool ignoreTls)
    {
        var handler = new HttpClientHandler();
        if (ignoreTls)
        {
            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
        }
        return handler;
    }

    public async Task<string> SendAsync(HttpRequestMessage request)
    {
        try
        {
            HttpResponseMessage response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException ex)
        {
            throw new ApplicationException($"Ошибка при отправке HTTP запроса: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Неизвестная ошибка: {ex.Message}", ex);
        }
    }
}