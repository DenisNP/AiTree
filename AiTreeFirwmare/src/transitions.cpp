#include <FastLED.h>
#include <transitions.h>
#include <globals.h>
#include <indices.h>

// Флаг: идёт ли переход в данный момент
bool transitionInProgress = false;

// Время перехода в секундах
float transitionTime = 2.0;

// Направление перехода
// 1 = от начала к концу, -1 = от конца к началу
int8_t transitionDirection = 1;

// Размер чанка для текущего перехода
uint8_t transitionChunkSize = NUM_LEDS / 5;

// Счетчик кадров для перехода
static uint32_t transitionFrameCounter = 0;

// Запуск перехода
extern void startTransition()
{
    // Генерируем перемешанные индексы на основе размера чанка
    shuffleIndices(transitionChunkSize);
    
    transitionInProgress = true;
    transitionFrameCounter = 0;
}

// Без перехода - сразу копируем nextLeds в leds
extern void noTransition()
{
    for (uint8_t i = 0; i < NUM_LEDS; i++)
    {
        setLed(i, nextLeds[i]);
    }
}

// Переход с чанками - лента постепенно заполняется в порядке indices
extern void chunkedTransition()
{
    if (!transitionInProgress)
    {
        // Переход не запущен - ничего не делаем
        return;
    }
    
    transitionFrameCounter++;
    
    // Вычисляем прогресс перехода
    float totalFrames = FRAMES_PER_SECOND * transitionTime;
    float progress = (float)transitionFrameCounter / totalFrames;
    
    if (progress >= 1.0)
    {
        // Переход завершён - копируем все диоды
        for (uint8_t i = 0; i < NUM_LEDS; i++)
        {
            setLed(i, nextLeds[i]);
        }
        transitionInProgress = false;
        transitionFrameCounter = 0;
    }
    else
    {
        // Постепенно заполняем диоды в порядке indices
        uint8_t ledsToFill = (uint8_t)(progress * NUM_LEDS);

        for (uint8_t i = 0; i < NUM_LEDS; i++)
        {
            uint8_t ledIndex = indices[i];
            if ((transitionDirection >= 0 && i < ledsToFill) || (transitionDirection < 0 && i >= NUM_LEDS - ledsToFill))
            {
                setLed(ledIndex, nextLeds[ledIndex]);
            }
            // Затухаем остальные диоды если переход идёт не линейный а кусками, потому что иначе в середине ленты не видно новых светодиодов
            else if (transitionChunkSize < NUM_LEDS)
            {
                CRGB existing = leds[ledIndex];
                setLed(ledIndex, CRGB(existing.r * FADE_RATIO, existing.g * FADE_RATIO, existing.b * FADE_RATIO));
            }
        }
    }
}

