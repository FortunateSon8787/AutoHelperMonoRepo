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
├── SubscriptionStatus: SubscriptionStatus
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
└── Business operations:
    ├── UpdateGoogleInfo(googlePicture?, googleRefreshToken?)
    ├── UpdateContacts(contacts?)
    ├── UpdateProfile(name, contacts?)
    ├── UpdateAvatar(avatarUrl)
    └── ChangePassword(newPasswordHash) → bool
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
    Premium,    // Полный доступ ко всем функциям
    Suspended   // Подписка приостановлена (ошибка оплаты)
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
| `InRepair` | В ремонте | `PartnerName` (планируется, AUT-58) |
| `Recycled` | Утилизирован | `DocumentUrl` PDF (планируется, AUT-59) |
| `Dismantled` | Разобран | `DocumentUrl` PDF (планируется, AUT-59) |

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

### ServiceRecord (Epic AUT-3)

```
ServiceRecord : AggregateRoot<Guid>
├── VehicleId: Guid
├── Title: string
├── Description: string
├── PerformedAt: DateTime
├── Cost: decimal
├── ExecutorName: string       (партнёр или сторонняя организация)
├── ExecutorContacts: string?  (контактные данные исполнителя)
├── Operations: List<string>   (перечень работ)
├── DocumentUrl: string        (PDF наряд-заказ — ОБЯЗАТЕЛЕН)
└── IsDeleted: bool            (soft-delete)
```

**Бизнес-правила:**
- Каждая запись ОБЯЗАНА содержать PDF-документ. История публично доступна по VIN.
- При обновлении фиксируется запись в AuditLog с JSON старой сущности (ключ `OldEntity`).
- Удаление — только soft-delete.

---

### Chat & Message (Epic AUT-4)

```
Chat : AggregateRoot<Guid>
├── CustomerId: Guid
├── VehicleId: Guid?           (опционально — привязка к конкретному авто)
├── Mode: ChatMode             (FaultHelp | WorkClarification | PartnerAdvice)
├── CreatedAt: DateTime
└── Messages: List<Message>

Message : Entity<Guid>
├── ChatId: Guid
├── Role: MessageRole          (User | Assistant)
├── Content: string
├── IsValid: bool              (false — если запрос был невалидным)
└── CreatedAt: DateTime
```

**Бизнес-правила:**
- Доступ к Режимам 1 и 2 только при активной подписке чатбота.
- Невалидный запрос не уменьшает счётчик запросов.
- Ответы генерируются на языке текущей локали сайта.

---

### ChatbotSubscription (Epic AUT-4, AUT-5)

```
ChatbotSubscription : AggregateRoot<Guid>
├── CustomerId: Guid
├── Plan: ChatbotPlan          (Regular | Pro | Maximum)
├── Status: SubscriptionStatus (Active | Cancelled | PastDue)
├── LemonSqueezySubscriptionId: string?
├── LemonSqueezyCustomerId: string?
├── RequestsRemaining: int     (оставшиеся запросы в текущем периоде)
├── RequestsTotal: int         (лимит по плану)
├── PeriodStart: DateTime
├── PeriodEnd: DateTime
└── CancelledAt: DateTime?
```

**Тарифы:**
```
Regular  → $4.99/мес,  10 запросов (Режим 1 + Режим 2 суммарно)
Pro      → $7.99/мес,  20 запросов
Maximum  → $12.99/мес, 40 запросов
Разовое пополнение → $3 / 10 запросов
```

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

### Partner (Epic AUT-6)

```
Partner : AggregateRoot<Guid>
├── Name: string
├── Type: PartnerType           (AutoService, CarWash, Towing, AutoShop, Other)
├── Specialization: string
├── Description: string
├── Address: string
├── Location: GeoPoint          (lat/lng)
├── WorkingHours: WorkingSchedule
├── Contacts: PartnerContacts   (phone, website, messengers)
├── LogoUrl: string?
├── IsVerified: bool
├── IsActive: bool
├── IsPotentiallyUnfit: bool    (>= 5 оценок ниже 3)
├── Documents: List<string>     (URL лицензий, сертификатов)
├── AccountUserId: Guid         (учётная запись партнёра)
├── ShowBannersToAnonymous: bool (разрешить показ баннеров анонимным пользователям)
└── IsDeleted: bool             (soft-delete)
```

**Бизнес-правило автопометки:**
```
IsPotentiallyUnfit = true  ←  если Reviews.Count(r => r.Rating < 3) >= 5
```

---

### Review (Epic AUT-6)

```
Review : AggregateRoot<Guid>
├── PartnerId: Guid
├── CustomerId: Guid
├── Rating: int                 (1–5)
├── Comment: string             (обязателен)
├── InteractionType: ReviewBasis (RecommendedByAI | ExecutorInServiceRecord)
├── InteractionReferenceId: Guid  (chatId или serviceRecordId)
├── CreatedAt: DateTime
└── IsDeleted: bool             (soft-delete)
```

**Бизнес-правило:** Оставить отзыв можно ТОЛЬКО если:
- Партнёр был рекомендован клиенту через AI-чат **ИЛИ**
- Партнёр указан исполнителем в записи о работе автомобиля клиента

---

### AdCampaign (Epic AUT-6)

```
AdCampaign : AggregateRoot<Guid>
├── PartnerId: Guid
├── Type: AdType                (OfferBlock | Banner)
├── TargetCategory: PartnerType
├── Content: string             (текст / URL изображения)
├── StartsAt: DateTime
├── EndsAt: DateTime
├── IsActive: bool
├── ShowToAnonymous: bool       (показывать анонимным пользователям)
├── Stats: AdStats              (показы, клики)
└── IsDeleted: bool             (soft-delete)
```

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
