using AiTreeServer.Common;

namespace AiTreeServer.Services;

public class PaletteService(AiService aiService, BusService bus, ILogger<PaletteService> logger) : BackgroundService
{
    private readonly Random _random = new();
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Palette service started");

        // Запускаем таймер для случайных палитр
        _ = Task.Run(async () => await RandomPaletteLoop(stoppingToken), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ParseNextRequest();
            } catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred processing request");
            }
        }
    }

    private async Task RandomPaletteLoop(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Ждём случайное время от 1 до 2 минут
            int delaySeconds = _random.Next(60, 121);
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
            
            if (!stoppingToken.IsCancellationRequested && bus.GetHistoryCount() > 0)
            {
                bus.SetRandom();
                // logger.LogInformation("Random palette set from history ({Count} total)", bus.GetHistoryCount());
            }
        }
    }

    private async Task ParseNextRequest()
    {
        if (bus.Requests.TryDequeue(out string request))
        {
            logger.LogInformation("Request got: {Request}", request);

            SetPaletteParameters? response = await aiService.AskChatForPalette(request);
            if (response != null)
            {
                logger.LogInformation(
                    "Response got: {Colors}, {Speed}, {Scale}",
                    string.Join(",", response.Colors), response.Speed, response.Scale
                );

                // если цвет один, делаем палитру из двух одинаковых, т.к. реализация чтения палитры ожидает от двух
                if (response.Colors.Length == 1)
                {
                    response = response with { Colors = [response.Colors[0], response.Colors[0]] };
                }

                // если получили достаточное число цветов, добавляем в историю
                if (response.Colors.Length >= 2)
                {
                    bus.AddPalette(response);
                    
                    logger.LogInformation("Palette added to history, total: {Count}", bus.GetHistoryCount());
                    logger.LogInformation("Current response: {CurrentResponse}", bus.GetCurrentResponse());
                }
                else
                {
                    logger.LogInformation("Response wasn't modified: insufficient colors");
                }
            }
        }
    }
}