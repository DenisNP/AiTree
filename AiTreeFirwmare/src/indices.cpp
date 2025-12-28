#include <Arduino.h>
#include <globals.h>
#include <indices.h>

uint8_t indices[NUM_LEDS];

extern void refillIndices()
{
    for (uint8_t i = 0; i < NUM_LEDS; i++)
    {
        indices[i] = i;
    }
}

extern void shuffleIndices(uint8_t chunkLen)
{
    if (chunkLen == 0 || NUM_LEDS < chunkLen)
        return;

    refillIndices();

    if (chunkLen > NUM_LEDS / 2)
        return; // no shuffle, [almost] whole strip is a chunk

    uint8_t numChunks = NUM_LEDS / chunkLen;

    for (uint8_t i = 0; i < numChunks; i++)
    {
        uint8_t j = random8(numChunks);
        while (j == i)
        {
            j = random8(numChunks);
        }

        for (uint8_t k = 0; k < chunkLen; k++)
        {
            uint8_t temp = indices[i * chunkLen + k];
            indices[i * chunkLen + k] = indices[j * chunkLen + k];
            indices[j * chunkLen + k] = temp;
        }
    }
}