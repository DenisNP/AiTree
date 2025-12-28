#pragma once

#include <FastLED.h>

// Флаг: идёт ли переход в данный момент
extern bool transitionInProgress;

// Время перехода в секундах
extern float transitionTime;

// Размер чанка для текущего перехода
extern uint8_t transitionChunkSize;

// Запуск перехода
extern void startTransition();

// Без перехода - сразу копирует nextLeds в leds
extern void noTransition();

// Переход с чанками - лента постепенно заполняется в порядке indices
// Направление зависит от transitionDirection
extern void chunkedTransition();
