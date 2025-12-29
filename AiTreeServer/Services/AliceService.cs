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

        if (request.IsEnter())
        {
            var response = new AliceResponse(request)
            {
                Response =
                {
                    Text = "Опишите сцену"
                }
            };

            return response;
        }

        if (request.Request.Command is { Length: > 0 })
        {
            string withoutFirstPrefix = request.Request.Command.RemovePrefixIfExists("светиться", "гореть");
            string trimmedCommand = withoutFirstPrefix.RemovePrefixIfExists("как", "словно", "будто");

            bus.EnqueueCommand(trimmedCommand);
            logger.LogInformation("Request added: {TrimmedCommand}", trimmedCommand);

            var response = new AliceResponse(request)
            {
                Response =
                {
                    Text = "Хорошо. Зажигаю ёлку как " + trimmedCommand,
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