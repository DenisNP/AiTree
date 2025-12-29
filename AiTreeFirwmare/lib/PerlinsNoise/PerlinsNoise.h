#ifndef PERLINS_NOISE_H
#define PERLINS_NOISE_H

#include <stdint.h>

class PerlinsNoise {
public:
    // Constructor with seed
    PerlinsNoise(uint32_t seed = 0);

    // 2D Perlin noise
    float noise2D(float x, float y);
    
    // Multi-octave Perlin noise (fractal)
    // scale - масштаб шума (обычно 0.05-0.2)
    // octaves - количество слоев (обычно 3-6)
    // persistence - затухание амплитуды (обычно 0.5)
    // lacunarity - увеличение частоты (обычно 2.0)
    // contrast - коэффициент контрастности (обычно 2.0), результат обрезается в [-1, 1]
    float fractal2D(float x, float y, float scale = 0.05, int octaves = 4, float persistence = 0.3, float lacunarity = 2.0, float contrast = 2.5f);

private:
    uint32_t _seed; // Store the user-provided seed

    // Utility functions
    float fade(float t);
    float lerp(float a, float b, float t);
    float dotGridGradient(int ix, int iy, float x, float y);
    int hash(int x, int y);
    void getGradient(int hash, float& gradX, float& gradY);
};

#endif // PERLIN_NOISE_H