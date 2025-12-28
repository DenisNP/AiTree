#include "PerlinsNoise.h"
#include <Arduino.h>

// Constructor with seed
PerlinsNoise::PerlinsNoise(uint32_t seed) {
    _seed = seed;
}

// Fade function for smoothing
float PerlinsNoise::fade(float t) {
    return t * t * t * (t * (t * 6 - 15) + 10);
}

// Linear interpolation
float PerlinsNoise::lerp(float a, float b, float t) {
    return a + t * (b - a);
}

// Hash function modified to use the seed (Arduino-compatible, optimized for ATmega328p)
int PerlinsNoise::hash(int x, int y) {
    return ((x * 91154 ^ y * 19143 ^ _seed) & 255);
}

// Get gradient vector based on hash value
void PerlinsNoise::getGradient(int hash, float& gradX, float& gradY) {
    int h = hash & 3; // 4 possible gradients in 2D
    gradX = (h & 1) == 0 ? 1.0 : -1.0;
    gradY = (h & 2) == 0 ? 1.0 : -1.0;
}

// Compute dot product at grid corner
float PerlinsNoise::dotGridGradient(int ix, int iy, float x, float y) {
    float gradX, gradY;
    getGradient(hash(ix, iy), gradX, gradY);

    float dx = x - ix;
    float dy = y - iy;

    return (dx * gradX + dy * gradY);
}

// Generate 2D Perlin noise
float PerlinsNoise::noise2D(float x, float y) {
    // Find grid cell coordinates
    int x0 = floor(x);
    int x1 = x0 + 1;
    int y0 = floor(y);
    int y1 = y0 + 1;

    // Compute fade curves for x and y
    float sx = fade(x - x0);
    float sy = fade(y - y0);

    // Interpolate dot products
    float n0, n1, ix0, ix1, value;

    n0 = dotGridGradient(x0, y0, x, y);
    n1 = dotGridGradient(x1, y0, x, y);
    ix0 = lerp(n0, n1, sx);

    n0 = dotGridGradient(x0, y1, x, y);
    n1 = dotGridGradient(x1, y1, x, y);
    ix1 = lerp(n0, n1, sx);

    value = lerp(ix0, ix1, sy);
    return value;
}

// Multi-octave fractal noise
float PerlinsNoise::fractal2D(float x, float y, float scale, int octaves, float persistence, float lacunarity, float contrast) {
    float total = 0.0;
    float frequency = 1.0;
    float amplitude = 1.0;
    float maxValue = 0.0;  // Для нормализации результата

    for(int i = 0; i < octaves; i++) {
        total += noise2D(x * frequency * scale, y * frequency * scale) * amplitude;

        maxValue += amplitude;

        amplitude *= persistence;
        frequency *= lacunarity;
    }

    // Нормализуем и применяем контрастность
    float result = (total / maxValue) * contrast;
    
    // Ограничиваем результат в диапазоне [-1, 1]
    if (result > 1.0) result = 1.0;
    if (result < -1.0) result = -1.0;
    
    return result;
}