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

// Основная функция запуска следующего паттерна
void startNextPattern()
{

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
    startNextPattern();
}

void loop()
{
    // получаем данные из сети
    fetchData();

    // Отрисовываем кадр с учетом перехода
    animate();

    // Отображаем на ленте
    FastLED.show();
    FastLED.delay(1000 / FRAMES_PER_SECOND);
}