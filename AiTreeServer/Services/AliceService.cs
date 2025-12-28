using AiTreeServer.Alice;

namespace AiTreeServer.Services;

public class AliceService(BusService bus, ILogger<AliceService> logger)
{
    public AliceResponse HandleRequest(AliceRequest request)
    {
        if (request.HasIntent("delete"))
        {
            bus.DeleteCurrentPalette();
            var response = new AliceResponse(request)
            {
                Response =
                {
                    Text = "Удаляю текущую палитру",
                    EndSession = true
                }
            };

            return response;
        }
        
        if (request.Request.Command is { Length: > 0 })
        {
            string trimmedCommand = request.Request.Command
                .TrimStart("как")
                .TrimStart("словно")
                .TrimStart("будто")
                .Trim()
                .ToString();

            bus.Requests.Enqueue(trimmedCommand);
            logger.LogInformation("Request added: {TrimmedCommand}", trimmedCommand);

            var response = new AliceResponse(request)
            {
                Response =
                {
                    Text = "Зажигаю ёлку " + request.Request.Command,
                    EndSession = true
                }
            };

            return response;
        }
        else
        {
            var response = new AliceResponse(request)
            {
                Response =
                {
                    Text = "Не поняла вас",
                    EndSession = true
                }
            };

            return response;
        }
    }
}