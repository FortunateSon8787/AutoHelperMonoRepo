# AutoHelper — API Reference

Base URL: `http://localhost:8080`
API Docs (Scalar UI): `http://localhost:8080/scalar/v1`
Health Check: `GET /health`

---

## Auth (`/api/auth`)

### POST /api/auth/register

Регистрация нового клиента с email и паролем.

**Request:**
```json
{
  "name": "Иван Иванов",
  "email": "ivan@example.com",
  "password": "SecurePass123!"
}
```

**Response 201:**
```json
{ "customerId": "uuid" }
```

**Errors:**
- `409 Conflict` — email уже зарегистрирован
- `400 Bad Request` — ошибки валидации

---

### POST /api/auth/login

Аутентификация по email + пароль. Возвращает JWT access token и refresh token.

**Request:**
```json
{
  "email": "ivan@example.com",
  "password": "SecurePass123!"
}
```

**Response 200:**
```json
{
  "accessToken": "eyJ...",
  "refreshToken": "base64-token",
  "expiresAt": "2026-03-25T10:15:00Z"
}
```

**Errors:**
- `401 Unauthorized` — неверные учётные данные

---

### POST /api/auth/refresh

Обмен refresh token на новую пару токенов (token rotation). Старый refresh token инвалидируется.

**Request:**
```json
{
  "refreshToken": "base64-old-token"
}
```

**Response 200:** (аналогично `/login`)

**Errors:**
- `401 Unauthorized` — токен истёк или не существует

---

### POST /api/auth/logout

Отзыв текущего refresh token.

**Request:**
```json
{
  "refreshToken": "base64-token"
}
```

**Response:** `204 No Content`

**Errors:**
- `404 Not Found` — токен не найден

---

## Клиенты (`/api/clients`) — **реализовано** (Epic AUT-1)

> Все эндпоинты требуют `Authorization: Bearer <accessToken>`

### GET /api/clients/me

Получение профиля текущего клиента.

**Response 200:**
```json
{
  "id": "uuid",
  "name": "Иван Иванов",
  "email": "ivan@example.com",
  "contacts": "+7 999 123-45-67",
  "subscriptionStatus": "Free",
  "authProvider": "Local",
  "registrationDate": "2026-03-24T21:17:00Z"
}
```

**Errors:**
- `404 Not Found` — клиент не найден

---

### PUT /api/clients/me

Обновление профиля (имя, контакты).

**Request:**
```json
{
  "name": "Иван Иванов",
  "contacts": "+7 999 123-45-67"
}
```

**Response:** `204 No Content`

**Errors:**
- `400 Bad Request` — ошибки валидации
- `404 Not Found` — клиент не найден

---

### POST /api/clients/me/password

Смена пароля (только для `AuthProvider = Local`).

**Request:**
```json
{
  "oldPassword": "OldPass123!",
  "newPassword": "NewPass456!"
}
```

**Response:** `204 No Content`

**Errors:**
- `400 Bad Request` — неверный старый пароль или ошибки валидации
- `404 Not Found` — клиент не найден

---

### POST /api/clients/me/avatar

Загрузка аватара клиента (multipart/form-data). Максимум 5 МБ, форматы: JPEG, PNG, WebP.

**Request:** `multipart/form-data` — поле `file`

**Response 200:**
```json
{
  "avatarUrl": "https://storage/avatars/uuid.jpg"
}
```

**Errors:**
- `400 Bad Request` — неверный тип файла или превышен размер
- `404 Not Found` — клиент не найден

---

## Автомобили (`/api/vehicles`)

### GET /api/vehicles/{vin}/owner — **реализовано** (AUT-12)

Публичный профиль владельца автомобиля по VIN. Авторизация **не требуется**.

**Response 200:**
```json
{
  "ownerId": "uuid",
  "name": "Иван Иванов",
  "contacts": "+7 999 123-45-67",
  "avatarUrl": "https://storage/avatars/uuid.jpg"
}
```

**Errors:**
- `404 Not Found` — автомобиль с таким VIN не найден

---

### Планируется (Epic AUT-2)

| Метод | Путь | Описание |
|-------|------|----------|
| GET | `/api/vehicles` | Список авто текущего клиента |
| POST | `/api/vehicles` | Создание нового автомобиля |
| GET | `/api/vehicles/{id}` | Детали автомобиля |
| PUT | `/api/vehicles/{id}` | Обновление данных |
| PUT | `/api/vehicles/{id}/status` | Смена статуса |
| GET | `/api/vehicles/{vin}/public` | Публичная карточка авто по VIN (SSR/SEO) |

### Статусы автомобиля

| Статус | Доп. данные |
|--------|-------------|
| `Active` | — |
| `ForSale` | — |
| `InRepair` | `partnerName` (обязательно, планируется AUT-58) |
| `Recycled` | `documentUrl` PDF (обязательно, планируется AUT-59) |
| `Dismantled` | `documentUrl` PDF (обязательно, планируется AUT-59) |

---

## История работ (`/api/vehicles/{vehicleId}/service-records`) — планируется (Epic AUT-3)

| Метод | Путь | Описание |
|-------|------|----------|
| GET | `/api/vehicles/{vehicleId}/service-records` | Список записей (публичный) |
| POST | `/api/vehicles/{vehicleId}/service-records` | Создание записи (только владелец) |
| GET | `/api/vehicles/{vehicleId}/service-records/{id}` | Детали записи |
| GET | `/api/vehicles/{vehicleId}/service-records/{id}/document` | Скачать PDF наряд |

---

## AI АвтоПомощник (`/api/chats`) — планируется (Epic AUT-4)

| Метод | Путь | Описание |
|-------|------|----------|
| GET | `/api/chats` | Список чатов клиента |
| POST | `/api/chats` | Создать чат (опционально привязать к авто) |
| GET | `/api/chats/{chatId}/messages` | История сообщений |
| POST | `/api/chats/{chatId}/messages` | Отправить сообщение → ответ LLM (streaming) |

**Доступ:** только клиентам с `SubscriptionStatus = Premium`.

---

## Биллинг (`/api/subscriptions`) — планируется (Epic AUT-5)

| Метод | Путь | Описание |
|-------|------|----------|
| GET | `/api/subscriptions/me` | Статус подписки |
| POST | `/api/subscriptions/checkout` | Создать Stripe checkout session |
| POST | `/api/subscriptions/cancel` | Отмена подписки |
| POST | `/api/stripe/webhook` | Stripe webhook endpoint |

---

## Партнёры (`/api/partners`) — планируется (Epic AUT-6)

| Метод | Путь | Описание |
|-------|------|----------|
| GET | `/api/partners` | Список верифицированных партнёров (с геофильтром) |
| POST | `/api/partners` | Регистрация партнёра |
| GET | `/api/partners/{id}` | Профиль партнёра (публичный) |
| PUT | `/api/partners/{id}` | Обновление профиля (только владелец) |
| POST | `/api/partners/{id}/reviews` | Оставить отзыв (нужен факт взаимодействия) |
| GET | `/api/partners/{id}/reviews` | Отзывы партнёра |

---

## Рекламные кампании (`/api/ad-campaigns`) — планируется (Epic AUT-6)

| Метод | Путь | Описание |
|-------|------|----------|
| GET | `/api/ad-campaigns` | Активные кампании для UI |
| POST | `/api/ad-campaigns` | Создать кампанию (партнёр) |
| PUT | `/api/ad-campaigns/{id}` | Редактировать кампанию |

---

## Административная панель (`/api/admin/...`) — планируется (Epic AUT-7)

**Доступ:** только роли `admin` и `superadmin`.

| Метод | Путь | Описание |
|-------|------|----------|
| GET | `/api/admin/customers` | Список клиентов (поиск, пагинация) |
| PUT | `/api/admin/customers/{id}/block` | Блокировка клиента |
| GET | `/api/admin/vehicles` | Все авто с поиском по VIN |
| GET | `/api/admin/partners` | Все партнёры |
| PUT | `/api/admin/partners/{id}/verify` | Верификация партнёра |
| PUT | `/api/admin/partners/{id}/deactivate` | Деактивация партнёра |
| GET | `/api/admin/partners/unfit` | Список «потенциально профнепригодных» |
| GET | `/api/admin/ad-campaigns` | Все рекламные кампании |

---

## Аутентификация запросов

Защищённые эндпоинты требуют заголовок:

```
Authorization: Bearer <accessToken>
```

Access token живёт **15 минут** (`expiresAt` — ISO 8601 DateTime UTC). После истечения — использовать `/api/auth/refresh` с refresh token.

Refresh token живёт **30 дней**, хранится в httpOnly cookie или передаётся в теле запроса.
