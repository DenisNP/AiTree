#pragma once

#include <globals.h>

// Запуск сетевого модуля (инициализация и подключение к WiFi)
extern void startNetwork();

// Получение данных с сервера
// Возвращает true если код изменился и нужно обработать новые данные
extern bool fetchData();

