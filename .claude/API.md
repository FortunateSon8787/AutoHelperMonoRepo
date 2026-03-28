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

### GET /api/vehicles — **реализовано** (AUT-13)

Список автомобилей текущего пользователя. Требует `Authorization: Bearer`.

**Response 200:**
```json
[
  {
    "id": "uuid",
    "vin": "1HGCM82633A123456",
    "brand": "Toyota",
    "model": "Camry",
    "year": 2020,
    "color": "White",
    "mileage": 45000,
    "status": "Active",
    "ownerId": "uuid"
  }
]
```

---

### POST /api/vehicles — **реализовано** (AUT-13)

Создание нового автомобиля. Требует `Authorization: Bearer`.

**Request:**
```json
{
  "vin": "1HGCM82633A123456",
  "brand": "Toyota",
  "model": "Camry",
  "year": 2020,
  "color": "White",
  "mileage": 45000
}
```

**Response 201:**
```json
{ "vehicleId": "uuid" }
```

**Errors:**
- `409 Conflict` — автомобиль с таким VIN уже существует
- `400 Bad Request` — ошибки валидации (VIN формат, год, пробег)

---

### GET /api/vehicles/{id} — **реализовано** (AUT-13)

Получение автомобиля по UUID. Доступно только владельцу. Требует `Authorization: Bearer`.

**Response 200:** (аналогично элементу из `GET /api/vehicles`)

**Errors:**
- `404 Not Found` — авто не найдено или принадлежит другому пользователю

---

### PUT /api/vehicles/{id} — **реализовано** (AUT-13)

Обновление мутабельных полей (VIN не меняется). Требует `Authorization: Bearer`.

**Request:**
```json
{
  "brand": "Toyota",
  "model": "Camry",
  "year": 2021,
  "color": "Black",
  "mileage": 50000
}
```

**Response:** `204 No Content`

**Errors:**
- `404 Not Found` — авто не найдено или принадлежит другому пользователю
- `400 Bad Request` — ошибки валидации

---

### PUT /api/vehicles/{id}/status — **реализовано** (AUT-14)

Смена статуса автомобиля. Требует `Authorization: Bearer`. Принимает `multipart/form-data`.

**Request (form fields):**
- `status` (string, required): `Active` | `ForSale` | `InRepair` | `Recycled` | `Dismantled`
- `partnerName` (string, required if status=InRepair): название автосервиса
- `document` (file, required if status=Recycled or Dismantled): PDF, max 10 МБ

**Response:** `204 No Content`

**Errors:**
- `400 Bad Request` — нарушение бизнес-правил (нет partnerName / документа)
- `404 Not Found` — авто не найдено или принадлежит другому пользователю

### Статусы автомобиля — **реализовано** (AUT-14)

| Статус | Доп. данные |
|--------|-------------|
| `Active` | — |
| `ForSale` | — |
| `InRepair` | `partnerName` (обязательно) |
| `Recycled` | `documentUrl` PDF (обязательно) |
| `Dismantled` | `documentUrl` PDF (обязательно) |

### Планируется (Epic AUT-2)

| Метод | Путь | Описание |
|-------|------|----------|
| GET | `/api/vehicles/{vin}/public` | Публичная карточка авто по VIN (SSR/SEO) |

---

## История работ (`/api/vehicles/{vehicleId}/service-records`) — **реализовано** (AUT-16 / Epic AUT-3)

### GET /api/vehicles/{vin}/service-records — публичный

Список всех записей о работах по VIN. Авторизация **не требуется**. Используется на публичной странице авто.

**Response 200:** `ServiceRecord[]`

---

### GET /api/vehicles/{vehicleId}/service-records — authenticated

Список записей для владельца. Требует `Authorization: Bearer`.

**Response 200:** `ServiceRecord[]`

**Errors:** `404` — авто не найдено или принадлежит другому пользователю

---

### POST /api/vehicles/{vehicleId}/service-records

Создание записи о работе. Требует `Authorization: Bearer`. Только владелец авто.

**Важно:** перед вызовом этого эндпоинта нужно загрузить PDF через `POST /api/service-records/document` и получить `documentUrl`.

**Request:**
```json
{
  "title": "Замена масла и фильтров",
  "description": "Подробное описание работ",
  "performedAt": "2026-03-01T00:00:00Z",
  "cost": 1500.00,
  "executorName": "AutoService Pro",
  "executorContacts": "+7-999-000-0000",
  "operations": ["Замена масла", "Замена масляного фильтра"],
  "documentUrl": "https://storage/service-records/documents/uuid.pdf"
}
```

**Response 201:**
```json
{ "serviceRecordId": "uuid" }
```

**Errors:** `400` — ошибки валидации; `404` — авто не найдено / не ваше

---

### GET /api/service-records/{id}

Детальная запись по ID. Требует `Authorization: Bearer`.

**Response 200:**
```json
{
  "id": "uuid",
  "vehicleId": "uuid",
  "title": "Замена масла",
  "description": "...",
  "performedAt": "2026-03-01T00:00:00Z",
  "cost": 1500.00,
  "executorName": "AutoService Pro",
  "executorContacts": "+7-999-000-0000",
  "operations": ["Замена масла"],
  "documentUrl": "https://storage/..."
}
```

---

### PUT /api/service-records/{id}

Обновление записи. Требует `Authorization: Bearer`. `documentUrl` **не меняется** — иммутабелен.

**Request:** аналогично POST, но без `documentUrl`.

**Response:** `204 No Content`

---

### DELETE /api/service-records/{id}

Soft-delete записи. Требует `Authorization: Bearer`.

**Response:** `204 No Content`

---

### POST /api/service-records/document

Загрузка PDF наряд-заказа. Требует `Authorization: Bearer`. `multipart/form-data`, поле `document`. Максимум 10 МБ.

Имя файла в хранилище генерируется как UUID: `service-records/documents/{uuid}.pdf`.

**Response 200:**
```json
{ "documentUrl": "https://storage/service-records/documents/uuid.pdf" }
```

**Errors:** `400` — не PDF или превышен размер

---

## AI АвтоПомощник (`/api/chats`) — **реализовано** (AUT-17 / Epic AUT-4)

> Все эндпоинты требуют `Authorization: Bearer <accessToken>`

### GET /api/chats

Список чат-сессий текущего клиента (без тел сообщений, только count).

**Response 200:** `ChatSummaryResponse[]`

```json
[
  {
    "id": "uuid",
    "mode": "FaultHelp",
    "title": "Стук в двигателе",
    "vehicleId": "uuid",
    "messageCount": 6,
    "createdAt": "2026-03-28T20:00:00Z"
  }
]
```

---

### POST /api/chats

Создать новую чат-сессию. Режимы `FaultHelp` и `WorkClarification` требуют `SubscriptionStatus = Premium`. Режим `PartnerAdvice` — бесплатный.

**Request:**
```json
{
  "mode": "FaultHelp",
  "title": "Стук в двигателе",
  "vehicleId": "uuid"
}
```

**ChatMode:** `FaultHelp` | `WorkClarification` | `PartnerAdvice`

**Response 201:**
```json
{ "chatId": "uuid" }
```

**Errors:**
- `403 Forbidden` — режим требует активной Premium-подписки
- `400 Bad Request` — ошибки валидации

---

### GET /api/chats/{chatId}/messages

История сообщений чата (только владелец). Отсортировано по времени.

**Response 200:** `MessageResponse[]`

```json
[
  {
    "id": "uuid",
    "role": "User",
    "content": "Слышу стук при запуске",
    "isValid": true,
    "createdAt": "2026-03-28T20:01:00Z"
  },
  {
    "id": "uuid",
    "role": "Assistant",
    "content": "Стук при запуске может указывать на...",
    "isValid": true,
    "createdAt": "2026-03-28T20:01:02Z"
  }
]
```

**Errors:**
- `404 Not Found` — чат не найден или принадлежит другому пользователю

---

### POST /api/chats/{chatId}/messages

Отправить сообщение в чат и получить ответ LLM. Off-topic запросы сохраняются с `isValid=false` и не расходуют квоту.

**Request:**
```json
{
  "content": "Слышу стук при запуске холодного двигателя",
  "locale": "ru"
}
```

- `locale` — язык ответа LLM (по умолчанию `"ru"`)

**Response 200:**
```json
{
  "assistantReply": "Стук при холодном запуске часто указывает на...",
  "wasValid": true
}
```

- `wasValid: false` — запрос был off-topic; вернулась стандартная заглушка

**Errors:**
- `403 Forbidden` — требуется Premium-подписка
- `404 Not Found` — чат не найден

---

**Бизнес-правила:**
- `FaultHelp` (Режим 1) и `WorkClarification` (Режим 2) — только `SubscriptionStatus = Premium`
- `PartnerAdvice` (Режим 3) — бесплатен для всех клиентов
- Topic guard проверяет релевантность вопроса перед вызовом LLM
- Невалидные сообщения хранятся для аудита, но не уменьшают счётчик подписки
- История, передаваемая LLM, фильтрует `isValid=false` сообщения
- LLM API ключ хранится только на бэкенде (LlmSettings, никогда не в браузере)

---

## Биллинг (`/api/subscriptions`) — планируется (Epic AUT-5)

| Метод | Путь | Описание |
|-------|------|----------|
| GET | `/api/subscriptions/me` | Статус подписки |
| POST | `/api/subscriptions/checkout` | Создать Stripe checkout session |
| POST | `/api/subscriptions/cancel` | Отмена подписки |
| POST | `/api/stripe/webhook` | Stripe webhook endpoint |

---

## Партнёры (`/api/partners`) — **реализовано** (AUT-24 / Epic AUT-6)

> Все эндпоинты требуют `Authorization: Bearer <accessToken>`

### POST /api/partners/register

Регистрация нового партнёра. Один аккаунт — один партнёрский профиль.

**Request:**
```json
{
  "name": "AutoService LLC",
  "type": "AutoService",
  "specialization": "Engine repair, diagnostics",
  "description": "Brief description of services",
  "address": "Moscow, Lenina St. 1",
  "locationLat": 55.75,
  "locationLng": 37.61,
  "workingOpenFrom": "09:00",
  "workingOpenTo": "18:00",
  "workingDays": "Mon-Fri",
  "contactsPhone": "+7-999-000-0000",
  "contactsWebsite": "https://example.com"
}
```

**Response 201:**
```json
{ "partnerId": "uuid" }
```

**Errors:**
- `409 Conflict` — профиль партнёра для этого аккаунта уже существует
- `400 Bad Request` — ошибки валидации
- `401 Unauthorized` — требуется авторизация

**Partner Types:** `AutoService` | `CarWash` | `Towing` | `AutoShop` | `Other`

После регистрации: `IsVerified = false`, `IsActive = false`. Профиль активируется после верификации администратором.

---

### GET /api/partners/me

Получение своего партнёрского профиля.

**Response 200:**
```json
{
  "id": "uuid",
  "name": "AutoService LLC",
  "type": "AutoService",
  "specialization": "Engine repair",
  "description": "...",
  "address": "Moscow, Lenina St. 1",
  "locationLat": 55.75,
  "locationLng": 37.61,
  "workingOpenFrom": "09:00",
  "workingOpenTo": "18:00",
  "workingDays": "Mon-Fri",
  "contactsPhone": "+7-999-000-0000",
  "contactsWebsite": "https://example.com",
  "logoUrl": null,
  "isVerified": false,
  "isActive": false,
  "accountUserId": "uuid"
}
```

**Errors:**
- `404 Not Found` — профиль партнёра не найден
- `401 Unauthorized` — требуется авторизация

---

### PUT /api/partners/me

Обновление партнёрского профиля. `type` не изменяется.

**Request:** (аналогично POST /register, но без поля `type`)

**Response:** `204 No Content`

**Errors:**
- `404 Not Found` — профиль не найден
- `400 Bad Request` — ошибки валидации
- `401 Unauthorized`

---

## Партнёры — Администрирование (`/api/admin/partners`) — **реализовано** (AUT-24)

**Доступ:** только роли `admin` и `superadmin`.

### GET /api/admin/partners/pending

Список партнёров, ожидающих верификации (`IsVerified = false`).

**Response 200:** `PartnerResponse[]`

---

### POST /api/admin/partners/{id}/verify

Верификация партнёра. Устанавливает `IsVerified = true`, `IsActive = true`.

**Response:** `204 No Content`

**Errors:** `404 Not Found` — партнёр не найден

---

### POST /api/admin/partners/{id}/deactivate

Деактивация партнёра. Устанавливает `IsActive = false`.

**Response:** `204 No Content`

**Errors:** `404 Not Found` — партнёр не найден

---

### POST /api/partners/{partnerId}/reviews — **реализовано** (AUT-25)

Создание отзыва на партнёра. Требует `Authorization: Bearer`. Один отзыв на одно взаимодействие (дубли блокируются).

**Request:**
```json
{
  "rating": 5,
  "comment": "Отличный сервис, всё сделали быстро",
  "basis": "ExecutorInServiceRecord",
  "interactionReferenceId": "uuid"
}
```

**Fields:**
- `rating` — целое от 1 до 5
- `comment` — непустая строка, максимум 2000 символов
- `basis` — `ExecutorInServiceRecord` | `RecommendedByAI`
- `interactionReferenceId` — UUID записи о работе (ServiceRecord.Id) или другого взаимодействия

**Response 201:**
```json
{ "reviewId": "uuid" }
```

**Errors:**
- `400 Bad Request` — ошибки валидации
- `401 Unauthorized` — требуется авторизация
- `404 Not Found` — партнёр не найден
- `409 Conflict` — отзыв на это взаимодействие уже существует

**Побочный эффект:** если партнёр накопил ≥ 5 отзывов с рейтингом < 3, его флаг `IsPotentiallyUnfit` становится `true`.

---

### GET /api/partners/{partnerId}/reviews — **реализовано** (AUT-25)

Публичный список отзывов партнёра. Авторизация **не требуется**.

**Response 200:**
```json
[
  {
    "id": "uuid",
    "partnerId": "uuid",
    "customerId": "uuid",
    "rating": 5,
    "comment": "Отличный сервис",
    "basis": "ExecutorInServiceRecord",
    "interactionReferenceId": "uuid",
    "createdAt": "2026-03-28T18:00:00Z"
  }
]
```

Отзывы возвращаются в порядке убывания даты создания. Удалённые отзывы (soft-delete) не включаются.

---

### GET /api/partners — **реализовано** (AUT-27)

Геолокационный поиск верифицированных партнёров. Авторизация **не требуется**.

**Query parameters:**
- `lat` (double, required) — широта точки поиска
- `lng` (double, required) — долгота точки поиска
- `radiusKm` (double, default 10, max 100) — радиус поиска в километрах
- `type` (string, optional) — фильтр по типу: `AutoService` | `CarWash` | `Towing` | `AutoShop` | `Other`
- `isOpenNow` (bool, default false) — если true, только партнёры, работающие прямо сейчас (UTC)

**Response 200:** `PartnerWithDistanceResponse[]` — отсортированы по расстоянию (ближайшие первые)

**PartnerWithDistanceResponse:**
```json
{
  "id": "uuid",
  "name": "AutoService LLC",
  "type": "AutoService",
  "specialization": "Engine repair",
  "description": "...",
  "address": "Moscow, Lenina St. 1",
  "locationLat": 55.75,
  "locationLng": 37.61,
  "distanceKm": 1.23,
  "workingOpenFrom": "09:00",
  "workingOpenTo": "18:00",
  "workingDays": "Mon-Fri",
  "isOpenNow": true,
  "contactsPhone": "+7-999-000-0000",
  "contactsWebsite": null,
  "logoUrl": null,
  "isVerified": true,
  "averageRating": 0.0,
  "reviewsCount": 0
}
```

**Бизнес-правила:**
- Расстояние вычисляется по формуле Haversine (R=6371 км) в Application слое
- Радиус автоматически ограничивается 100 км
- Неизвестный `type` → пустой список (не ошибка)
- `isOpenNow` использует UTC время и `WorkingHours.OpenFrom/OpenTo`

---

### GET /api/partners/{id} — **реализовано** (AUT-27)

Публичный профиль верифицированного и активного партнёра. Авторизация **не требуется**.

**Response 200:** `PartnerResponse`

**Errors:**
- `404 Not Found` — партнёр не найден, не верифицирован или неактивен

---

## Рекламные кампании — **реализовано** (AUT-26 / Epic AUT-6)

### GET /api/ad-campaigns/my

Список рекламных кампаний текущего партнёра. Требует `Authorization: Bearer`.

**Response 200:** `AdCampaignResponse[]`

**Errors:** `401` — не авторизован; `404` — партнёрский профиль не найден

---

### POST /api/ad-campaigns

Создание новой рекламной кампании. Только верифицированные и активные партнёры.

**Request:**
```json
{
  "type": "Banner",
  "targetCategory": "AutoService",
  "content": "Текст объявления или URL изображения",
  "startsAt": "2026-04-01T00:00:00Z",
  "endsAt": "2026-04-30T23:59:59Z",
  "showToAnonymous": true
}
```

**Ad Types:** `OfferBlock` | `Banner`
**Target Categories:** `AutoService` | `CarWash` | `Towing` | `AutoShop` | `Other`

**Response 201:**
```json
{ "campaignId": "uuid" }
```

**Errors:** `400` — партнёр не верифицирован / не активен, ошибки валидации; `401` — не авторизован

---

### PUT /api/ad-campaigns/{id}

Обновление кампании. Только владелец кампании.

**Request:** (аналогично POST)

**Response:** `204 No Content`

**Errors:** `404` — кампания не найдена или принадлежит другому партнёру

---

### DELETE /api/ad-campaigns/{id}

Soft-delete кампании. Только владелец.

**Response:** `204 No Content`

**Errors:** `404` — кампания не найдена или принадлежит другому партнёру

---

### GET /api/ads

Публичная выдача активных рекламных кампаний. Авторизация **не требуется**.

**Query parameters:**
- `isAuthenticated` (bool) — авторизован ли текущий пользователь
- `isPartner` (bool) — является ли пользователь партнёром
- `targetCategory` (string, optional) — фильтр по категории услуг

**Response 200:** `AdCampaignResponse[]` — отсортированы случайно (ротация)

**Бизнес-правила:**
- Партнёрам (`isPartner=true`) — пустой массив
- Анонимным (`isAuthenticated=false`) — только кампании с `showToAnonymous=true`
- Возвращаются только активные кампании с `isActive=true` и `startsAt ≤ now ≤ endsAt`

**AdCampaignResponse:**
```json
{
  "id": "uuid",
  "partnerId": "uuid",
  "type": "Banner",
  "targetCategory": "AutoService",
  "content": "Текст объявления",
  "startsAt": "2026-04-01T00:00:00Z",
  "endsAt": "2026-04-30T23:59:59Z",
  "isActive": false,
  "showToAnonymous": true,
  "statsImpressions": 0,
  "statsClicks": 0
}
```

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
