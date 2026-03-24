# AutoHelper — Agent Navigation Guide

> Этот файл — точка входа для AI-агента. Читай его первым при работе с монорепозиторием.

## Что такое AutoHelper

SaaS-платформа для учёта автомобилей, истории их обслуживания, AI-диагностики и подбора партнёров (автосервисы, эвакуаторы, автомойки и др.). Подробные бизнес-требования: `.claude/AutoHelper_Requirements_v1.3.md`.

---

## Структура монорепозитория

```
AutoHelperMonoRepo/
├── backend/                  # ASP.NET Core 10 Web API (Clean Architecture)
│   ├── src/
│   │   ├── AutoHelper.Api            # Presentation: Minimal API endpoints, middleware
│   │   ├── AutoHelper.Application    # Use Cases: MediatR commands/queries, interfaces
│   │   ├── AutoHelper.Domain         # Domain: Entities, AggregateRoots, Value Objects, Events
│   │   └── AutoHelper.Infrastructure # Infrastructure: EF Core, JWT, S3/MinIO, external services
│   ├── tests/                        # Unit + integration tests
│   ├── AutoHelper.sln
│   ├── Directory.Build.props         # Shared MSBuild properties
│   ├── Directory.Packages.props      # Central package version management
│   └── docker-compose.yml            # Backend-only compose (dev)
│
├── frontend/                 # Next.js 15 + TypeScript (App Router)
│   ├── app/                          # App Router pages & layouts
│   │   ├── auth/                     # login, register pages
│   │   └── actions/                  # Server Actions
│   ├── components/                   # Shared UI components
│   ├── services/                     # HTTP-клиенты (axios)
│   ├── contexts/                     # React context providers
│   ├── types/                        # TypeScript типы/интерфейсы
│   ├── i18n/                         # next-intl config
│   ├── messages/                     # Переводы: ru.json, en.json
│   ├── lib/                          # Утилиты
│   └── middleware.ts                 # Auth middleware (JWT cookie check)
│
├── docker-compose.yml        # Full-stack compose (postgres + minio + backend + frontend)
└── .claude/                  # Документация для AI-агента (не влияет на сборку)
    ├── AGENT.md              ← ты здесь
    ├── ARCHITECTURE.md
    ├── CONVENTIONS.md
    ├── API.md
    ├── FRONTEND.md
    ├── DOMAIN.md
    ├── AutoHelper_Requirements_v1.3.md
    └── JIRA_DECOMPOSITION.md
```

---

## Ключевые файлы-ориентиры

| Что нужно понять | Файл |
|------------------|------|
| Бизнес-требования | `.claude/AutoHelper_Requirements_v1.3.md` |
| Декомпозиция задач (Jira) | `.claude/JIRA_DECOMPOSITION.md` |
| Архитектура бэкенда | `.claude/ARCHITECTURE.md` |
| Конвенции кода, git, миграции | `.claude/CONVENTIONS.md` |
| Доменная модель | `.claude/DOMAIN.md` |
| API-эндпоинты | `.claude/API.md` |
| Фронтенд-структура | `.claude/FRONTEND.md` |
| Точка входа бэкенда | `backend/src/AutoHelper.Api/Program.cs` |
| DI-регистрация | `backend/src/AutoHelper.Api/Extensions/` |
| DbContext | `backend/src/AutoHelper.Infrastructure/Persistence/AppDbContext.cs` |
| Routing фронтенда | `frontend/middleware.ts` |
| i18n конфиг | `frontend/i18n/request.ts` |

---

## Управление требованиями (ОБЯЗАТЕЛЬНО читать)

Если в процессе работы пользователь пишет что-то похожее на **изменение требований или функционала** AutoHelper — нужно:

1. Заметить это и явно сообщить пользователю: _«Это похоже на изменение требований»_
2. Предложить внести изменения в файл `.claude/AutoHelper_Requirements_v1.3.md`
3. Перезаписать файл с обновлёнными требованиями
4. Увеличить номер версии в имени файла (например, `v1.3` → `v1.4`) и переименовать

---

## Текущий статус разработки

**Реализовано (Epic AUT-1 — Управление клиентами ✅ Готово):**
- ✅ Clean Architecture скелет (Domain / Application / Infrastructure / Api)
- ✅ Customer aggregate (email+password + Google OAuth, AvatarUrl)
- ✅ Vehicle aggregate (VIN уникальность, статусы, OwnerId)
- ✅ JWT auth: Register / Login / RefreshToken / Logout (MediatR CQRS)
- ✅ Клиентский профиль: GET/PUT /api/clients/me, смена пароля, загрузка аватара
- ✅ Публичный поиск владельца по VIN: GET /api/vehicles/{vin}/owner
- ✅ EF Core + PostgreSQL (3 миграции в `Infrastructure/Persistence/Migrations/`)
- ✅ MinIO/S3 инфраструктура (аватары)
- ✅ Serilog структурированное логирование + CorrelationId middleware
- ✅ Next.js App Router + next-intl (ru/en)
- ✅ Auth UI (login/register страницы)
- ✅ Profile UI страница (`/profile`)
- ✅ Публичная SSR-страница владельца авто (`/vehicles/[vin]`)
- ✅ Docker Compose (postgres + minio + backend + frontend)
- ✅ Unit-тесты: Domain (Customer, Vehicle), Application (JwtTokenService, GetVehicleOwner handler)

**В работе / Планируется (см. JIRA_DECOMPOSITION.md):**
- 🔲 CRUD автомобилей (Epic AUT-2: AUT-13..15)
- 🔲 Service Records / история работ (Epic AUT-3)
- 🔲 AI АвтоПомощник — LLM-чат (Epic AUT-4)
- 🔲 Биллинг / Stripe (Epic AUT-5)
- 🔲 Партнёры (Epic AUT-6)
- 🔲 Админ-панель (Epic AUT-7)

---

## Стек технологий

| Слой | Технология |
|------|------------|
| Backend | .NET 10, ASP.NET Core Minimal API |
| ORM | EF Core 9, PostgreSQL 17 |
| CQRS/Mediator | MediatR |
| Валидация | FluentValidation |
| Аутентификация | JWT (access + refresh), Google OAuth 2.0 |
| File Storage | MinIO (S3-compatible) |
| AI | OpenAI GPT-4o |
| Платежи | Stripe |
| Логирование | Serilog |
| API Docs | OpenAPI + Scalar |
| Frontend | Next.js 15, TypeScript, Tailwind CSS |
| i18n | next-intl |
| HTTP-клиент (FE) | Axios |
| Контейнеризация | Docker, Docker Compose |
| CI/CD | GitHub Actions (планируется) |
| Мониторинг | Prometheus + Grafana (планируется) |

---

## Правила, которые НЕЛЬЗЯ нарушать

1. **VIN уникален** — unique constraint в БД, валидация в домене.
2. **Только одна запись авто** — создаётся один раз, владелец меняется.
3. **AI только по подписке Premium** — middleware проверяет `SubscriptionStatus`.
4. **Хардкод строк в компонентах запрещён** — только `useTranslations()` из next-intl.
5. **API-ключ OpenAI только на бэкенде** — никогда в браузер.
6. **Перед коммитом**: `dotnet build` → `dotnet test` → проверка миграций (см. CONVENTIONS.md).
7. **Миграции только с флагом `--output-dir Persistence/Migrations`** (см. CONVENTIONS.md).

---

## Рабочий процесс AI-агента

### Использование скиллов (ОБЯЗАТЕЛЬНО)

Перед написанием кода — выбрать и вызвать подходящий скилл, не писать код напрямую:

| Задача | Скиллы |
|--------|--------|
| Реализация .NET/C# кода | `dotnet-senior-dev` → затем `dotnet-code-reviewer` |
| Next.js страница (SSR, Server Component) | `nextjs-server-client-components`, `nextjs-app-router-fundamentals` |
| Server Actions, cookies, форmy | `nextjs-advanced-routing`, `nextjs-client-cookie-pattern` |
| useSearchParams, Suspense | `nextjs-use-search-params-suspense` |

### Git-коммиты

- **Не упоминать Claude** в сообщениях коммитов (ни `Co-Authored-By`, ни в теле).

### После завершения каждой Story

1. Обновить статус задачи в Jira → **Готово**
2. Добавить комментарий в Jira с описанием что сделано
3. Актуализировать связанные файлы в `.claude/`: `API.md`, `DOMAIN.md`, `CONVENTIONS.md`

---

## Быстрый старт (локально)

```bash
# Из корня монорепы
cp .env.example .env       # заполнить переменные
docker compose up -d       # поднять всё: postgres, minio, backend, frontend

# Backend отдельно (для разработки)
cd backend
dotnet run --project src/AutoHelper.Api

# Frontend отдельно
cd frontend
npm install
npm run dev
```

**Порты:**
- Frontend: http://localhost:3000
- Backend API: http://localhost:8080
- Scalar (API docs): http://localhost:8080/scalar/v1
- MinIO Console: http://localhost:9001
- PostgreSQL: localhost:5432
