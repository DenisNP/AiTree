#pragma once

#include <FastLED.h>

// установить скорость и масштабирование анимации палитры
extern void setParameters(uint8_t speed, uint8_t scale);

// Анимация на основе шума Перлина
extern void noise();

// Анимация загрузки
extern void loading();
