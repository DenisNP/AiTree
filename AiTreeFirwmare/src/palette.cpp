#include <palette.h>
#include <FastLED.h>
#include <globals.h>

// Инициализируем палитру радугой
CRGBPalette16 currentPalette = RainbowColors_p;

extern CRGBPalette16 getCurrentPalette() {
    return currentPalette;
}

extern void setPalette(const CRGB* colors, uint8_t numColors)
{
    if (numColors < 1) numColors = 1;
    if (numColors > 16) numColors = 16;

    /* средние цвета делаем чаще чем крайние, потому что серого 
     * в нормализованном шуме меньше чем белого и чёрного;
     * Наверное это можно было бы сделать каким-то красивым алгоритмом,
     * но я пишу этот код ночью 29 декабря и слегка замучился
    */
    switch (numColors)
    {
    case 1:
        // Все 16 слотов один цвет
        for (uint8_t i = 0; i < 16; i++) currentPalette[i] = colors[0];
        break;
    
    case 2:
        // Равномерно: каждый цвет по 8 слотов
        // [C0,C0,C0,C0,C0,C0,C0,C0, C1,C1,C1,C1,C1,C1,C1,C1]
        for (uint8_t i = 0; i < 8; i++) currentPalette[i] = colors[0];
        for (uint8_t i = 8; i < 16; i++) currentPalette[i] = colors[1];
        break;
    
    case 3:
        // [C0,C0,C0,C0,C0, C1,C1,C1,C1,C1,C1, C2,C2,C2,C2,C2]
        for (uint8_t i = 0; i < 5; i++) currentPalette[i] = colors[0];
        for (uint8_t i = 5; i < 11; i++) currentPalette[i] = colors[1];
        for (uint8_t i = 11; i < 16; i++) currentPalette[i] = colors[2];
        break;
    
    case 4:
        // Равномерно: каждый цвет по 4 слота
        // [C0,C0,C0,C0, C1,C1,C1,C1, C2,C2,C2,C2, C3,C3,C3,C3]
        for (uint8_t i = 0; i < 4; i++) currentPalette[i] = colors[0];
        for (uint8_t i = 4; i < 8; i++) currentPalette[i] = colors[1];
        for (uint8_t i = 8; i < 12; i++) currentPalette[i] = colors[2];
        for (uint8_t i = 12; i < 16; i++) currentPalette[i] = colors[3];
        break;
    
    case 5:
        // [C0,C0,C0, C1,C1,C1, C2,C2,C2,C2, C3,C3,C3, C4,C4,C4]
        for (uint8_t i = 0; i < 3; i++) currentPalette[i] = colors[0];
        for (uint8_t i = 3; i < 6; i++) currentPalette[i] = colors[1];
        for (uint8_t i = 6; i < 10; i++) currentPalette[i] = colors[2];
        for (uint8_t i = 10; i < 13; i++) currentPalette[i] = colors[3];
        for (uint8_t i = 13; i < 16; i++) currentPalette[i] = colors[4];
        break;
    
    case 6:
        // [C0,C0, C1,C1,C1, C2,C2,C2, C3,C3,C3, C4,C4,C4, C5,C5]
        for (uint8_t i = 0; i < 2; i++) currentPalette[i] = colors[0];
        for (uint8_t i = 2; i < 5; i++) currentPalette[i] = colors[1];
        for (uint8_t i = 5; i < 8; i++) currentPalette[i] = colors[2];
        for (uint8_t i = 8; i < 11; i++) currentPalette[i] = colors[3];
        for (uint8_t i = 11; i < 14; i++) currentPalette[i] = colors[4];
        for (uint8_t i = 14; i < 16; i++) currentPalette[i] = colors[5];
        break;
    
    case 7:
        // [C0,C0, C1,C1, C2,C2,C2, C3,C3,C3, C4,C4, C5,C5, C6,C6]
        for (uint8_t i = 0; i < 2; i++) currentPalette[i] = colors[0];
        for (uint8_t i = 2; i < 4; i++) currentPalette[i] = colors[1];
        for (uint8_t i = 4; i < 7; i++) currentPalette[i] = colors[2];
        for (uint8_t i = 7; i < 10; i++) currentPalette[i] = colors[3];
        for (uint8_t i = 10; i < 12; i++) currentPalette[i] = colors[4];
        for (uint8_t i = 12; i < 14; i++) currentPalette[i] = colors[5];
        for (uint8_t i = 14; i < 16; i++) currentPalette[i] = colors[6];
        break;
    
    default:
        // Для 8+ цветов используем циклическое повторение
        for (uint8_t i = 0; i < 16; i++)
        {
            currentPalette[i] = colors[i % numColors];
        }
        break;
    }
}