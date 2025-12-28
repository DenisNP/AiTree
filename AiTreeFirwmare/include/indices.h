#pragma once

#include <Arduino.h>
#include <globals.h>

extern uint8_t indices[NUM_LEDS];
extern void refillIndices();
extern void shuffleIndices(uint8_t chunkLen);