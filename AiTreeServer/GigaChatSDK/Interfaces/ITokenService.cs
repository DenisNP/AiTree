using AiTreeServer.GigaChatSDK.Models;

namespace AiTreeServer.GigaChatSDK.Interfaces;

public interface ITokenService
{
    Task<Token> CreateTokenAsync();
    long? ExpiresAt { get; }
    Token? Token { get; }
}