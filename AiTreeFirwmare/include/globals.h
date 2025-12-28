#pragma once

#include <FastLED.h>

// Пин для подключения светодиодной ленты
#define DATA_PIN            5
// Тип светодиодной ленты
#define LED_TYPE            WS2812B
// Порядок цветов RGB
#define COLOR_ORDER         RGB

// Частота обновления (кадров в секунду)
#define FRAMES_PER_SECOND   120
// Количество светодиодов на ленте
#define NUM_LEDS            100
// Яркость светодиодов (0-255)
#define BRIGHTNESS          200

// Настройки WiFi
#define WIFI_SSID           "471"
#define WIFI_PASSWORD       "89052791441"
#define WIFI_TIMEOUT        10000  // Таймаут подключения в миллисекундах

// URL для получения данных
#define DATA_URL            "http://aitree.ispretty.fun:8080/palette"

// Интервал опроса сервера (в миллисекундах)
#define FETCH_INTERVAL      200

// Максимальная длина payload:
// код(1) + кол-во цветов(2) + цвета(16*3*3=144) + запятые(~50) + speed(2) + scale(2) + запас
#define MAX_PAYLOAD_LENGTH  256

// Коэффициент затухания для переходов
#define FADE_RATIO          0.97f

// =====
extern CRGB nextLeds[NUM_LEDS];
extern CRGB leds[NUM_LEDS];

extern void setLed(uint8_t index, CRGB color);

// Макрос для определения размера массива
#define ARRAY_SIZE(A) (sizeof(A) / sizeof((A)[0]))