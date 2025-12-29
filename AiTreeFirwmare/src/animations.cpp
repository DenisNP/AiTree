#include <FastLED.h>
#include <animations.h>
#include <globals.h>
#include <palette.h>
#include <../lib/PerlinsNoise/PerlinsNoise.h>

PerlinsNoise PNoise(654123);

// Скорость анимации
float speed = 5.0;

// Коэффициент масштабирования палитры
// 1.0 - вся палитра на всю ленту
// 0.5 - половина палитры растянута на всю ленту
// 2.0 - палитра повторяется 2 раза на ленте
float paletteScale = 1.0;

// Счетчик кадров для вычисления времени
static uint32_t frameCounter = 0;

// Счетчик кадров для анимации загрузки
static uint32_t loadingFrameCounter = 0;

extern void setParameters(uint8_t _speed, uint8_t scale)
{
    // Устанавливаем скорость
    if (_speed >= 1 && _speed <= 10)
    {
        speed = _speed * 1.0f;
    }

    // Устанавливаем масштаб палитры
    if (scale >= 1 && scale <= 10)
    {
        paletteScale = 5.0 - (scale - 1) * 0.5;
    }
}

extern void loading()
{
    // Увеличиваем счетчик кадров для загрузки
    loadingFrameCounter++;
    
    // Вычисляем текущее время в цикле
    float currentTime = (float)loadingFrameCounter / FRAMES_PER_SECOND;
    float cycleProgress = fmod(currentTime, LOADING_CYCLE_TIME) / LOADING_CYCLE_TIME;
    
    // Длина полоски
    const uint8_t stripLength = NUM_LEDS / 3;
    
    // Проверяем, в какой половине цикла мы находимся
    if (cycleProgress < 0.5)
    {
        // Первая половина - полоска пробегает по ленте
        float runProgress = cycleProgress * 2.0;  // От 0.0 до 1.0
        
        // Позиция центра полоски (от 0 до NUM_LEDS)
        int stripPosition = (int)(runProgress * (NUM_LEDS + stripLength)) - (stripLength / 2);
        
        // Рисуем полоску
        for (uint8_t i = 0; i < NUM_LEDS; i++)
        {
            // Проверяем, попадает ли этот светодиод в полоску
            if (i >= stripPosition && i < stripPosition + stripLength)
            {
                nextLeds[i] = CRGB::Red;
            }
            else
            {
                CRGB existing = leds[i];
                nextLeds[i] = CRGB(existing.r * FADE_RATIO * FADE_RATIO, existing.g * FADE_RATIO * FADE_RATIO, existing.b * FADE_RATIO * FADE_RATIO);
            }
        }
    }
    else
    {
        // Вторая половина - всё тёмное
        for (uint8_t i = 0; i < NUM_LEDS; i++)
        {
            CRGB existing = leds[i];
            nextLeds[i] = CRGB(existing.r * FADE_RATIO * FADE_RATIO, existing.g * FADE_RATIO * FADE_RATIO, existing.b * FADE_RATIO * FADE_RATIO);
        }
    }
}

extern void noise()
{
    // Увеличиваем счетчик кадров
    frameCounter++;

    // Вычисляем Y координату (смещение) на основе времени
    float yOffset = 0.0;

    // Текущее время с учетом начального смещения
    float currentTime = (float)frameCounter / FRAMES_PER_SECOND;

    // Y координата движется со временем
    yOffset = currentTime * speed * 0.3f;

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
