# AutoHelper — Agent Navigation Guide

> Этот файл — точка входа для AI-агента. Читай его первым при работе с монорепозиторием.

## Что такое AutoHelper

SaaS-платформа для учёта автомобилей, истории их обслуживания, AI-диагностики и подбора партнёров (автосервисы, эвакуаторы, автомойки и др.). Подробные бизнес-требования: `.claude/AutoHelper_Requirements_v1.4.md`.

---

## Структура монорепозитория

```
AutoHelperMonoRepo/
├── backend/          # ASP.NET Core 10 Web API (Clean Architecture) — подробнее: .claude/ARCHITECTURE.md
├── frontend/         # Next.js 15 + TypeScript (App Router) — подробнее: .claude/FRONTEND.md
├── docker-compose.yml
└── .claude/          # Документация для AI-агента
```

---

## Ключевые файлы-ориентиры

| Что нужно понять | Файл |
|------------------|------|
| Бизнес-требования | `.claude/AutoHelper_Requirements_v1.6.md` |
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
2. Предложить внести изменения в файл `.claude/AutoHelper_Requirements_v1.6.md`
3. Перезаписать файл с обновлёнными требованиями
4. Увеличить номер версии в имени файла (например, `v1.4` → `v1.5`) и переименовать

---

## Текущий статус разработки

✅ Реализованы Epics: AUT-1 (Клиенты), AUT-2 (Автомобили), AUT-3 (Service Records)
✅ Частично реализован Epic AUT-6 (Партнёры): AUT-24 (регистрация/профиль/верификация), AUT-25 (отзывы), AUT-26 (рекламные кампании), AUT-27 (геолокационный поиск + карта Leaflet)
✅ Частично реализован Epic AUT-4 (AI-чат): AUT-17 (чат-инфраструктура), AUT-19 (Режим 1: FaultHelp — многошаговая диагностика), AUT-20 (Режим 2: WorkClarification — одношаговый анализ работ + рыночные бенчмарки)
🔲 В планах: AUT-5 (Биллинг), AUT-7 (Админ), AUT-8 (Фронтенд), AUT-9 (DevOps), AUT-150 (Soft-delete + Аудит)

Подробная декомпозиция: `.claude/JIRA_DECOMPOSITION.md`

---

## Правила, которые НЕЛЬЗЯ нарушать

1. **VIN уникален** — unique constraint в БД, валидация в домене.
2. **Только одна запись авто** — создаётся один раз, владелец меняется.
3. **Все режимы AI только по подписке** — middleware проверяет подписку. Режимы 1 и 2 уменьшают счётчик запросов. Режим 3 подписку требует, но счётчик **не уменьшает**.
4. **Хардкод строк в компонентах запрещён** — только `useTranslations()` из next-intl.
5. **API-ключ LLM-провайдера только на бэкенде** — никогда в браузер.
6. **Перед коммитом**: `dotnet build` → `dotnet test` → проверка миграций (см. CONVENTIONS.md).
7. **Миграции только с флагом `--output-dir Persistence/Migrations`** (см. CONVENTIONS.md).
8. **Все удаления — soft-delete** (IsDeleted = true), физическое удаление из БД запрещено.
9. **Аудит-лог** — все CRUD-операции над Customer, Vehicle, ServiceRecord фиксируются в AuditLogs.
10. **Инфраструктурные абстракции** — Storage, LLM, Billing реализуются через интерфейсы Application-слоя.

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

### Работа с Jira-тасками (ОБЯЗАТЕЛЬНО перед реализацией)

При получении задания вида «выполни AUT-XX»:

1. **Прочитать родительскую таску** через `getJiraIssue` — описание, acceptance criteria.
2. **Проверить наличие субтасок** — если есть, прочитать каждую субтаску отдельно.
   - Описание субтаски может содержать требования, ограничения или нюансы, которых нет в родительской.
   - Реализация учитывает **все** описания субтасок, а не только их заголовки.
3. Только после полного изучения задачи — переходить к реализации.

### Git-коммиты

- **Не упоминать Claude** в сообщениях коммитов (ни `Co-Authored-By`, ни в теле).

### После завершения каждой Story  (ОБЯЗАТЕЛЬНО)

1. Смержить feature-ветку в `main`, запушить
2. Обновить статус задачи в Jira → **Готово**
3. Добавить комментарий в Jira: что сделано, на что обратить внимание, что изменилось в структуре/взаимодействии компонентов
4. Актуализировать связанные файлы в `.claude/`: `API.md`, `DOMAIN.md`, `CONVENTIONS.md`

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
