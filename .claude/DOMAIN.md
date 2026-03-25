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
│   └── UpdateDetails(brand, model, year, color?, mileage)
│       ↳ обновляет мутабельные поля; VIN и OwnerId неизменны
│
└── (StatusDetails — планируется в AUT-57..59)
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
├── Operations: List<string>   (перечень работ)
└── DocumentUrl: string        (PDF наряд-заказ — ОБЯЗАТЕЛЕН)
```

**Бизнес-правило:** Каждая запись ОБЯЗАНА содержать PDF-документ. История публично доступна по VIN.

---

### Chat & Message (Epic AUT-4)

```
Chat : AggregateRoot<Guid>
├── CustomerId: Guid
├── VehicleId: Guid?           (опционально — привязка к конкретному авто)
├── CreatedAt: DateTime
└── Messages: List<Message>

Message : Entity<Guid>
├── ChatId: Guid
├── Role: MessageRole          (User / Assistant)
├── Content: string
└── CreatedAt: DateTime
```

**Бизнес-правило:** Доступ к чату только при `SubscriptionStatus = Premium`.

---

### Subscription (Epic AUT-5)

```
Subscription : AggregateRoot<Guid>
├── CustomerId: Guid
├── Plan: SubscriptionPlan     (Free / Premium)
├── Status: SubscriptionStatus (Active / Cancelled / PastDue)
├── StripeSubscriptionId: string?
├── StripeCustomerId: string?
├── CurrentPeriodStart: DateTime
├── CurrentPeriodEnd: DateTime
└── CancelledAt: DateTime?
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
├── IsPotentiallyUnfit: bool    (авто: >= 5 оценок ниже 3)
└── Documents: List<string>     (URL лицензий, сертификатов)
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
└── CreatedAt: DateTime
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
└── Stats: AdStats              (показы, клики)
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
| 6 | AI-чат доступен только при Premium подписке |
| 7 | АвтоПомощник отвечает строго в 3 режимах; прочие вопросы отклоняются |
| 8 | Новые партнёры публикуются только после верификации администратором |
| 9 | API-ключ OpenAI используется только на бэкенде |
