#pragma once

#include <globals.h>

// Запуск сетевого модуля (инициализация и подключение к WiFi)
extern void startNetwork();

// Запуск задачи для периодического получения данных с сервера
extern void startNetworkTask();

// Проверка очереди на наличие новых данных
// Возвращает true если получены новые данные, данные будут распарсены и применены
extern bool checkNetworkData();

