# AutoHelper — API Reference

Base URL: `http://localhost:8080`
API Docs (Scalar UI): `http://localhost:8080/scalar/v1`
Health Check: `GET /health`

---

## Auth (`/api/auth`)

### POST /api/auth/register

Rate limited: 10 запросов / 1 минута по IP.

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

Rate limited: 10 запросов / 1 минута по IP.

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
  "subscriptionStatus": "Premium",
  "subscriptionPlan": "Pro",
  "subscriptionStartDate": "2026-03-31T00:00:00Z",
  "subscriptionEndDate": "2026-04-30T00:00:00Z",
  "aiRequestsRemaining": 87,
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

Постранично возвращает чат-сессии текущего клиента (без тел сообщений, только count). Упорядочены по убыванию даты создания.

**Query params:** `page` (default: 1), `pageSize` (default: 20)

**Response 200:** `PagedResult<ChatSummaryResponse>`

```json
{
  "items": [
    {
      "id": "uuid",
      "mode": "FaultHelp",
      "title": "Стук в двигателе",
      "vehicleId": "uuid",
      "messageCount": 6,
      "createdAt": "2026-03-28T20:00:00Z"
    }
  ],
  "totalCount": 42,
  "page": 1,
  "pageSize": 20,
  "totalPages": 3,
  "hasNextPage": true
}
```

---

### DELETE /api/chats/{chatId}

Soft-delete чат-сессии. Запись остаётся в БД с `IsDeleted = true` и исключается из всех запросов через глобальный EF-фильтр. Удалить можно только свой чат.

**Response:** `204 No Content`

**Errors:**
- `401 Unauthorized` — не аутентифицирован
- `404 Not Found` — чат не найден или не принадлежит текущему клиенту

---

### POST /api/chats

Создать новую чат-сессию. Режимы `FaultHelp` и `WorkClarification` требуют активной платной подписки. Режим `PartnerAdvice` — бесплатный.

Для `FaultHelp` и `WorkClarification` первый ответ ассистента возвращается **сразу** в теле ответа на создание чата.

**Request (FaultHelp):**
```json
{
  "mode": "FaultHelp",
  "title": "Стук в двигателе",
  "vehicleId": "uuid",
  "diagnosticsInput": {
    "symptoms": "Стук при холодном запуске",
    "recentEvents": "Заменил масло 2 недели назад",
    "previousIssues": "Раньше такого не было"
  }
}
```

**Request (WorkClarification):**
```json
{
  "mode": "WorkClarification",
  "title": "Проверка замены тормозных колодок",
  "vehicleId": "uuid",
  "workClarificationInput": {
    "worksPerformed": "Замена тормозных колодок передних колёс",
    "workReason": "Скрип при торможении",
    "laborCost": 3000,
    "partsCost": 5000,
    "guarantees": "6 месяцев на работу и детали"
  }
}
```

**Request (PartnerAdvice):**
```json
{
  "mode": "PartnerAdvice",
  "title": "Поиск шиномонтажа",
  "partnerAdviceInput": {
    "request": "Нужен шиномонтаж рядом",
    "lat": 50.45,
    "lng": 30.52,
    "urgency": "NotSpecified"
  }
}
```

`urgency` — enum: `NotSpecified` (0) | `NotUrgent` (1) | `Urgent` (2)

**ChatMode:** `FaultHelp` | `WorkClarification` | `PartnerAdvice`

**Response 201:**
```json
{
  "chatId": "uuid",
  "initialAssistantReply": "Текстовый ответ ассистента",
  "diagnosticResultJson": null,
  "workClarificationResultJson": null,
  "partnerAdviceResultJson": null
}
```

- `initialAssistantReply` — для всех режимов содержит текстовый ответ ассистента; для `PartnerAdvice` при off-topic запросе — rejection-сообщение
- `diagnosticResultJson` — сериализованный `DiagnosticsLlmResult` для `FaultHelp` при `responseStage = "diagnostic_result"`, иначе `null`
- `workClarificationResultJson` — сериализованный `WorkClarificationLlmResult` для `WorkClarification`, иначе `null`. Фронтенд парсит и рендерит `WorkClarificationResultCard`
- `partnerAdviceResultJson` — сериализованный `PartnerAdviceLlmResult` для `PartnerAdvice`, иначе `null`. Фронтенд парсит и рендерит `PartnerAdviceResultCard`

**Errors:**
- `403 Forbidden` — режим требует активной подписки
- `400 Bad Request` — отсутствует `diagnosticsInput` для FaultHelp, `workClarificationInput` для WorkClarification, или ошибки валидации

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
    "diagnosticResultJson": null,
    "createdAt": "2026-03-28T20:01:00Z"
  },
  {
    "id": "uuid",
    "role": "Assistant",
    "content": "Текстовый ответ (follow_up вопрос или итоговый ответ)",
    "isValid": true,
    "diagnosticResultJson": "{\"potentialProblems\":[...],\"urgencyLevel\":\"medium\",...}",
    "workClarificationResultJson": null,
    "partnerAdviceResultJson": null,
    "createdAt": "2026-03-28T20:01:02Z"
  }
]
```

- `diagnosticResultJson` — сериализованный `DiagnosticsLlmResult` только для FaultHelp сообщений с `responseStage = "diagnostic_result"`. Для всех остальных сообщений — `null`. Фронтенд парсит и рендерит `DiagnosticResultCard`.
- `workClarificationResultJson` — сериализованный `WorkClarificationLlmResult` для ответа в WorkClarification чате. Для всех остальных сообщений — `null`. Фронтенд парсит и рендерит `WorkClarificationResultCard`.
- `partnerAdviceResultJson` — сериализованный `PartnerAdviceLlmResult` для ответа в PartnerAdvice чате (только при валидном запросе). Для всех остальных сообщений — `null`. Фронтенд парсит и рендерит `PartnerAdviceResultCard`.

**Errors:**
- `404 Not Found` — чат не найден или принадлежит другому пользователю

---

### POST /api/chats/{chatId}/messages

Отправить follow-up сообщение в чат и получить ответ LLM. Используется только для `FaultHelp` (после уточняющего вопроса). `WorkClarification` и `PartnerAdvice` являются одношаговыми и не принимают follow-up сообщений (чат уже `Completed`).

Off-topic запросы сохраняются с `isValid=false` и не расходуют квоту.

**Request:**
```json
{
  "content": "Стук появляется только при холодном запуске, проходит через 5 минут",
  "locale": "ru"
}
```

- `locale` — язык ответа LLM (по умолчанию `"ru"`)

**Response 200:**
```json
{
  "assistantReply": "Текстовый ответ ассистента",
  "wasValid": true,
  "responseStage": "follow_up",
  "chatStatus": "AwaitingUserAnswers",
  "diagnosticResultJson": null
}
```

- `wasValid: false` — запрос был off-topic; вернулась стандартная заглушка, квота не уменьшена
- `responseStage` — `"follow_up"` | `"diagnostic_result"` для FaultHelp; `null` для других режимов
- `chatStatus` — текущее состояние чата: `Active` | `AwaitingUserAnswers` | `FinalAnswerSent` | `Completed`
- `diagnosticResultJson` — сериализованный `DiagnosticsLlmResult` когда `responseStage = "diagnostic_result"`, иначе `null`. Фронтенд парсит и рендерит `DiagnosticResultCard`.

**Errors:**
- `403 Forbidden` — требуется активная подписка
- `404 Not Found` — чат не найден или принадлежит другому пользователю
- `409 Conflict` — чат завершён (`Completed`), новые сообщения не принимаются

---

**Бизнес-правила чатов:**
- `FaultHelp` (Режим 1) и `WorkClarification` (Режим 2) — только активная платная подписка
- `PartnerAdvice` (Режим 3) — бесплатен для всех клиентов
- RequestClassifier (router model, gpt-4.1-nano) проверяет релевантность запроса перед основным вызовом LLM
- **При `ChatStatus = AwaitingUserAnswers` классификатор полностью пропускается** — ответ пользователя на уточняющий вопрос считается заведомо валидным (`ClassificationResult.ValidFollowUpAnswer`)
- Невалидные сообщения хранятся в `InvalidChatRequests` для аудита, но не уменьшают счётчик подписки
- История, передаваемая LLM, фильтрует `IsValid=false` сообщения
- LLM API ключ хранится только на бэкенде (`LlmSettings`), никогда не в браузере
- `FaultHelp`: после `FinalAnswerSent` разрешён ровно 1 дополнительный вопрос; затем чат → `Completed`
- `FaultHelp` возвращает `diagnosticResultJson` (сериализованный `DiagnosticsLlmResult`) при `responseStage = "diagnostic_result"`; фронтенд рендерит карточку `DiagnosticResultCard`
- `WorkClarification`: одношаговый — форма → LLM → чат сразу `Completed`, follow-up невозможен; возвращает `workClarificationResultJson` (сериализованный `WorkClarificationLlmResult`); фронтенд рендерит `WorkClarificationResultCard` со структурированной карточкой (обоснованность работ, стоимость в EUR, гарантии, общая оценка честности); ценовой эталон — средние европейские цены (EUR); поле `repeat_interval_km` удалено из результата
- `PartnerAdvice`: строго одношаговый — форма → LLM → чат сразу `Completed`, follow-up невозможен; **отклонённый (off-topic) запрос также переводит чат в `Completed`**; возвращает `partnerAdviceResultJson` (сериализованный `PartnerAdviceLlmResult`) при валидном запросе, `null` при отклонении; urgency передаётся как enum (`NotSpecified` | `NotUrgent` | `Urgent`)
- `Completed`-чаты доступны только для чтения (GET), новые сообщения не принимаются (`409`)
- Суммаризация истории запускается автоматически при ≥ 20 сообщениях в чате
- При ошибке classifier-а: fail open (обработка продолжается, ошибка логируется)

---

## Подписки — **реализовано** (AUT-84/85 / Epic AUT-5)

> Все эндпоинты требуют `Authorization: Bearer <accessToken>`

### GET /api/clients/me/subscription

Статус подписки и остаток AI-запросов.

**Response 200:**
```json
{
  "status": "Premium",
  "plan": "Pro",
  "startDate": "2026-03-31T00:00:00Z",
  "endDate": "2026-04-30T00:00:00Z",
  "aiRequestsRemaining": 87,
  "monthlyPriceUsd": 7.99,
  "monthlyRequestQuota": 100
}
```

**Plans:** `None` | `Normal` ($4.99, 30 req/mo) | `Pro` ($7.99, 100 req/mo) | `Max` ($12.99, 300 req/mo)

---

### POST /api/clients/me/subscription/activate

Активация или смена тарифного плана. Биллинг через `IBillingService` (интеграция Lemon Squeezy — в отдельной задаче).

**Request:**
```json
{ "plan": "Pro" }
```

**Response:** `204 No Content`

**Errors:**
- `400 Bad Request` — `SUBSCRIPTION_003` (invalid plan name)
- `404 Not Found` — клиент не найден

---

### POST /api/clients/me/subscription/topup

Разовое пополнение AI-запросов (без смены тарифа).

**Request:**
```json
{ "count": 10 }
```

**Response:** `204 No Content`

**Errors:**
- `400 Bad Request` — count ≤ 0
- `404 Not Found` — клиент не найден

---

### Биллинг через Lemon Squeezy — планируется (Epic AUT-5)

| Метод | Путь | Описание |
|-------|------|----------|
| POST | `/api/subscriptions/checkout` | Создать Lemon Squeezy checkout session |
| POST | `/api/subscriptions/cancel` | Отмена подписки |
| POST | `/api/billing/webhook` | Lemon Squeezy webhook endpoint |

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

## Партнёры — Администрирование (`/api/admin/partners`) — **реализовано** (AUT-24, AUT-30)

**Доступ:** только роль `admin`.

### GET /api/admin/partners

Пагинированный список всех партнёров (включая неверифицированных и неактивных) с поиском по имени или адресу.

**Query parameters:**
- `page` (int, default 1)
- `pageSize` (int, default 20)
- `search` (string, optional) — частичное совпадение с Name или Address

**Response 200:**
```json
{
  "items": [
    {
      "id": "uuid",
      "name": "AutoService LLC",
      "type": "AutoService",
      "specialization": "Engine repair",
      "address": "Moscow, Lenina St. 1",
      "isVerified": true,
      "isActive": true,
      "isPotentiallyUnfit": false,
      "reviewCount": 12,
      "averageRating": 4.3
    }
  ],
  "totalCount": 35,
  "page": 1,
  "pageSize": 20
}
```

---

### GET /api/admin/partners/unfit

Список партнёров с `IsPotentiallyUnfit = true`, с детализацией отзывов с рейтингом ниже 3.

**Response 200:**
```json
[
  {
    "id": "uuid",
    "name": "BadService LLC",
    "address": "Moscow, Pushkina St. 5",
    "lowRatingReviews": [
      {
        "id": "uuid",
        "rating": 1,
        "comment": "Ужасный сервис",
        "createdAt": "2026-03-28T18:00:00Z"
      }
    ]
  }
]
```

---

### GET /api/admin/partners/{id}

Детальный профиль партнёра со всеми его отзывами.

**Response 200:**
```json
{
  "id": "uuid",
  "name": "AutoService LLC",
  "type": "AutoService",
  "specialization": "Engine repair",
  "address": "Moscow, Lenina St. 1",
  "isVerified": true,
  "isActive": true,
  "isPotentiallyUnfit": false,
  "reviews": [
    {
      "id": "uuid",
      "rating": 5,
      "comment": "Отличный сервис",
      "createdAt": "2026-03-28T18:00:00Z"
    }
  ]
}
```

**Errors:** `404` — партнёр не найден (`ADMIN_005`)

---

### POST /api/admin/partners/{id}/verify

Верификация партнёра. Устанавливает `IsVerified = true`, `IsActive = true`.

**Response:** `204 No Content`

**Errors:**
- `404` — партнёр не найден (`ADMIN_005`)
- `400` — партнёр уже верифицирован (`ADMIN_006`)

---

### POST /api/admin/partners/{id}/deactivate

Деактивация партнёра. Устанавливает `IsActive = false` и деактивирует все его активные рекламные кампании.

**Response:** `204 No Content`

**Errors:**
- `404` — партнёр не найден (`ADMIN_005`)
- `400` — партнёр уже деактивирован (`ADMIN_007`)

---

### DELETE /api/admin/partners/{id}

Soft-delete партнёра (`IsDeleted = true`, `IsActive = false`).

**Response:** `204 No Content`

**Errors:** `404` — партнёр не найден (`ADMIN_005`)

---

### DELETE /api/admin/reviews/{id}

Soft-delete отзыва партнёра. Автоматически пересчитывает флаг `IsPotentiallyUnfit` у партнёра.

**Response:** `204 No Content`

**Errors:** `404` — отзыв не найден (`ADMIN_008`)

---

## Рекламные кампании — Администрирование (`/api/admin/ad-campaigns`) — **реализовано** (AUT-31)

**Доступ:** только роль `admin`.

### GET /api/admin/ad-campaigns

Пагинированный список всех рекламных кампаний (без soft-deleted) с опциональным фильтром по партнёру.

**Query parameters:**
- `page` (int, default 1)
- `pageSize` (int, default 20)
- `partnerId` (guid, optional) — фильтр по конкретному партнёру

**Response 200:**
```json
{
  "items": [
    {
      "id": "uuid",
      "partnerId": "uuid",
      "type": "Banner",
      "targetCategory": "AutoService",
      "content": "Текст рекламы",
      "startsAt": "2026-04-01T00:00:00Z",
      "endsAt": "2026-04-30T23:59:59Z",
      "isActive": true,
      "showToAnonymous": false,
      "statsImpressions": 150,
      "statsClicks": 12
    }
  ],
  "totalCount": 42,
  "page": 1,
  "pageSize": 20
}
```

---

### POST /api/admin/ad-campaigns/{id}/activate

Активирует рекламную кампанию (устанавливает `IsActive = true`).

**Response:** `204 No Content`

**Errors:**
- `404` — кампания не найдена (`ADMIN_009`)
- `400` — кампания уже активна (`ADMIN_010`)

---

### POST /api/admin/ad-campaigns/{id}/deactivate

Деактивирует рекламную кампанию (устанавливает `IsActive = false`).

**Response:** `204 No Content`

**Errors:**
- `404` — кампания не найдена (`ADMIN_009`)
- `400` — кампания уже неактивна (`ADMIN_011`)

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

## Административная панель — Клиенты (`/api/admin/customers`) — **реализовано** (AUT-28)

**Доступ:** только роль `admin`.

### GET /api/admin/customers

Пагинированный список клиентов с поиском по имени или email.

**Query parameters:**
- `page` (int, default 1)
- `pageSize` (int, default 20)
- `search` (string, optional) — частичное совпадение с name или email

**Response 200:**
```json
{
  "items": [
    {
      "id": "uuid",
      "name": "Иван Иванов",
      "email": "ivan@example.com",
      "contacts": "+7 999 123-45-67",
      "subscriptionStatus": "Premium",
      "subscriptionPlan": "Pro",
      "aiRequestsRemaining": 87,
      "authProvider": "Local",
      "registrationDate": "2026-03-24T21:17:00Z",
      "isBlocked": false,
      "invalidChatRequestCount": 2
    }
  ],
  "totalCount": 42,
  "page": 1,
  "pageSize": 20
}
```

---

### GET /api/admin/customers/{id}

Карточка конкретного клиента.

**Response 200:** `AdminCustomerResponse` (аналогично элементу из списка)

**Errors:** `404` — клиент не найден

---

### POST /api/admin/customers/{id}/block

Блокировка аккаунта клиента.

**Response:** `204 No Content`

**Errors:**
- `404` — клиент не найден (`ADMIN_001`)
- `400` — аккаунт уже заблокирован (`ADMIN_002`)

---

### POST /api/admin/customers/{id}/unblock

Разблокировка аккаунта клиента.

**Response:** `204 No Content`

**Errors:**
- `404` — клиент не найден (`ADMIN_001`)
- `400` — аккаунт не заблокирован (`ADMIN_003`)

---

## Административная панель — Автомобили (`/api/admin/vehicles`) — **реализовано** (AUT-29)

**Доступ:** только роль `admin`.

### GET /api/admin/vehicles

Пагинированный список автомобилей с поиском по VIN, марке или модели.

**Query parameters:**
- `page` (int, default 1)
- `pageSize` (int, default 20)
- `search` (string, optional) — частичное совпадение с VIN, Brand или Model (регистронезависимо)

**Response 200:**
```json
{
  "items": [
    {
      "id": "uuid",
      "vin": "1HGCM82633A123456",
      "brand": "Toyota",
      "model": "Camry",
      "year": 2020,
      "color": "White",
      "mileage": 45000,
      "status": "Active",
      "partnerName": null,
      "documentUrl": null,
      "ownerId": "uuid"
    }
  ],
  "totalCount": 18,
  "page": 1,
  "pageSize": 20
}
```

---

### GET /api/admin/vehicles/{id}

Карточка конкретного автомобиля по UUID.

**Response 200:** `AdminVehicleResponse` (аналогично элементу из списка)

**Errors:** `404` — автомобиль не найден (`ADMIN_004`)

---

## Admin Auth (`/api/admin/auth`) — **реализовано** (AUT-32)

Аутентификация администраторов. Использует отдельные httpOnly cookie (`adminAccessToken`, `adminRefreshToken`), не конфликтующие с клиентскими cookie. Admin токены подписываются **отдельным секретом** (`Jwt:AdminSecret`), изолированным от клиентских.

### POST /api/admin/auth/login

Rate limited: 10 запросов / 1 минута по IP.

**Request:**
```json
{ "email": "admin@autohelper.io", "password": "..." }
```

**Response 200:** Устанавливает httpOnly cookie:
- `adminAccessToken` — JWT с `role` claim (`"admin"` или `"superadmin"`), подписан `AdminSecret`, TTL 15 мин
- `adminRefreshToken` — opaque токен, персистируется в `admin_refresh_tokens`, TTL 7 дней

**Errors:**
- `401 Unauthorized` — неверные credentials
- `429 Too Many Requests` — превышен rate limit

---

### POST /api/admin/auth/refresh

Обмен refresh token на новую пару токенов (token rotation). Старый refresh token инвалидируется. Читает `adminRefreshToken` из cookie.

**Response 200:** Устанавливает новые httpOnly cookies (аналогично `/login`).

**Errors:**
- `401 Unauthorized` — токен истёк, отозван или не существует

---

### POST /api/admin/auth/logout

**Требует аутентификации** (роль `admin` или `superadmin`).  
Отзывает refresh token в БД и удаляет cookie `adminAccessToken` и `adminRefreshToken`.

**Response:** `204 No Content`

**Errors:**
- `401 Unauthorized` — не аутентифицирован
- `404 Not Found` — refresh токен не найден

---

## Конфигурация АвтоПомощника (`/api/admin/chatbot-config`) — **реализовано** (AUT-162)

**Доступ:** только роль `admin`.

### GET /api/admin/chatbot-config

Получение текущей конфигурации чатбота.

**Response 200:**
```json
{
  "isEnabled": true,
  "maxCharsPerField": 2000,
  "dailyLimitByPlan": {
    "None": 0,
    "Normal": 10,
    "Pro": 20,
    "Max": 40
  },
  "topUpPriceUsd": 3.00,
  "topUpRequestCount": 10,
  "disablePartnerSuggestionsInMode1": false
}
```

---

### PUT /api/admin/chatbot-config

Обновление конфигурации чатбота. Принимает тот же формат, что GET. Все поля обязательны.

**Response 204:** No Content

**Errors:**
- `400 Bad Request` — невалидный ключ плана в `dailyLimitByPlan`, нулевые значения лимитов/цен

---

## Административная панель — прочее (`/api/admin/...`) — реализовано (Epic AUT-7)

**Доступ:** только роли `admin` и `superadmin` (JWT с role claim, устанавливается через `/api/admin/auth/login`).

---

## Аутентификация запросов

Защищённые эндпоинты требуют заголовок:

```
Authorization: Bearer <accessToken>
```

Access token живёт **15 минут** (`expiresAt` — ISO 8601 DateTime UTC). После истечения — использовать `/api/auth/refresh` с refresh token.

Refresh token живёт **30 дней**, хранится в httpOnly cookie или передаётся в теле запроса.
