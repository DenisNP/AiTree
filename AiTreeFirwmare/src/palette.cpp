#include <palette.h>
#include <FastLED.h>

// Инициализируем палитру радугой
CRGBPalette16 currentPalette = RainbowColors_p;

extern CRGBPalette16 getCurrentPalette() {
    return currentPalette;
}

extern void setPalette(const CRGB* colors, uint8_t numColors)
{
    if (numColors < 1) numColors = 1;
    if (numColors > 16) numColors = 16;
    
    // Заполняем палитру повторением цветов без градиентов
    for (uint8_t i = 0; i < 16; i++)
    {
        // Циклически выбираем цвет из массива
        currentPalette[i] = colors[i % numColors];
    }
}