#include <FastLED.h>
#include <animations.h>
#include <globals.h>
#include <palette.h>
#include <../lib/PerlinsNoise/PerlinsNoise.h>

PerlinsNoise PNoise(654123);

// Время в секундах для полного оборота палитры
float rotationTime = 10.0;

// Коэффициент масштабирования палитры
// 1.0 - вся палитра на всю ленту
// 0.5 - половина палитры растянута на всю ленту
// 2.0 - палитра повторяется 2 раза на ленте
float paletteScale = 1.0;

// Счетчик кадров для вычисления времени
static uint32_t frameCounter = 0;

extern void setParameters(uint8_t speed, uint8_t scale)
{
    // Устанавливаем скорость (rotationTime)
    // speed от 1 (медленно) до 10 (быстро)
    // 1 -> 20 секунд, 10 -> 2 секунды
    if (speed >= 1 && speed <= 10)
    {
        rotationTime = 20.0 - (speed - 1) * 2.0;
    }
    
    // Устанавливаем масштаб палитры (paletteScale)
    // scale от 1 до 10
    // 1 -> 0.25, 10 -> 3.0
    if (scale >= 1 && scale <= 10)
    {
        paletteScale = 0.25 + (scale - 1) * 0.3;
    }
}

extern void noise()
{
    // Увеличиваем счетчик кадров
    frameCounter++;
    
    // Вычисляем Y координату (смещение) на основе времени
    float yOffset = 0.0;
    if (rotationTime != 0.0)
    {
        // Текущее время с учетом начального смещения
        float currentTime = (float)frameCounter / FRAMES_PER_SECOND;
        
        // Y координата движется со временем
        yOffset = currentTime / rotationTime;
    }
    
    for (uint8_t i = 0; i < NUM_LEDS; i++)
    {
        // X координата = позиция диода
        float xCoord = (float)i * paletteScale;

        // Получаем значение шума Перлина (от -1 до 1)
        // Y координата = смещение для анимации
        float noiseValue = PNoise.fractal2D(xCoord, yOffset);
        
        // Преобразуем в индекс палитры (от 0 до 65535)
        uint16_t paletteIndex = (uint16_t)((noiseValue + 1.0) * 32767.5);
        
        // Получаем цвет из текущей палитры
        CRGB color = ColorFromPaletteExtended(getCurrentPalette(), paletteIndex, 255, TBlendType::LINEARBLEND);
        
        // Пишем в nextLeds
        nextLeds[i] = color;
    }
}

