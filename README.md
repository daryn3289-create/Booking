# Booking

## Описание проекта
Booking — это микросервисное приложение для бронирования отелей, включающее аутентификацию, каталог отелей, профили пользователей, шлюз, интеграцию с Keycloak, PostgreSQL, RabbitMQ и SMTP (Mailhog).

## Технологии
- .NET (ASP.NET Core)
- Docker, Docker Compose
- PostgreSQL
- Keycloak (OAuth2/OpenID Connect)
- RabbitMQ
- Mailhog (SMTP)
- pgAdmin

## Зависимости
- Docker
- Docker Compose
- .NET SDK

## Быстрый старт
1. Установите Docker и Docker Compose.
2. Клонируйте репозиторий.
3. Запустите все сервисы:
   ```
   docker compose up --build
   ```
4. Сервисы будут доступны на следующих портах:
   - Identity API: 5000
   - Hotel Catalog API: 5001
   - Profile API: 5002
   - Gateway: 80
   - Keycloak: 8080
   - PostgreSQL: 5432
   - pgAdmin: 5050
   - RabbitMQ: 5672 (AMQP), 15672 (Web UI)
   - Mailhog: 1025 (SMTP), 8025 (Web UI)

## Настройка Keycloak
- Админ-панель: http://localhost:8080
- Логин: admin
- Пароль: admin
- Keycloak использует PostgreSQL как хранилище данных.
- Для интеграции с сервисами настройте Realm, Client и Users в Keycloak.

## Настройка PostgreSQL
- Данные для подключения:
  - Host: postgres
  - Database: booking
  - User: postgres
  - Password: postgres
- Для управления используйте pgAdmin (http://localhost:5050, admin@admin.com/admin).

## Настройка RabbitMQ
- Web UI: http://localhost:15672
- Логин: guest
- Пароль: guest

## Настройка Mailhog
- Web UI: http://localhost:8025
- SMTP: localhost:1025

## Структура Docker Compose
Все сервисы и зависимости описаны в `compose.yaml`. Для запуска и остановки используйте стандартные команды Docker Compose.

## Примечания
- Для разработки и тестирования используйте локальные порты, указанные выше.
- Все данные Keycloak, PostgreSQL, RabbitMQ и Mailhog сохраняются в Docker volume.

---
Для подробной настройки каждого микросервиса смотрите соответствующие Dockerfile и документацию по .NET.
