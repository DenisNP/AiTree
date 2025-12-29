using System.ComponentModel;
using System.Drawing;

namespace AiTreeServer.Common;

public record SetPaletteParameters
{
    [Description("Список HEX-кодов цветов палитры, не менее 3 и не более 7 кодов")]
    public string[] Colors { get; init; } = [];
    [Description("Скорость анимации, от 1 (медленно) до 10 (быстро)")]
    public int Speed { get; init; }
    [Description("Масштабирование палитры на гирлянду от 1 (мелко) до 10 (крупно)")]
    public int Scale { get; init; }

    private string? _stringInterpretation;

    /// <summary>
    /// Вычисляет двузначный хэш от параметров палитры
    /// </summary>
    private int CalculateHash()
    {
        int hash = Colors.Length + Speed + Scale + Colors.Sum(color => color.GetHashCode() % 1000);
        return Math.Abs(hash) % 100;
    }

    /// <summary>
    /// Конвертирует HEX цвет в RGB каналы
    /// </summary>
    private static int[]? HexToChannels(string hex)
    {
        try
        {
            Color color = Utils.HexToColor(hex);
            return [color.R, color.G, color.B];
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Генерирует строку в формате: hash,numColors,r1,g1,b1,...,speed,scale
    /// Кэширует результат после первого вычисления
    /// </summary>
    public string ToStringInterpretation()
    {
        if (_stringInterpretation != null)
        {
            return _stringInterpretation;
        }
        
        int hash = CalculateHash();
        
        // Конвертируем все hex-цвета в значения по каналам
        var channels = new List<int>();
        foreach (string hexColor in Colors)
        {
            int[]? currentChannels = HexToChannels(hexColor);
            if (currentChannels is { Length: 3 })
            {
                channels.AddRange(currentChannels);
            }
        }
        
        int numColors = channels.Count / 3;
        
        _stringInterpretation = $"{hash},{numColors},{string.Join(",", channels)},{Speed},{Scale}";
        return _stringInterpretation;
    }
}