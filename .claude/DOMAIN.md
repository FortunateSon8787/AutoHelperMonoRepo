# AutoHelper — Domain Model

## Принципы доменной модели

- Все бизнес-правила живут **только в Domain** — никакой логики в Infrastructure или Api.
- Агрегаты создаются через **фабричные методы** (не через конструктор напрямую).
- Все свойства агрегатов **private set** — изменение только через методы агрегата.
- **Domain Events** публикуются внутри агрегата, диспатчатся после коммита через MediatR.
- Исключения для доменных ошибок **не используются** — возвращается `Result.Failure`.

---

## Реализованные агрегаты

### Customer

**Файл:** `Domain/Customers/Customer.cs`

```
Customer : AggregateRoot<Guid>
├── Id: Guid
├── Name: string
├── Email: string               (нормализован: lowercase)
├── PasswordHash: string?       (null для Google OAuth)
├── Contacts: string?           (телефон, Telegram и др.)
├── AvatarUrl: string?          (URL в object storage)
├── SubscriptionStatus: SubscriptionStatus  (Free | Premium | Suspended)
├── SubscriptionPlan: SubscriptionPlan      (None | Normal | Pro | Max)
├── SubscriptionStartDate: DateTime?        (UTC; null для free tier)
├── SubscriptionEndDate: DateTime?          (UTC; null для free tier)
├── AiRequestsRemaining: int               (счётчик; уменьшается Режимами 1 и 2)
├── RegistrationDate: DateTime
├── AuthProvider: AuthProvider
│
├── Google OAuth поля:
│   ├── GoogleId: string?
│   ├── GoogleEmail: string?
│   ├── GooglePicture: string?
│   └── GoogleRefreshToken: string?
│
├── Factory methods:
│   ├── CreateWithPassword(name, email, passwordHash, contacts?)
│   └── CreateWithGoogle(name, email, googleId, googleEmail?, googlePicture?, googleRefreshToken?, contacts?)
│
├── Admin fields:
│   └── IsBlocked: bool             (default: false; блокировка администратором)
│
└── Business operations:
    ├── UpdateGoogleInfo(googlePicture?, googleRefreshToken?)
    ├── UpdateContacts(contacts?)
    ├── UpdateProfile(name, contacts?)
    ├── UpdateAvatar(avatarUrl)
    ├── ChangePassword(newPasswordHash) → bool
    ├── ActivateSubscription(plan) — активирует план, сбрасывает счётчик по тарифу
    ├── TopUpRequests(count)       — разовое пополнение счётчика
    ├── CancelSubscription()       — возврат на free tier
    ├── DecrementAiQuota() → bool  — −1 запрос; false если счётчик уже 0
    ├── Block()                    — IsBlocked = true (только admin)
    └── Unblock()                  — IsBlocked = false (только admin)
```

**Domain Events:**
- `CustomerRegisteredEvent` — публикуется при создании (оба factory methods)

---

### RefreshToken

**Файл:** `Domain/Customers/RefreshToken.cs`

Хранит refresh-токены для клиентов. Поддерживает token rotation.

---

## Enums

### AuthProvider

```csharp
enum AuthProvider { Local, Google }
```

### SubscriptionStatus

```csharp
enum SubscriptionStatus
{
    Free,       // Нет доступа к AI-чату
    Premium,    // Полный доступ ко всем функциям (активный план)
    Suspended   // Подписка приостановлена (ошибка оплаты)
}
```

### SubscriptionPlan

```csharp
enum SubscriptionPlan
{
    None,    // Free tier
    Normal,  // $4.99/мес — 30 запросов/мес
    Pro,     // $7.99/мес — 100 запросов/мес
    Max      // $12.99/мес — 300 запросов/мес
}
```

---

### Vehicle — **полностью реализован** (AUT-12, AUT-13 / Epic AUT-2)

**Файл:** `Domain/Vehicles/Vehicle.cs`

```
Vehicle : AggregateRoot<Guid>
├── Vin: string           (УНИКАЛЕН в системе — unique constraint в БД, нормализован ToUpperInvariant)
├── Brand: string
├── Model: string
├── Year: int
├── Color: string?
├── Mileage: int
├── Status: VehicleStatus
├── OwnerId: Guid         (FK → Customer)
│
├── Factory method:
│   └── Create(vin, brand, model, year, ownerId, color?, mileage?)
│       ↳ throws DomainException если VIN не соответствует формату ISO 3779
│
├── Business operations:
│   ├── UpdateDetails(brand, model, year, color?, mileage)
│   │   ↳ обновляет мутабельные поля; VIN и OwnerId неизменны
│   └── ChangeStatus(status, partnerName?, documentUrl?) → Result
│       ↳ InRepair: требует partnerName; Recycled/Dismantled: требует documentUrl
│       ↳ прочие статусы: обнуляет partnerName и documentUrl
```

**VIN-инвариант (ISO 3779):** ровно 17 символов, только A–Z и 0–9, буквы I, O, Q запрещены.
Нормализация: `Trim().ToUpperInvariant()` до проверки. Нарушение → `DomainException`.

**Enum VehicleStatus:**

| Значение | Описание | Доп. данные |
|----------|----------|-------------|
| `Active` | По умолчанию | — |
| `ForSale` | В продаже | — |
| `InRepair` | В ремонте | `PartnerName` (обязательно) |
| `Recycled` | Утилизирован | `DocumentUrl` PDF (обязательно) |
| `Dismantled` | Разобран | `DocumentUrl` PDF (обязательно) |

**Бизнес-правила:**
- VIN уникален — `IVehicleRepository.ExistsByVinAsync` проверяется перед созданием.
- VIN валидируется в домене по ISO 3779 (17 символов, без I/O/Q) при вызове `Vehicle.Create()`.
- VIN и OwnerId неизменны после создания — изменение через `UpdateDetails` невозможно.

---

## Сквозные доменные механизмы

### Soft-delete (Epic AUT-150)

Все агрегаты содержат поле `IsDeleted: bool` (по умолчанию `false`). Физическое удаление из БД не производится. EF Core глобальный фильтр `HasQueryFilter(e => !e.IsDeleted)` применяется ко всем сущностям автоматически.

### AuditLog (Epic AUT-150)

```
AuditLog : Entity<Guid>
├── OperationType: AuditOperationType   (Created | Updated | Deleted)
├── EntityType: string                  ("Customer" | "Vehicle" | "ServiceRecord")
├── EntityId: Guid
├── PerformedAt: DateTime               (UTC)
├── PerformedByUserId: Guid?
├── PerformedByRole: string             ("Client" | "Admin" | "System")
└── AdditionalInfo: string?             (JSON; для Updated ServiceRecord — {"OldEntity": "<json>"})
```

---

## Планируемые агрегаты (по требованиям)

---

### ServiceRecord — **реализован** (AUT-16 / Epic AUT-3)

**Файл:** `Domain/ServiceRecords/ServiceRecord.cs`

```
ServiceRecord : AggregateRoot<Guid>
├── VehicleId: Guid
├── Title: string
├── Description: string
├── PerformedAt: DateTime
├── Cost: decimal
├── ExecutorName: string       (партнёр или сторонняя организация)
├── ExecutorContacts: string?  (контактные данные исполнителя)
├── Operations: List<string>   (перечень работ; хранится как jsonb в БД)
├── DocumentUrl: string        (PDF наряд-заказ — ОБЯЗАТЕЛЕН, иммутабелен)
└── IsDeleted: bool            (soft-delete)
│
├── Factory method:
│   └── Create(vehicleId, title, description, performedAt, cost,
│             executorName, executorContacts?, operations, documentUrl)
│       ↳ throws DomainException если documentUrl пустой
│       ↳ публикует ServiceRecordCreatedEvent
│
└── Business operations:
    ├── Update(title, description, performedAt, cost, executorName,
    │         executorContacts?, operations)
    │   ↳ DocumentUrl не меняется при обновлении (иммутабелен)
    └── Delete() → IsDeleted = true
```

**Domain Events:**
- `ServiceRecordCreatedEvent(ServiceRecordId, VehicleId)` — публикуется при создании

**Бизнес-правила:**
- Каждая запись ОБЯЗАНА содержать PDF-документ. История публично доступна по VIN.
- `DocumentUrl` иммутабелен — задаётся при создании, не меняется при `Update`.
- Удаление — только soft-delete.
- EF Core `HasQueryFilter(r => !r.IsDeleted)` применяется глобально.

---

### Chat & Message — **реализованы** (AUT-17, AUT-19, AUT-20 / Epic AUT-4)

**Файлы:** `Domain/Chats/Chat.cs`, `Domain/Chats/Message.cs`, `Domain/Chats/DiagnosticsInput.cs`, `Domain/Chats/WorkClarificationInput.cs`

```
Chat : AggregateRoot<Guid>
├── CustomerId: Guid
├── VehicleId: Guid?                   (опционально — привязка к конкретному авто)
├── Mode: ChatMode                     (FaultHelp | WorkClarification | PartnerAdvice)
├── Status: ChatStatus                 (Active | AwaitingUserAnswers | FinalAnswerSent | Completed)
├── Title: string                      (пользовательское название сессии, max 200)
├── CreatedAt: DateTime
├── AllowOneAdditionalQuestion: bool   (true после FinalAnswerSent; один доп. вопрос разрешён)
└── Messages: List<Message>            (private backing field, PropertyAccessMode.Field)
│
├── Factory method:
│   └── Create(customerId, mode, title, vehicleId?)
│       ↳ throws DomainException если customerId == Guid.Empty
│       ↳ throws DomainException если title пустой
│
├── Business operations:
│   ├── AddExchange(userContent, assistantContent, diagnosticResultJson?) → IReadOnlyList<Message>
│   │   — валидный обмен; возвращает 2 новых сообщения для явной регистрации в EF Core
│   ├── AddInvalidUserMessage(userContent) → Message
│   │   — off-topic, не уменьшает квоту; возвращает новое сообщение для регистрации в EF Core
│   └── CanReceiveMessage() → bool
│
└── FaultHelp state transitions (только Mode = FaultHelp):
    ├── TransitionToAwaitingAnswers()  Active → AwaitingUserAnswers
    ├── TransitionBackToActive()       AwaitingUserAnswers → Active
    ├── TransitionToFinalAnswerSent()  Active → FinalAnswerSent (AllowOneAdditionalQuestion = true)
    └── Complete()                     any → Completed
```

**ChatStatus enum:**
```csharp
enum ChatStatus
{
    Active,               // чат открыт
    AwaitingUserAnswers,  // только FaultHelp: ждём ответа на уточняющий вопрос
    FinalAnswerSent,      // только FaultHelp: диагноз отправлен, один доп. вопрос разрешён
    Completed             // чат закрыт
}
```

**FaultHelp (Mode 1) — стейт-машина:**
```
Active → AwaitingUserAnswers ↔ Active → FinalAnswerSent → Completed
```
После `FinalAnswerSent` разрешён ровно 1 дополнительный вопрос, после ответа → `Completed`.

**WorkClarification (Mode 2) — одношаговый:**
```
Active → (ProcessWorkClarificationInitialAsync) → Completed
```
Никаких follow-up. Форма → LLM → чат сразу закрывается.

---

**DiagnosticsInput** _(Domain/Chats/DiagnosticsInput.cs)_ — входная форма Mode 1:
```
├── Symptoms: string           (обязателен)
├── RecentEvents: string?
└── PreviousIssues: string?
```

**WorkClarificationInput** _(Domain/Chats/WorkClarificationInput.cs)_ — входная форма Mode 2:
```
├── WorksPerformed: string     (перечень работ и деталей; обязателен)
├── WorkReason: string         (предлог для выполнения работ; обязателен)
├── LaborCost: decimal         (стоимость работ)
├── PartsCost: decimal         (стоимость деталей)
└── Guarantees: string?        (гарантии и обещания сервиса)
```

---

**Message** _(Domain/Chats/Message.cs)_:
```
Message : Entity<Guid>
├── ChatId: Guid
├── Role: MessageRole          (User | Assistant)
├── Content: string
├── IsValid: bool              (false — off-topic/rejected; не уменьшает квоту подписки)
├── DiagnosticResultJson: string?  (сериализованный DiagnosticsLlmResult; только для FaultHelp diagnostic_result, null в остальных случаях)
└── CreatedAt: DateTime
```

**Фабрики Message (internal — только для Chat):**
- `CreateUserMessage(chatId, content)` — IsValid=true
- `CreateAssistantMessage(chatId, content, diagnosticResultJson?)` — IsValid=true
- `CreateInvalidUserMessage(chatId, content)` — IsValid=false

**Бизнес-правила:**
- Режим `FaultHelp` и `WorkClarification` — только `SubscriptionStatus = Premium`
- Режим `PartnerAdvice` — бесплатный для всех клиентов
- Topic guard: off-topic запросы сохраняются с `IsValid=false`, но не уменьшают квоту
- При `ChatStatus = AwaitingUserAnswers` классификатор пропускается — ответ пользователя на уточняющий вопрос всегда валиден
- История, передаваемая LLM, фильтрует `IsValid=false` сообщения
- EF: cascade delete сообщений при удалении чата
- **EF Core tracking:** новые `Message`-сущности, добавленные через `AddExchange`/`AddInvalidUserMessage`, должны явно регистрироваться через `IChatRepository.AddMessages()` — backing-field коллекция не отслеживается автоматически при загруженном агрегате

---

### InvalidChatRequest — **реализован** (AUT-19 / Epic AUT-4)

**Файл:** `Domain/Chats/InvalidChatRequest.cs`

```
InvalidChatRequest : Entity<Guid>
├── ChatId: Guid
├── CustomerId: Guid
├── UserInput: string          (текст отклонённого запроса)
├── RejectionReason: string    (off_topic | missing_context | unsafe | out_of_scope)
└── CreatedAt: DateTime        (UTC)
│
└── Factory method:
    └── Create(chatId, customerId, userInput, rejectionReason)
```

Одна запись на каждый отклонённый запрос. Используется для аудита off-topic обращений.

---

### Subscription — **реализовано** (AUT-84/85)

Подписка встроена в агрегат `Customer` (не отдельная сущность).
Биллинг через `IBillingService` (интеграция Lemon Squeezy — планируется в Epic AUT-5).

**Тарифы:**
```
None    → free tier, 0 запросов
Normal  → $4.99/мес,  30 запросов/мес
Pro     → $7.99/мес,  100 запросов/мес
Max     → $12.99/мес, 300 запросов/мес
Разовое пополнение → через POST /api/clients/me/subscription/topup
```

**Бизнес-правила:**
- `AiRequestsRemaining` уменьшается только Режимами 1 (FaultHelp) и 2 (WorkClarification)
- Режим 3 (PartnerAdvice) требует подписки, но счётчик **не уменьшает**
- Off-topic запросы (IsValid=false) никогда не уменьшают счётчик
- При `AiRequestsRemaining = 0` → ответ `CHAT_009 QuotaExceeded`
- `ActivateSubscription(plan)` сбрасывает счётчик до месячного лимита плана

---

### InvalidChatRequest (Epic AUT-4)

```
InvalidChatRequest : Entity<Guid>
├── CustomerId: Guid
├── InvalidAttemptsCount: int
├── LastInvalidAt: DateTime
└── Details: string            (JSON-массив: [{text, mode, timestamp}, ...])
```

---

### Partner — **реализован** (AUT-24 / Epic AUT-6)

**Файл:** `Domain/Partners/Partner.cs`

```
Partner : AggregateRoot<Guid>
├── Name: string
├── Type: PartnerType           (AutoService, CarWash, Towing, AutoShop, Other)
├── Specialization: string
├── Description: string
├── Address: string
├── Location: GeoPoint          (Value Object: Lat, Lng)
├── WorkingHours: WorkingSchedule (Value Object: OpenFrom, OpenTo, WorkDays)
├── Contacts: PartnerContacts   (Value Object: Phone, Website?, MessengerLinks?)
├── LogoUrl: string?
├── IsVerified: bool            (default: false)
├── IsActive: bool              (default: false; true только после верификации)
├── IsPotentiallyUnfit: bool    (>= 5 оценок ниже 3)
├── ShowBannersToAnonymous: bool
├── AccountUserId: Guid         (учётная запись партнёра; УНИКАЛЕН — один партнёр на аккаунт)
└── IsDeleted: bool             (soft-delete)
│
├── Factory method:
│   └── Create(name, type, specialization, description, address,
│             location, workingHours, contacts, accountUserId)
│       ↳ throws DomainException если name пустой или accountUserId == Guid.Empty
│       ↳ публикует PartnerRegisteredEvent
│       ↳ IsVerified = false, IsActive = false по умолчанию
│
└── Business operations:
    ├── UpdateProfile(name, spec, desc, addr, location, workingHours, contacts)
    ├── UpdateLogo(logoUrl)
    ├── Verify()                      → IsVerified = true, IsActive = true
    ├── Deactivate()                  → IsActive = false
    ├── SetBannerVisibility(bool)
    ├── RecalculateFitnessFlag(count) → IsPotentiallyUnfit = count >= 5
    └── Delete()                      → IsDeleted = true, IsActive = false
```

**Domain Events:**
- `PartnerRegisteredEvent(PartnerId, AccountUserId)` — публикуется при создании

**Value Objects:**
- `GeoPoint(Lat, Lng)` — Lat ∈ [-90, 90], Lng ∈ [-180, 180]; хранится как owned entity (location_lat, location_lng)
- `WorkingSchedule(OpenFrom, OpenTo, WorkDays)` — WorkDays non-empty; хранится как owned entity
- `PartnerContacts(Phone, Website?, MessengerLinks?)` — Phone non-empty; хранится как owned entity

**Бизнес-правила:**
- Один аккаунт → один партнёрский профиль (unique index на AccountUserId)
- Новый партнёр не активен до верификации администратором
- Soft-delete: `HasQueryFilter(p => !p.IsDeleted)`

**Бизнес-правило автопометки:**
```
IsPotentiallyUnfit = true  ←  если Reviews.Count(r => r.Rating < 3) >= 5
```
Пересчитывается вызовом `RecalculateFitnessFlag(lowRatingCount)` из хендлера после добавления отзыва.

---

### Review — **реализован** (AUT-25 / Epic AUT-6)

**Файл:** `Domain/Reviews/Review.cs`

```
Review : AggregateRoot<Guid>
├── PartnerId: Guid
├── CustomerId: Guid
├── Rating: int                 (1–5)
├── Comment: string             (обязателен, max 2000 символов)
├── Basis: ReviewBasis          (RecommendedByAI | ExecutorInServiceRecord)
├── InteractionReferenceId: Guid  (ServiceRecord.Id или Chat.Id)
├── CreatedAt: DateTime         (UTC, задаётся при создании)
└── IsDeleted: bool             (soft-delete)
│
├── Factory method:
│   └── Create(partnerId, customerId, rating, comment, basis, interactionReferenceId)
│       ↳ throws DomainException если partnerId/customerId/interactionReferenceId == Guid.Empty
│       ↳ throws DomainException если rating вне диапазона [1, 5]
│       ↳ throws DomainException если comment пустой
│
└── Business operations:
    └── Delete() → IsDeleted = true
```

**Enum ReviewBasis:**
```csharp
enum ReviewBasis { RecommendedByAI, ExecutorInServiceRecord }
```

**Бизнес-правило:** Оставить отзыв можно ТОЛЬКО если:
- Партнёр был рекомендован клиенту через AI-чат (`RecommendedByAI`) **ИЛИ**
- Партнёр указан исполнителем в записи о работе автомобиля клиента (`ExecutorInServiceRecord`)

Дублирование проверяется уникальным составным индексом на (PartnerId, CustomerId, Basis, InteractionReferenceId).

**EF Core:** таблица `reviews`, HasQueryFilter по `IsDeleted`, FK на `partners` с `DeleteBehavior.Restrict`.

---

### AdCampaign — **реализован** (AUT-26 / Epic AUT-6)

**Файл:** `Domain/AdCampaigns/AdCampaign.cs`

```
AdCampaign : AggregateRoot<Guid>
├── PartnerId: Guid
├── Type: AdType                (OfferBlock | Banner)
├── TargetCategory: PartnerType
├── Content: string             (текст / URL изображения; max 2048)
├── StartsAt: DateTime          (UTC)
├── EndsAt: DateTime            (UTC)
├── IsActive: bool              (default: false; активируется admin-ом)
├── ShowToAnonymous: bool       (показывать анонимным пользователям)
├── Stats: AdStats              (owned VO: Impressions, Clicks)
└── IsDeleted: bool             (soft-delete)
│
├── Factory method:
│   └── Create(partnerId, type, targetCategory, content, startsAt, endsAt, showToAnonymous)
│       ↳ throws DomainException если partnerId == Guid.Empty
│       ↳ throws DomainException если content пустой
│       ↳ throws DomainException если endsAt <= startsAt
│       ↳ IsActive = false по умолчанию
│
└── Business operations:
    ├── Update(type, targetCategory, content, startsAt, endsAt, showToAnonymous)
    ├── Activate()     → IsActive = true
    ├── Deactivate()   → IsActive = false
    ├── IsVisibleTo(isAuthenticated, isPartner) → bool
    │   ↳ false если IsDeleted или !IsActive
    │   ↳ false если isPartner (партнёры никогда не видят рекламу — правило 13)
    │   ↳ false если !isAuthenticated && !ShowToAnonymous
    └── Delete()       → IsDeleted = true, IsActive = false
```

**Enum AdType:**
```csharp
enum AdType { OfferBlock, Banner }
```

**Value Object AdStats:**
```
AdStats
├── Impressions: int
├── Clicks: int
├── RecordImpression()
└── RecordClick()
```

**Бизнес-правила:**
- `IsActive = false` по умолчанию; активируется администратором (метод готов для будущего admin-модуля)
- Партнёры никогда не видят рекламные баннеры (метод `IsVisibleTo` принудительно возвращает `false`)
- Анонимным пользователям показывается только при `ShowToAnonymous = true`
- Ротация — на уровне Application: `OrderBy(_ => Guid.NewGuid())`
- Таргетинг по `TargetCategory` (тип партнёра = категория услуг)
- Soft-delete: `HasQueryFilter(c => !c.IsDeleted)`

---

### PlatformReview (Epic AUT-8)

```
PlatformReview : AggregateRoot<Guid>
├── CustomerId: Guid
├── Text: string
├── Rating: int?                (опционально)
├── CreatedAt: DateTime
├── IsApproved: bool            (одобрен для отображения на лендинге)
└── IsDeleted: bool             (soft-delete)
```

**Бизнес-правило:** На лендинге отображаются только записи с `IsApproved = true`.

---

## Enums (обновлённые)

### ChatbotPlan

```csharp
enum ChatbotPlan { Regular, Pro, Maximum }
```

### ChatMode

```csharp
enum ChatMode { FaultHelp, WorkClarification, PartnerAdvice }
```

### AuditOperationType

```csharp
enum AuditOperationType { Created, Updated, Deleted }
```

---

## Ключевые бизнес-инварианты (краткая шпаргалка)

| # | Правило |
|---|---------|
| 1 | VIN уникален — не может быть двух авто с одинаковым VIN |
| 2 | История работ публична — доступна всем по VIN |
| 3 | Статусы «Утилизирован» и «Разобран» требуют PDF-документа |
| 4 | Оценить партнёра можно только при наличии факта взаимодействия |
| 5 | ≥ 5 оценок ниже 3 → автопометка «Потенциально профнепригодный» |
| 6 | AI-чат (Режимы 1 и 2) доступен только при активной подписке чатбота |
| 7 | АвтоПомощник отвечает строго в 3 режимах; прочие вопросы отклоняются |
| 8 | Новые партнёры публикуются только после верификации администратором |
| 9 | API-ключ LLM-провайдера используется только на бэкенде |
| 10 | Все удаления сущностей — soft-delete (IsDeleted = true) |
| 11 | Все CRUD-операции над Customer, Vehicle, ServiceRecord — фиксируются в AuditLogs |
| 12 | Невалидный запрос к чатботу не уменьшает счётчик запросов подписки |
| 13 | Партнёр видит только свой кабинет; рекламные баннеры партнёрам не показываются |
| 14 | На лендинге отображаются только одобренные администратором отзывы о платформе |
