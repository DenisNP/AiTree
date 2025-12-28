using System.Text.Json;
using AiTreeServer.Alice;
using AiTreeServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace AiTreeServer.Controllers;

[ApiController]
[Route("[controller]")]
public class AliceController(AliceService aliceService) : ControllerBase
{
    private readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNamingPolicy =  JsonNamingPolicy.SnakeCaseLower
    };

    [HttpPost]
    public Task Post()
    {
        using var reader = new StreamReader(Request.Body);
        string body = reader.ReadToEnd();

        var request = JsonSerializer.Deserialize<AliceRequest>(body, _jsonOpts);

        if (request == null)
        {
            Console.WriteLine("Request is null:");
            Console.WriteLine(body);
            return Response.WriteAsync("Request is null");
        }
        
        AliceResponse response = request.IsPing()
            ? new AliceResponse(request).ToPong()
            : aliceService.HandleRequest(request);

        string stringResponse = JsonSerializer.Serialize(response, _jsonOpts);
        return Response.WriteAsync(stringResponse);
    }
}