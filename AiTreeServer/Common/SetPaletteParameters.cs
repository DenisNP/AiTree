using System.ComponentModel;

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