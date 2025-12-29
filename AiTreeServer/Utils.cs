using System.Drawing;

namespace AiTreeServer;

public static class Utils
{
    public static string[] SortColorsByProximity(IEnumerable<string> colors)
    {
        // Сортируем по цветовому кругу (Hue), затем по насыщенности и светлоте
        string[] sorted = colors
            .Select(hex => new 
            { 
                Hex = hex, 
                Color = HexToColor(hex),
                Hsl = RgbToHsl(HexToColor(hex))
            })
            .OrderBy(c => c.Hsl.H)
            .ThenBy(c => c.Hsl.S)
            .ThenBy(c => c.Hsl.L)
            .Select(c => c.Hex)
            .ToArray();

        return sorted;
    }

    public static Color HexToColor(string hex)
    {
        hex = hex.Trim().Replace("#", "");

        if (hex.Length == 3) // краткая форма #RGB
        {
            hex = new string(new[] 
            { 
                hex[0], hex[0], 
                hex[1], hex[1], 
                hex[2], hex[2] 
            });
        }
        
        if (hex.Length == 6)
        {
            return Color.FromArgb(
                Convert.ToInt32(hex.Substring(0, 2), 16),
                Convert.ToInt32(hex.Substring(2, 2), 16),
                Convert.ToInt32(hex.Substring(4, 2), 16)
            );
        }
        
        throw new ArgumentException($"Некорректный HEX формат: {hex}");
    }

    private static (double H, double S, double L) RgbToHsl(Color color)
    {
        double r = color.R / 255.0;
        double g = color.G / 255.0;
        double b = color.B / 255.0;

        double max = Math.Max(r, Math.Max(g, b));
        double min = Math.Min(r, Math.Min(g, b));
        double delta = max - min;

        double h = 0;
        double s = 0;
        double l = (max + min) / 2.0;

        if (delta != 0)
        {
            s = l > 0.5 ? delta / (2 - max - min) : delta / (max + min);

            if (max == r)
                h = (g - b) / delta + (g < b ? 6 : 0);
            else if (max == g)
                h = (b - r) / delta + 2;
            else if (max == b)
                h = (r - g) / delta + 4;

            h /= 6;
        }

        return (h * 360, s, l);
    }
}