namespace AiTreeServer.GigaChatSDK.Interfaces;

public interface IHttpService
{
    Task<string> SendAsync(HttpRequestMessage request);
    HttpClientHandler CreateHttpClientHandler(bool ignoreTls);
}