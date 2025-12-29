#include <FastLED.h>
#include <globals.h>
#include <animations.h>
#include <transitions.h>
#include <palette.h>
#include <network.h>

// Отрисовка кадра с учетом текущего состояния перехода
void animate()
{
    if (isLoadingMode)
    {
        loading();
        noTransition();
        return;
    }

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
    Serial.begin(115200);

    delay(1000);
    FastLED.addLeds<LED_TYPE, DATA_PIN, COLOR_ORDER>(leds, NUM_LEDS);
    FastLED.setBrightness(BRIGHTNESS);
    // FastLED.setMaxPowerInVoltsAndMilliamps(5, 5000);

    fill_solid(leds, NUM_LEDS, CRGB::Blue);
    FastLED.show();

    // Настройка генератора случайных чисел
    random16_set_seed(5867);
    random16_add_entropy(analogRead(3));

    // запускаем сеть
    startNetwork();

    fill_solid(leds, NUM_LEDS, CRGB::Green);
    FastLED.show();
    delay(1000);

    // Запускаем задачу для периодического получения данных
    startNetworkTask();

    // Запускаем первый паттерн
    startTransition();
}

void loop()
{
    // Замеряем время в начале итерации
    unsigned long frameStartTime = millis();

    // Проверяем очередь на наличие новых данных из сети
    if (checkNetworkData())
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