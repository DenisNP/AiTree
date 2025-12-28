#include <FastLED.h>
#include <globals.h>

CRGB nextLeds[NUM_LEDS];
CRGB leds[NUM_LEDS];

extern void setLed(uint8_t index, CRGB color)
{
    leds[index] = CRGB(color.r, color.g, color.b);
}