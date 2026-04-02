# AutoHelper — Conventions & Workflow

## Актуализация контекстных файлов (ОБЯЗАТЕЛЬНО после выполнения Story)

После завершения каждой Story-задачи Jira необходимо обновить все контекстные файлы репозитория, которые связаны с реализованным функционалом:

| Что изменилось | Какие файлы обновить |
|----------------|----------------------|
| Новый API-эндпоинт | `.claude/API.md` |
| Новая доменная сущность / агрегат | `.claude/DOMAIN.md` |
| Новая/изменённая бизнес-логика | `.claude/AutoHelper_Requirements_v1.3.md` |
| Общие соглашения / конвенции | `.claude/CONVENTIONS.md` |
| Декомпозиция задач Jira | `.claude/JIRA_DECOMPOSITION.md` |

**Правило:** Перемещай реализованные сущности/эндпоинты из раздела «планируется» в раздел «реализовано» и дополняй описание актуальными деталями.

---

## Jira Workflow (ОБЯЗАТЕЛЬНО после выполнения задачи)

Если задача выдана со ссылкой на Jira-тикет или с явным упоминанием таски (например, "выполни AUT-44"), то **после мержа в main** необходимо актуализировать таску в Jira:

1. Перевести таску в соответствующий статус (`In Review` или `Done`) через `transitionJiraIssue`
2. При необходимости — частично обновить описание таски через `editJiraIssue`
3. Добавить комментарий через `addCommentToJiraIssue`, который обязательно включает:
   - Краткое описание **что было сделано**
   - На что **обратить внимание** (нетривиальные решения, edge-cases, ограничения)
   - Что **изменилось в структуре и взаимоотношении компонентов** (новые зависимости, изменённые контракты, side-эффекты для других модулей)
4. Если есть worklog — залогировать потраченное время через `addWorklogToJiraIssue`

Используй MCP-инструменты Atlassian (`transitionJiraIssue`, `editJiraIssue`, `addCommentToJiraIssue`, `addWorklogToJiraIssue`) для этих действий.

---

## Git + Jira Workflow (ОБЯЗАТЕЛЬНО при выполнении Jira-таски)

### Шаг −1: Изучение таски и субтасок (ПЕРЕД ЛЮБОЙ РЕАЛИЗАЦИЕЙ)

Перед началом работы **обязательно** прочитать таску полностью через `getJiraIssue`:

1. Прочитать описание родительской таски.
2. **Если у таски есть субтаски** — прочитать каждую субтаску через `getJiraIssue`:
   - Описание субтаски может содержать требования, edge-cases, ограничения или нюансы реализации, которые не отражены в родительской таске.
   - Реализация должна учитывать **все** описания субтасок, а не только заголовки.
3. Составить полное понимание объёма работы до написания первой строки кода.

> **Правило:** Субтаска = отдельная единица требований. Никогда не пропускай чтение описания субтасок.

### Шаг 0: Подготовка ветки main перед началом работы

Перед тем как приступить к выполнению задачи:

```bash
# Переключиться на main и актуализировать
git checkout main
git pull origin main

# Собрать проект
cd backend
dotnet build

# Запустить тесты
dotnet test
```

- Если сборка **сломана** или тесты **не прошли** — исправить проблему, закоммитить исправление в `main`, снова выполнить `git push origin main` и убедиться что всё зелёное.
- Только после этого переходить к шагу 1.

### Шаг 1: Создать feature-ветку

```bash
# Название ветки — по заголовку таски (kebab-case, номер таски в префиксе)
# Примеры: feature/aut-13-vehicle-crud, feature/aut-44-service-records
git checkout -b feature/aut-<номер>-<краткое-описание>
```

### Шаг 2: Реализация задачи

Писать код согласно скиллам и соглашениям. Промежуточные коммиты разрешены.

### Шаг 3: Финальная сборка и тесты перед мержем

```bash
cd backend
dotnet build
```

Коммит допускается **только при 0 ошибках сборки**.

```bash
dotnet test
```

Коммит допускается **только при прохождении всех тестов**.

### Шаг 4: Финальный коммит в feature-ветке

Сообщение коммита должно содержать:
- Заголовок таски (номер + название)
- Краткое описание выполненной работы

```
AUT-13: Add vehicle CRUD endpoints

Implement create/update/delete commands with FluentValidation,
EF Core configuration and migration for Vehicle aggregate.
```

### Шаг 5: Мерж в main

```bash
git checkout main
git pull origin main          # актуализировать перед мержем
git merge feature/aut-<номер>-<краткое-описание>
git push origin main
```

### Шаг 6: Проверка миграций (если менялись Domain-модели или DbContext)

```bash
# Посмотреть список миграций
dotnet ef migrations list \
  --project src/AutoHelper.Infrastructure \
  --startup-project src/AutoHelper.Api

# Создать тестовую миграцию
dotnet ef migrations add CheckMigration \
  --project src/AutoHelper.Infrastructure \
  --startup-project src/AutoHelper.Api \
  --output-dir Persistence/Migrations

# Если пустая — snapshot актуален, удалить
dotnet ef migrations remove \
  --project src/AutoHelper.Infrastructure \
  --startup-project src/AutoHelper.Api
```

Если `CheckMigration` **не пустая** → есть незафиксированные изменения → нужен ревью вручную.

**Если выполнить шаги автоматически невозможно** — уведомить разработчика и попросить сделать это вручную.

---

## EF Core Миграции

### ⚠️ КРИТИЧНО: всегда использовать `--output-dir`

```bash
dotnet ef migrations add <MigrationName> \
  --project src/AutoHelper.Infrastructure \
  --startup-project src/AutoHelper.Api \
  --output-dir Persistence/Migrations
```

Без `--output-dir Persistence/Migrations` EF Core положит файлы в `Infrastructure/Migrations/` (неверный путь).

**Правильный путь миграций:** `backend/src/AutoHelper.Infrastructure/Persistence/Migrations/`

### Прочие команды

```bash
# Применить миграции вручную
dotnet ef database update \
  --project src/AutoHelper.Infrastructure \
  --startup-project src/AutoHelper.Api

# Откатить последнюю миграцию
dotnet ef migrations remove \
  --project src/AutoHelper.Infrastructure \
  --startup-project src/AutoHelper.Api

# Сгенерировать SQL-скрипт
dotnet ef migrations script \
  --project src/AutoHelper.Infrastructure \
  --startup-project src/AutoHelper.Api
```

---

## Backend Conventions (C#/.NET)

### Именование

| Элемент | Конвенция | Пример |
|---------|-----------|--------|
| Команда MediatR | `<Action><Entity>Command` | `RegisterCustomerCommand` |
| Запрос MediatR | `Get<Entity>Query` | `GetVehicleByVinQuery` |
| Handler | `<Command/Query>Handler` | `RegisterCustomerHandler` |
| Repository interface | `I<Entity>Repository` | `ICustomerRepository` |
| Repository impl | `<Entity>Repository` | `CustomerRepository` |
| Endpoint map method | `Map<Feature>Endpoints` | `MapVehicleEndpoints` |
| EF configuration | `<Entity>Configuration` | `CustomerConfiguration` |

### Структура новой фичи (бэкенд)

```
Application/Features/<ModuleName>/
  <Action>/
    <Action><Entity>Command.cs      # или Query
    <Action><Entity>Handler.cs
    <Action><Entity>Validator.cs    # FluentValidation (если нужна)

Api/Features/<ModuleName>/
  <ModuleName>Endpoints.cs          # Minimal API endpoints
```

### Result паттерн

```csharp
// Handler возвращает Result<T>, никаких исключений для бизнес-логик
public async Task<Result<Guid>> Handle(RegisterCustomerCommand request, CancellationToken ct)
{
    if (await _repo.ExistsByEmailAsync(request.Email, ct))
        return Result.Failure<Guid>("Email already taken.");

    var customer = Customer.CreateWithPassword(request.Name, request.Email, hash);
    await _repo.AddAsync(customer, ct);
    await _uow.SaveChangesAsync(ct);
    return Result.Success(customer.Id);
}
```

### Domain Events

Агрегаты публикуют события через `AddDomainEvent(...)`. `AppDbContext.SaveChangesAsync` автоматически диспатчит их через MediatR после коммита.

```csharp
// В методе агрегата
customer.AddDomainEvent(new CustomerRegisteredEvent(customer.Id, customer.Email));
```

### Конфигурации EF Core

Каждая сущность имеет свой `IEntityTypeConfiguration<T>` в `Infrastructure/Persistence/Configurations/`. Нельзя использовать Data Annotations на доменных классах.

---

## Frontend Conventions (Next.js / TypeScript)

### Дизайн-система — СТРОГОЕ ПРАВИЛО

Все новые страницы и компоненты должны следовать дизайн-системе проекта. Подробно: `.claude/FRONTEND.md` → раздел «Дизайн-система».

**Ключевые правила стилизации:**

```tsx
// ✅ Правильно — используем токены дизайн-системы
<div className="min-h-screen bg-background">
<div className="bg-card border border-border rounded-2xl shadow-card">
<p className="text-muted-foreground">
<Button variant="accent" size="lg">

// ❌ Запрещено — хардкод цветов/размеров вне дизайн-системы
<div className="bg-gray-50">
<div className="bg-white border border-gray-200 rounded-xl">
<p className="text-gray-500">
<button className="bg-blue-600 text-white">
```

**Алерты:**
```tsx
// Ошибка
<div className="bg-destructive/5 border border-destructive/20 text-destructive rounded-xl px-4 py-3 text-sm">
// Успех
<div className="bg-success/5 border border-success/20 text-success rounded-xl px-4 py-3 text-sm">
```

**Нативные `<select>` и `<textarea>`** — использовать классы из `lib/form-styles.ts`:
```tsx
import { nativeSelectCn, nativeTextareaCn } from "@/lib/form-styles";
<select className={nativeSelectCn} />
<textarea className={nativeTextareaCn} />
```

**Загрузка / ошибка страницы** — единый паттерн:
```tsx
if (isLoading) return (
  <div className="min-h-screen flex items-center justify-center bg-background">
    <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
  </div>
);
```

### AppHeader

Для **всех** внутренних страниц (dashboard, profile, partner) используется `<AppHeader />`:
```tsx
import { AppHeader } from "@/components/AppHeader";

// Стандартное использование
<AppHeader />

// С action-кнопками (Logout и т.д.)
<AppHeader>
  <Button variant="ghost" size="sm" onClick={handleLogout}>...</Button>
</AppHeader>
```

`LocaleSwitcher` встроен в `AppHeader`. **Не добавлять** `LocaleSwitcher` напрямую на страницы.

### i18n — СТРОГОЕ ПРАВИЛО

**Все текстовые строки в UI хранятся только в файлах переводов.** Хардкод строк в компонентах — **запрещён**.

```typescript
// ✅ Правильно
const t = useTranslations('auth.login');
return <h1>{t('title')}</h1>;

// ❌ Запрещено
return <h1>Вход в систему</h1>;
return <Link href="/partner/cabinet">← Партнёрский кабинет</Link>;
```

Файлы переводов: `frontend/messages/ru.json`, `frontend/messages/en.json`

Структура ключей по модулям:
```json
{
  "auth": {
    "login": { "title": "...", "subtitle": "...", "emailLabel": "..." },
    "register": { "title": "...", "subtitle": "...", ... },
    "errors": { "invalidCredentials": "...", "emailTaken": "..." }
  },
  "adCampaigns": {
    "backToPartnerCabinet": "...",
    ...
  },
  "common": { "submit": "...", "loading": "...", "or": "..." }
}
```

### Server vs Client Components

- **Server Components** — по умолчанию. Для SSR страниц (публичные страницы авто, профили партнёров).
- **`'use client'`** — только если нужны: хуки состояния, события браузера, useTranslations (если нет server-side версии).
- **Server Actions** (`'use server'`) — в `app/actions/` или `actions.ts` рядом с фичей.

### AbortController в async useEffect — ОБЯЗАТЕЛЬНО

Любой `useEffect` с асинхронным запросом должен:
1. Отменять предыдущий запрос при повторном вызове (дебаунс, пагинация).
2. Иметь cleanup-функцию чтобы избежать setState на размонтированном компоненте.

```typescript
// Для списков с дебаунсом/фильтрами — AbortController:
const abortRef = useRef<AbortController | null>(null);

const load = useCallback(async () => {
  abortRef.current?.abort();
  const controller = new AbortController();
  abortRef.current = controller;
  try {
    const result = await service.getItems(page, search, controller.signal);
    setData(result);
  } catch (err) {
    if (axios.isCancel(err)) return; // игнорировать отменённые запросы
    setError(...);
  }
}, [page, search]);

useEffect(() => {
  load();
  return () => abortRef.current?.abort();
}, [load]);

// Для одиночных загрузок (без повторов) — флаг cancelled:
useEffect(() => {
  let cancelled = false;
  service.getItem().then(data => {
    if (!cancelled) setData(data);
  });
  return () => { cancelled = true; };
}, []);
```

Методы в `adminService.ts` принимают опциональный `signal?: AbortSignal` и передают его в axios config.

### Middleware (auth guard)

`frontend/middleware.ts` — проверяет наличие `refreshToken` cookie. Публичные маршруты добавляются в массив `PUBLIC_ROUTES`.

```typescript
const PUBLIC_ROUTES = ["/auth/login", "/auth/register"];
// Добавлять сюда новые публичные маршруты: /vehicles/[vin], /partners/[id], etc.
```

### Именование файлов

| Тип | Конвенция | Пример |
|-----|-----------|--------|
| Page | `page.tsx` | `app/dashboard/page.tsx` |
| Layout | `layout.tsx` | `app/(dashboard)/layout.tsx` |
| Server Action | `actions.ts` | `app/vehicles/actions.ts` |
| Component | `PascalCase.tsx` | `VehicleCard.tsx` |
| Service | `camelCase.ts` | `vehicleService.ts` |
| Types | `camelCase.ts` | `vehicle.ts` в `/types/` |

### HTTP-клиент

Используется `axios`. Каждый модуль имеет свой сервис в `services/`:

```typescript
// services/vehicleService.ts
export const vehicleService = {
  async getByVin(vin: string): Promise<Vehicle> { ... },
  async create(data: CreateVehicleRequest): Promise<Vehicle> { ... },
};
```

---

## Docker Compose

```bash
# Запустить всё
docker compose up -d

# Только инфраструктура (postgres + minio)
docker compose up -d postgres minio

# Перебилдить конкретный сервис
docker compose up -d --build backend

# Логи
docker compose logs -f backend
docker compose logs -f frontend
```

**Env-переменные:** `docker-compose.yml` в корне монорепы читает `.env` файл. Пример: `.env.example` (нужно создать).

---

## Storage: MinIO vs Cloudflare R2

Провайдер хранилища выбирается через `appsettings.json` (или env-переменную):

```json
"Storage": {
  "Provider": "MinIO",         // "MinIO" (по умолчанию) или "R2"
  "ServiceUrl": "http://minio:9000",
  "AccessKey": "...",
  "SecretKey": "...",
  "BucketName": "autohelper",
  "CloudflareAccountId": "",   // только для R2
  "PublicBaseUrl": ""          // только для R2: https://pub-xxx.r2.dev
}
```

- `"MinIO"` → использует `S3StorageService` (path-style URL)
- `"R2"` → использует `R2StorageService` (использует `PublicBaseUrl` если задан, иначе path-style)
- Оба реализуют `IStorageService` из Application-слоя
- Файлы загружаемые через `/api/service-records/document` имеют UUID-ключи: `service-records/documents/{uuid}.pdf`
