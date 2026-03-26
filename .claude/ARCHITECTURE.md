# AutoHelper — Backend Architecture

## Принцип: Clean Architecture

Бэкенд строго следует Clean Architecture. Зависимости направлены только **внутрь** (к Domain). Внешние слои знают о внутренних, но не наоборот.

```
┌──────────────────────────────────────────────────────────────┐
│  AutoHelper.Api          (Presentation)                       │
│  — Minimal API Endpoints, Middleware, DI-регистрация          │
│  — Знает об Application, Infrastructure                       │
├──────────────────────────────────────────────────────────────┤
│  AutoHelper.Application  (Use Cases)                          │
│  — MediatR Commands/Queries/Handlers                          │
│  — Interfaces (IRepository, IUnitOfWork, IJwtTokenService…)   │
│  — Знает только о Domain                                      │
├──────────────────────────────────────────────────────────────┤
│  AutoHelper.Infrastructure (Adapters / Infrastructure)        │
│  — EF Core AppDbContext, Repositories, Migrations             │
│  — JwtTokenService, PasswordHasher                            │
│  — S3/MinIO, Lemon Squeezy, ILlmProvider (OpenAI ChatGPT 5.4) │
│  — Реализует интерфейсы из Application                        │
│  — Знает о Domain и Application (только интерфейсы)           │
├──────────────────────────────────────────────────────────────┤
│  AutoHelper.Domain       (Core)                               │
│  — Entities, AggregateRoots, Value Objects                    │
│  — Domain Events                                              │
│  — Бизнес-инварианты                                          │
│  — НЕ знает ни о чём внешнем (0 зависимостей)                 │
└──────────────────────────────────────────────────────────────┘
```

---

## Слой Domain (`AutoHelper.Domain`)

### Базовые классы

```
Domain/Common/
  ├── Entity<TId>         — базовый класс сущности с Id и DomainEvents
  ├── AggregateRoot<TId>  — маркерный класс агрегата (наследует Entity)
  ├── IDomainEvent        — интерфейс доменного события
  ├── DomainException     — базовое исключение домена
  └── NotFoundException   — исключение «не найдено»
```

### Агрегаты (реализованы)

| Агрегат | Файл | Описание |
|---------|------|----------|
| `Customer` | `Customers/Customer.cs` | Клиент. Поддерживает Local (email+password) и Google OAuth |
| `RefreshToken` | `Customers/RefreshToken.cs` | Refresh-токен, привязан к Customer |
| `Vehicle` | `Vehicles/Vehicle.cs` | Автомобиль. VIN уникален. Реализован в AUT-12 |

### Enums

| Enum | Значения |
|------|---------|
| `AuthProvider` | `Local`, `Google` |
| `SubscriptionStatus` | `Free`, `Premium`, `Suspended` |
| `VehicleStatus` | `Active`, `ForSale`, `InRepair`, `Recycled`, `Dismantled` |

### Агрегаты к реализации (по требованиям)

| Агрегат | Предполагаемый модуль |
|---------|-----------------------|
| `ServiceRecord` | История работ (Epic AUT-3) |
| `AuditLog` | Аудит-лог (Epic AUT-150) |
| `Chat` / `Message` | AI-чат (Epic AUT-4) |
| `ChatbotSubscription` | Биллинг / чатбот (Epic AUT-4, AUT-5) |
| `InvalidChatRequest` | Чатбот (Epic AUT-4) |
| `Partner` | Партнёры (Epic AUT-6) |
| `Review` | Рейтинги (Epic AUT-6) |
| `AdCampaign` | Реклама (Epic AUT-6) |
| `PlatformReview` | Блог лендинга (Epic AUT-8) |

---

## Слой Application (`AutoHelper.Application`)

### Паттерн CQRS через MediatR

Каждая фича — отдельная папка в `Features/<FeatureName>/`:

```
Features/Auth/
  ├── Register/
  │   ├── RegisterCustomerCommand.cs   # IRequest<Result<Guid>>
  │   ├── RegisterCustomerHandler.cs
  │   └── RegisterCustomerValidator.cs
  ├── Login/
  │   ├── LoginCommand.cs
  │   ├── LoginHandler.cs
  │   └── LoginValidator.cs
  ├── RefreshToken/
  │   ├── RefreshTokenCommand.cs
  │   ├── RefreshTokenHandler.cs
  │   └── RefreshTokenValidator.cs
  └── Logout/
      ├── LogoutCommand.cs
      ├── LogoutHandler.cs
      └── LogoutValidator.cs

Features/Clients/
  ├── GetMyProfile/
  │   ├── GetMyProfileQuery.cs         # IRequest<Result<ClientProfileResponse>>
  │   └── GetMyProfileQueryHandler.cs
  ├── UpdateMyProfile/
  │   ├── UpdateMyProfileCommand.cs
  │   ├── UpdateMyProfileCommandHandler.cs
  │   └── UpdateMyProfileCommandValidator.cs
  ├── ChangePassword/
  │   ├── ChangePasswordCommand.cs
  │   ├── ChangePasswordCommandHandler.cs
  │   └── ChangePasswordCommandValidator.cs
  └── UploadAvatar/
      ├── UploadAvatarCommand.cs
      └── UploadAvatarCommandHandler.cs

Features/Vehicles/
  └── GetVehicleOwner/
      ├── GetVehicleOwnerQuery.cs      # IRequest<Result<VehicleOwnerResponse>>
      └── GetVehicleOwnerQueryHandler.cs
```

### Интерфейсы (определяются в Application, реализуются в Infrastructure)

```
Common/Interfaces/
  ├── IUnitOfWork              — SaveChangesAsync
  ├── ICustomerRepository      — GetByEmailAsync, GetByIdAsync, ExistsByEmailAsync, Add
  ├── IVehicleRepository       — GetByVinAsync, ExistsByVinAsync, Add
  ├── IRefreshTokenRepository  — Add, GetByTokenAsync, MarkAsRevokedAsync
  ├── IJwtTokenService         — GenerateAccessToken, GenerateRefreshToken, ValidateToken
  ├── IPasswordHasher          — Hash, Verify
  ├── IStorageService          — UploadAsync, CompressAsync
  ├── ILlmProvider             — SendAsync(prompt, context, locale) — абстракция LLM (начальная реализация: OpenAI ChatGPT 5.4)
  ├── IBillingService          — абстракция биллинга (начальная реализация: Lemon Squeezy)
  ├── IAuditLogService         — LogAsync(operationType, entityType, entityId, performedBy, additionalInfo?)
  └── ICurrentUser             — UserId (из JWT claims)
```

### MediatR Pipeline Behaviors

```
Common/Behaviors/
  ├── LoggingBehavior.cs       — структурированное логирование всех запросов
  └── ValidationBehavior.cs    — автовалидация через FluentValidation
```

### Result паттерн

Все Handler-ы возвращают `Result<T>` (или `Result`). Никаких исключений для бизнес-логики — только `Result.Failure("error message")`.

---

## Слой Infrastructure (`AutoHelper.Infrastructure`)

### Persistence

```
Persistence/
  ├── AppDbContext.cs             — DbContext + IUnitOfWork
  │                                 SaveChangesAsync диспатчит DomainEvents через MediatR
  │                                 DbSets: Customers, RefreshTokens, Vehicles
  ├── DatabaseMigrator.cs         — Автомиграция при старте (если AutoMigrateOnStartup=true)
  ├── Configurations/
  │   ├── CustomerConfiguration.cs
  │   ├── RefreshTokenConfiguration.cs
  │   └── VehicleConfiguration.cs
  ├── Migrations/
  │   ├── 20260324211723_InitialCreate      — таблицы Customers, RefreshTokens
  │   ├── 20260324221025_AddAvatarUrl       — поле AvatarUrl в Customers
  │   └── 20260324223918_AddVehicles        — таблица Vehicles (unique VIN)
  └── Repositories/
      ├── CustomerRepository.cs
      ├── VehicleRepository.cs
      └── RefreshTokenRepository.cs
```

### Security

```
Security/
  ├── JwtTokenService.cs   — Генерация/валидация JWT (HS256)
  ├── JwtSettings.cs       — Конфигурация (секрет, issuer, audience, TTL)
  └── PasswordHasher.cs    — PBKDF2 хэширование паролей
```

### Storage

```
Storage/
  ├── S3StorageService.cs  — Загрузка файлов в MinIO/S3 (аватары, PDF)
  └── StorageSettings.cs   — Конфигурация (endpoint, keys, bucket)
```

### Common

```
Common/
  └── CurrentUser.cs       — ICurrentUser: извлекает UserId из JWT claims (NameIdentifier / sub)
```

---

## Слой Api (`AutoHelper.Api`)

### Организация по фичам

```
Features/
  ├── Auth/
  │   └── AuthEndpoints.cs         — MapAuthEndpoints() → /api/auth/*
  ├── Clients/
  │   └── ClientsEndpoints.cs      — MapClientsEndpoints() → /api/clients/* [requires auth]
  └── Vehicles/
      └── VehiclesEndpoints.cs     — MapVehiclesEndpoints() → /api/vehicles/*
```

Каждый новый модуль добавляет свой `XxxEndpoints.cs` с методом `MapXxxEndpoints()`, который вызывается в `Program.cs`.

### Middleware

```
Middleware/
  ├── CorrelationIdMiddleware.cs    — добавляет X-Correlation-Id к каждому запросу
  └── HttpLoggingMiddleware.cs      — структурированное логирование req/res (маскировка sensitive headers)
```

### GlobalExceptionHandler

`GlobalExceptionHandler.cs` — перехватывает необработанные исключения, возвращает `ProblemDetails`.

### Program.cs порядок middleware

```csharp
app.UseExceptionHandler();
app.UseMiddleware<CorrelationIdMiddleware>();  // 1. Correlation ID первым
app.UseMiddleware<HttpLoggingMiddleware>();    // 2. Logging
// (dev only) app.MapOpenApi(); app.MapScalarApiReference();
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health");
app.MapAuthEndpoints();
app.MapClientsEndpoints();
app.MapVehiclesEndpoints();
// ... MapXxxEndpoints() для каждого нового модуля
```

---

## Конфигурация (appsettings.json)

```json
{
  "ConnectionStrings": {
    "Database": "Host=localhost;Port=5432;Database=autohelper;..."
  },
  "Jwt": {
    "Secret": "<secret>",
    "Issuer": "autohelper-api",
    "Audience": "autohelper-api",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 30
  },
  "Storage": {
    "ServiceUrl": "http://localhost:9000",
    "AccessKey": "...",
    "SecretKey": "...",
    "BucketName": "autohelper"
  },
  "LemonSqueezy": { "ApiKey": "...", "WebhookSecret": "...", "StoreId": "..." },
  "LLM": { "Provider": "OpenAI", "ApiKey": "...", "Model": "gpt-5.4" },
  "Database": { "AutoMigrateOnStartup": true }
}
```

**В Docker Compose переменные окружения задаются через `.env` файл** (см. `docker-compose.yml`).

---

## Тесты (`backend/tests/`)

```
tests/
  ├── AutoHelper.Domain.Tests/          — Unit-тесты доменной логики (Customer, Vehicle, RefreshToken)
  ├── AutoHelper.Application.Tests/     — Unit-тесты handlers + JwtTokenService
  └── AutoHelper.Infrastructure.Tests/  — Integration-тесты (планируется: WebApplicationFactory)
```

- Запуск: `dotnet test` из папки `backend/`
- **Обязательно перед каждым коммитом** (см. CONVENTIONS.md)
