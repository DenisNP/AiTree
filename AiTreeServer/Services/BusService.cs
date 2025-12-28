using System.Collections.Concurrent;
using AiTreeServer.Common;

namespace AiTreeServer.Services;

public class BusService
{
    /// <summary>
    /// Радужная палитра по умолчанию (7 цветов радуги)
    /// </summary>
    private const string DefaultRainbowResponse = "0,7,255,0,0,255,127,0,255,255,0,0,255,0,0,255,255,0,0,255,127,0,255,5,5";

    private readonly Lock _lock = new();
    private readonly List<SetPaletteParameters> _paletteHistory = new();
    private int _currentIndex = -1;

    public ConcurrentQueue<string> Requests { get; } = new();

    /// <summary>
    /// Добавляет палитру в историю и делает её текущей
    /// </summary>
    public void AddPalette(SetPaletteParameters parameters)
    {
        lock (_lock)
        {
            _paletteHistory.Insert(0, parameters);
            _currentIndex = 0;
        }
    }

    /// <summary>
    /// Получает текущие параметры
    /// </summary>
    public SetPaletteParameters? PeekParameters()
    {
        lock (_lock)
        {
            if (_currentIndex >= 0 && _currentIndex < _paletteHistory.Count)
            {
                return _paletteHistory[_currentIndex];
            }
            return null;
        }
    }

    /// <summary>
    /// Получает текущую строку ответа: из истории или радугу по умолчанию
    /// </summary>
    public string GetCurrentResponse()
    {
        SetPaletteParameters? parameters = PeekParameters();
        return parameters?.ToStringInterpretation() ?? DefaultRainbowResponse;
    }

    /// <summary>
    /// Устанавливает текущей случайную палитру из ранее полученных
    /// </summary>
    public void SetRandom()
    {
        lock (_lock)
        {
            if (_paletteHistory.Count == 0)
            {
                _currentIndex = -1;
                return;
            }
            
            var random = new Random();
            _currentIndex = random.Next(0, _paletteHistory.Count);
        }
    }

    /// <summary>
    /// Получает количество палитр в истории
    /// </summary>
    public int GetHistoryCount()
    {
        lock (_lock)
        {
            return _paletteHistory.Count;
        }
    }

    /// <summary>
    /// Удалить текущую палитру
    /// </summary>
    public void DeleteCurrentPalette()
    {
        if (_currentIndex < 0)
        {
            return;
        }

        lock (_lock)
        {
            _paletteHistory.RemoveAt(_currentIndex);
            SetRandom();
        }
    }
}