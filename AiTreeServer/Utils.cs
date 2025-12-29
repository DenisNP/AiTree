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

    public static string AdjustColorIfHighSaturation(string hex)
    {
        const double saturationThreshold = 0.5;
        const double lightnessReduction = 0.2;

        Color color = HexToColor(hex);
        (double h, double s, double l) = RgbToHsl(color);

        if (s <= saturationThreshold)
        {
            return hex;
        }

        double newLightness = l * (1 - lightnessReduction);
        newLightness = Math.Max(0, Math.Min(1, newLightness));

        Color adjustedColor = HslToRgb(h, s, newLightness);
        return ColorToHex(adjustedColor);
    }

    private static Color HslToRgb(double h, double s, double l)
    {
        // Нормализуем hue в диапазоне 0-360
        h %= 360;
        if (h < 0)
        {
            h += 360;
        }

        // Если насыщенность равна 0, цвет серый
        if (s == 0)
        {
            int gray = (int)(l * 255);
            return Color.FromArgb(gray, gray, gray);
        }

        double c = (1 - Math.Abs(2 * l - 1)) * s;
        double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
        double m = l - c / 2;

        double r1 = 0, g1 = 0, b1 = 0;

        if (h is >= 0 and < 60)
        {
            r1 = c;
            g1 = x;
            b1 = 0;
        }
        else if (h is >= 60 and < 120)
        {
            r1 = x;
            g1 = c;
            b1 = 0;
        }
        else if (h is >= 120 and < 180)
        {
            r1 = 0;
            g1 = c;
            b1 = x;
        }
        else if (h is >= 180 and < 240)
        {
            r1 = 0;
            g1 = x;
            b1 = c;
        }
        else if (h is >= 240 and < 300)
        {
            r1 = x;
            g1 = 0;
            b1 = c;
        }
        else // 300-360
        {
            r1 = c;
            g1 = 0;
            b1 = x;
        }

        int r = (int)((r1 + m) * 255);
        int g = (int)((g1 + m) * 255);
        int b = (int)((b1 + m) * 255);

        // Ограничиваем значения 0-255
        r = Math.Max(0, Math.Min(255, r));
        g = Math.Max(0, Math.Min(255, g));
        b = Math.Max(0, Math.Min(255, b));

        return Color.FromArgb(r, g, b);
    }

    private static string ColorToHex(Color color)
    {
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
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

    public static string RemovePrefixIfExists(this string input, params string[] prefixes)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        foreach (string prefix in prefixes)
        {
            if (input.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return input.Substring(prefix.Length);
            }
        }

        return input.Trim();
    }
}