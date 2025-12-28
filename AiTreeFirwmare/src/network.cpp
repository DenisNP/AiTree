#include <WiFi.h>
#include <HTTPClient.h>
#include <network.h>
#include <palette.h>
#include <animations.h>
#include <freertos/FreeRTOS.h>
#include <freertos/task.h>
#include <freertos/queue.h>

// Структура для передачи данных через очередь
struct PayloadData {
    char payload[MAX_PAYLOAD_LENGTH];
};

// Очередь для передачи данных
static QueueHandle_t dataQueue = NULL;

// Внутренние функции (не экспортируются наружу)

// Инициализация WiFi модуля
static void initWiFi()
{
    WiFi.mode(WIFI_STA);
    WiFi.disconnect();
    delay(100);
}

// Подключение к WiFi сети
static bool connectToWiFi()
{
    if (Serial)
    {
        Serial.println("Подключение к WiFi...");
        Serial.print("SSID: ");
        Serial.println(WIFI_SSID);
    }

    WiFi.begin(WIFI_SSID, WIFI_PASSWORD);

    unsigned long startAttemptTime = millis();

    // Ожидание подключения с таймаутом
    while (WiFi.status() != WL_CONNECTED && millis() - startAttemptTime < WIFI_TIMEOUT)
    {
        delay(500);
        if (Serial)
        {
            Serial.print(".");
        }
    }

    if (WiFi.status() == WL_CONNECTED)
    {
        if (Serial)
        {
            Serial.println();
            Serial.println("WiFi подключен!");
            Serial.print("IP адрес: ");
            Serial.println(WiFi.localIP());
        }
        return true;
    }
    else
    {
        if (Serial)
        {
            Serial.println();
            Serial.println("Не удалось подключиться к WiFi");
        }
        return false;
    }
}

// Внутренние переменные для хранения последних данных
static int lastDataCode = -1;  // -1 означает что данных еще не было
static String lastDataString = "";

// Парсинг цветов из строки данных
// Возвращает позицию после последнего распарсенного цвета
static int parseColors(const String& payload, int startPos, uint8_t numColors)
{
    if (numColors > 16)
    {
        return startPos;
    }
    
    if (Serial)
    {
        Serial.print("Количество цветов: ");
        Serial.println(numColors);
    }
    
    // Создаем массив для цветов
    CRGB colors[16];
    int pos = startPos;
    int nextComma;
    
    // Парсим каждый цвет (R,G,B)
    for (uint8_t i = 0; i < numColors; i++)
    {
        // Парсим R
        nextComma = payload.indexOf(',', pos);
        if (nextComma < 0 && i < numColors - 1) break;  // Недостаточно данных
        
        uint8_t r = (nextComma > 0) ? payload.substring(pos, nextComma).toInt() : payload.substring(pos).toInt();
        pos = nextComma + 1;
        
        // Парсим G
        nextComma = payload.indexOf(',', pos);
        if (nextComma < 0 && i < numColors - 1) break;
        
        uint8_t g = (nextComma > 0) ? payload.substring(pos, nextComma).toInt() : payload.substring(pos).toInt();
        pos = nextComma + 1;
        
        // Парсим B
        nextComma = payload.indexOf(',', pos);
        uint8_t b = (nextComma > 0) ? payload.substring(pos, nextComma).toInt() : payload.substring(pos).toInt();
        
        if (nextComma > 0)
        {
            pos = nextComma + 1;
        }
        
        colors[i] = CRGB(r, g, b);
    }
    
    // Применяем новую палитру
    setPalette(colors, numColors);
    
    return pos;
}

// Парсинг параметров speed и scale
static void parseParameters(const String& payload, int startPos)
{
    if (startPos >= payload.length())
    {
        return;
    }
    
    int pos = startPos;
    int nextComma = payload.indexOf(',', pos);
    
    uint8_t speed = 5;  // Значение по умолчанию
    uint8_t scale = 1;  // Значение по умолчанию
    
    if (nextComma > 0)
    {
        speed = payload.substring(pos, nextComma).toInt();
        pos = nextComma + 1;
        
        // Парсим scale
        if (pos < payload.length())
        {
            scale = payload.substring(pos).toInt();
        }
    }
    else if (pos < payload.length())
    {
        // Только одно число (speed)
        speed = payload.substring(pos).toInt();
    }
    
    // Устанавливаем параметры
    setParameters(speed, scale);
    
    if (Serial)
    {
        Serial.print("Установлены параметры: speed=");
        Serial.print(speed);
        Serial.print(", scale=");
        Serial.println(scale);
    }
}

// Публичные функции

// Запуск сетевого модуля
extern void startNetwork()
{
    initWiFi();
    connectToWiFi();
}

// Получение данных с сервера (внутренняя функция)
// Возвращает payload если данные изменились, иначе пустую строку
static String fetchData()
{
    // Проверяем подключение к WiFi
    if (WiFi.status() != WL_CONNECTED)
    {
        return "";
    }

    HTTPClient http;
    http.begin(DATA_URL);
    
    int httpCode = http.GET();
    
    if (httpCode == HTTP_CODE_OK)
    {
        String payload = http.getString();
        payload.trim();

        // Парсим первое число (код)
        int commaIndex = payload.indexOf(',');
        int currentCode;
        
        if (commaIndex > 0)
        {
            currentCode = payload.substring(0, commaIndex).toInt();
        }
        else
        {
            // Если запятой нет, значит это одно число
            currentCode = payload.toInt();
        }
        
        // Проверяем изменился ли код
        if (currentCode != lastDataCode)
        {                       
            lastDataCode = currentCode;
            lastDataString = payload;

            if (Serial)
            {
                Serial.println("Получены новые данные: ");
                Serial.println(payload);
            }

            http.end();
            return payload;  // Данные изменились - возвращаем payload
        }
        else
        {
            http.end();
            return "";  // Данные те же
        }
    }
    else
    {
        http.end();
        return "";
    }
}

// Парсинг payload и применение настроек (вызывается в основном потоке)
static void parsePayload(const String& payload)
{
    // Парсим первое число (код) - уже проверено в fetchData
    int commaIndex = payload.indexOf(',');
    
    if (commaIndex <= 0)
    {
        return;
    }
    
    // Парсим остальные данные - количество цветов и палитру
    int pos = commaIndex + 1;
    int nextComma = payload.indexOf(',', pos);
    
    if (nextComma > 0)
    {
        // Получаем количество цветов
        uint8_t numColors = payload.substring(pos, nextComma).toInt();

        if (numColors > 0 && numColors <= 16)
        {
            // Парсим цвета
            pos = parseColors(payload, nextComma + 1, numColors);
            
            // Парсим параметры speed и scale
            parseParameters(payload, pos);
        }
    }
}

// Задача FreeRTOS для периодического получения данных
static void networkTask(void* parameter)
{
    TickType_t lastFetchTime = xTaskGetTickCount();
    
    while (true)
    {
        // Ждем интервал перед следующим запросом
        vTaskDelayUntil(&lastFetchTime, pdMS_TO_TICKS(FETCH_INTERVAL));
        
        // Пытаемся получить данные
        String payload = fetchData();
        
        if (payload.length() > 0 && payload.length() < MAX_PAYLOAD_LENGTH)
        {
            // Данные изменились - отправляем payload в очередь
            PayloadData data;
            payload.toCharArray(data.payload, MAX_PAYLOAD_LENGTH);
            xQueueSend(dataQueue, &data, 0);
        }
    }
}

// Запуск задачи для периодического получения данных
extern void startNetworkTask()
{
    // Создаем очередь на 1 элемент с данными PayloadData
    dataQueue = xQueueCreate(1, sizeof(PayloadData));

    if (dataQueue == NULL)
    {
        if (Serial)
        {
            Serial.println("Ошибка создания очереди!");
        }
        return;
    }
    
    // Создаем задачу на ядре 0 (чтобы не мешать основному циклу на ядре 1)
    xTaskCreatePinnedToCore(
        networkTask,           // Функция задачи
        "NetworkTask",         // Имя задачи
        8192,                  // Размер стека (увеличен для HTTP)
        NULL,                  // Параметр задачи
        1,                     // Приоритет задачи
        NULL,                  // Хэндл задачи
        0                      // Ядро (0 или 1)
    );
    
    if (Serial)
    {
        Serial.println("Сетевая задача запущена");
    }
}

// Проверка очереди на наличие новых данных
extern bool checkNetworkData()
{
    PayloadData data;
    
    // Неблокирующая проверка очереди
    if (xQueueReceive(dataQueue, &data, 0) == pdTRUE)
    {
        // Получили новые данные - парсим их в основном потоке
        String payload = String(data.payload);
        parsePayload(payload);
        return true;
    }
    
    return false;
}