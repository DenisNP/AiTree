#include <FastLED.h>
#include <globals.h>

CRGB nextLeds[NUM_LEDS];
CRGB leds[NUM_LEDS];

// Флаг состояния загрузки
bool isLoadingMode = true;

extern void setLed(uint8_t index, CRGB color)
{
    leds[index] = CRGB(color.r * 0.5f, color.g * 0.5f, color.b * 0.5f);
}