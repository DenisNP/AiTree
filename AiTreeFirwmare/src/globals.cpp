#include <FastLED.h>
#include <globals.h>

CRGB nextLeds[NUM_LEDS];
CRGB leds[NUM_LEDS];

// Флаг состояния загрузки
bool isLoadingMode = true;

extern void setLed(uint8_t index, CRGB color)
{
    /*CRGB next = CRGB(color.r, color.g, color.b);
    CRGB existing = leds[index];
    leds[index] = blend(existing, next, 50);*/
    leds[index] = CRGB(color.r, color.g, color.b);
}