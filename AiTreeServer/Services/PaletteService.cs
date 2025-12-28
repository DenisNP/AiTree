using AiTreeServer.Common;

namespace AiTreeServer.Services;

public class PaletteService(AiService aiService, BusService bus, ILogger<PaletteService> logger) : BackgroundService
{
    private int _lastResponseCode = 0;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Palette service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            await ParseNextRequest();
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
                
                bus.LastParameters = response;

                // если цвет один, делаем палитру из двух одинаковых, т.к. реализация чтения палитры ожидает от двух
                if (response.Colors.Length == 1)
                {
                    response = response with { Colors = [response.Colors[0], response.Colors[0]] };
                }

                // сконвертируем все hex-цвета в значения по каналам
                var channels = new List<int>();
                foreach (string hexColor in response.Colors)
                {
                    int[]? currentChannels = HexToChannels(hexColor);
                    if (currentChannels is { Length: 3 })
                    {
                        channels.AddRange(currentChannels);
                    }
                }

                // если получили правильное и достаточное число цветов, перезапишем текущий ответ
                if (channels.Count >= 6 && channels.Count % 3 == 0)
                {
                    int numColors = channels.Count / 3;
                    _lastResponseCode = 1 - _lastResponseCode;
                    bus.CurrentResponse = $"{_lastResponseCode},{numColors},{string.Join(",", channels)},{response.Speed},{response.Scale}";
                    
                    logger.LogInformation("Current response: {CurrentResponse}", bus.CurrentResponse);
                }
                else
                {
                    logger.LogInformation("Response wasn't modified");
                }
            }
        }
    }

    private int[]? HexToChannels(string hex)
    {
        // Удаляем символ # если есть
        hex = hex.TrimStart('#');
    
        // Проверяем корректность длины
        if (hex.Length != 6)
        {
            return null;
        }

        try
        {
            // Преобразуем каждые два символа в число
            var r = Convert.ToInt32(hex[..2], 16);
            var g = Convert.ToInt32(hex[2..4], 16);
            var b = Convert.ToInt32(hex[4..6], 16);

            return [r, g, b];
        }
        catch
        {
            return null;
        }
    }
}