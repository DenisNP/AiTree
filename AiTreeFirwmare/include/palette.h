#pragma once

#include <FastLED.h>

extern CRGBPalette16 getCurrentPalette();
extern void setPalette(const CRGB* colors, uint8_t numColors);