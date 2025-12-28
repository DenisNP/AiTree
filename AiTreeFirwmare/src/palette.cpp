#include <palette.h>
#include <FastLED.h>

CRGBPalette16 currentPalette;

extern CRGBPalette16 getCurrentPalette() {
    return currentPalette;
}

extern void setPalette(const CRGB* colors, uint8_t numColors)
{
    if (numColors < 2) numColors = 2;
    if (numColors > 16) numColors = 16;
    
    // Заполняем палитру с интерполяцией между цветами
    if (numColors == 2) {
        fill_gradient_RGB(currentPalette, 0, colors[0], 15, colors[1]);
    } else {
        uint8_t step = 16 / (numColors - 1);
        for (uint8_t i = 0; i < numColors - 1; i++) {
            fill_gradient_RGB(currentPalette, i * step, colors[i], (i + 1) * step, colors[i + 1]);
        }
    }
}