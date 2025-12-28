#include <WiFi.h>
#include <HTTPClient.h>
#include <network.h>
#include <palette.h>
#include <animations.h>

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

// Получение данных с сервера
extern bool fetchData()
{
    // Проверяем подключение к WiFi
    if (WiFi.status() != WL_CONNECTED)
    {
        Serial.println("WiFi не подключен, fetchData отменен");
        return false;
    }

    HTTPClient http;
    http.begin(DATA_URL);
    
    int httpCode = http.GET();
    
    if (httpCode == HTTP_CODE_OK)
    {
        String payload = http.getString();
        if (Serial)
        {
            Serial.println("Получены данные: ");
            Serial.println(payload);
        }

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

            http.end();
            return true;  // Данные изменились
        }
        else
        {
            http.end();
            return false;  // Данные те же
        }
    }
    else
    {
        if (Serial)
        {
            Serial.print("Ошибка HTTP запроса: ");
            Serial.println(httpCode);
        }

        http.end();
        return false;
    }
}

