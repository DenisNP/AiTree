#include <FastLED.h>
#include <globals.h>
#include <animations.h>
#include <transitions.h>
#include <palette.h>
#include <network.h>

// Отрисовка кадра с учетом текущего состояния перехода
void animate()
{
    noise();

    if (transitionInProgress)
    {
        // Переход активен - применяем переход с чанками
        chunkedTransition();
    }
    else
    {
        // Переход не активен - мгновенное обновление
        noTransition();
    }
}

void setup()
{
    Serial.begin(9600);

    delay(1000);
    FastLED.addLeds<LED_TYPE, DATA_PIN, COLOR_ORDER>(leds, NUM_LEDS).setCorrection(TypicalLEDStrip);
    FastLED.setBrightness(BRIGHTNESS);
    // FastLED.setMaxPowerInVoltsAndMilliamps(5, 5000);

    // Настройка генератора случайных чисел
    random16_set_seed(5867);
    random16_add_entropy(analogRead(3));

    // запускаем сеть и получаем данные
    startNetwork();
    fetchData();

    // Запускаем первый паттерн
    startTransition();
}

void loop()
{
    // Замеряем время в начале итерации
    unsigned long frameStartTime = millis();
    
    // получаем данные из сети
    if (fetchData())
    {
        startTransition();
    }

    // Отрисовываем кадр с учетом перехода
    animate();

    // Отображаем на ленте
    FastLED.show();
    
    // Вычисляем сколько времени ушло на обработку
    unsigned long frameTime = millis() - frameStartTime;
    unsigned long targetFrameTime = 1000 / FRAMES_PER_SECOND;
    
    // Делаем паузу только на оставшееся время
    if (frameTime < targetFrameTime)
    {
        FastLED.delay(targetFrameTime - frameTime);
    }
}