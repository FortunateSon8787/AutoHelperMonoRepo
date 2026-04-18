# AutoHelper — Frontend Guide

## Стек

- **Next.js 15** (App Router, TypeScript)
- **Tailwind CSS 3** — стилизация (дизайн-система на CSS-переменных)
- **next-intl** — мультиязычность (ru/en)
- **Axios** — HTTP-клиент (auth, profile)
- **react-hook-form + zod** — формы и валидация
- **Docker** — standalone output (`output: "standalone"` в `next.config.ts`)

---

## Дизайн-система

Визуальный стиль — Modern SaaS Light (основан на Figma Make reference в `figma-make-reference/`).

### Цветовые токены (globals.css)

| Токен | Значение | Использование |
|-------|----------|---------------|
| `--primary` | navy `#0f1c3f` | Кнопки default, логотип, заголовки |
| `--accent` | cyan `#06b6d4` | Highlight, иконки, focus ring, loading |
| `--success` | green `#10b981` | Успех, чекмарки |
| `--destructive` | red `#ef4444` | Ошибки, удаление |
| `--background` | `#f8f9fb` | Фон страниц |
| `--card` | `#ffffff` | Фон карточек |
| `--border` | `rgba(15,28,63,0.1)` | Границы элементов |
| `--muted-foreground` | `#6b7280` | Подсказки, метаданные |
| `--radius` | `0.75rem` | Базовый border-radius |

### Border-radius

| Класс | Размер | Применение |
|-------|--------|------------|
| `rounded-lg` | 0.75rem | Теги, маленькие элементы |
| `rounded-xl` | 1rem | Кнопки, инпуты, алерты |
| `rounded-2xl` | 1.25rem | Карточки, формы |
| `rounded-3xl` | 1.75rem | Крупные блоки |

### Тени (tailwind.config.ts)

- `shadow-card` — стандартная тень карточки
- `shadow-card-hover` — тень при hover

### Паттерны компонентов

**Карточка:**
```tsx
<div className="bg-card border border-border rounded-2xl p-6 shadow-card hover:shadow-card-hover transition-shadow">
```

**Алерт ошибки:**
```tsx
<div className="bg-destructive/5 border border-destructive/20 text-destructive rounded-xl px-4 py-3 text-sm">
```

**Алерт успеха:**
```tsx
<div className="bg-success/5 border border-success/20 text-success rounded-xl px-4 py-3 text-sm">
```

**Иконка-бокс:**
```tsx
<div className="w-11 h-11 rounded-xl bg-primary/10 flex items-center justify-center">
  <Icon className="h-5 w-5 text-primary" />
</div>
```

---

## Структура `frontend/`

```
frontend/
├── app/                      # App Router
│   ├── layout.tsx            # Root layout (NextIntlClientProvider, шрифты Geist)
│   ├── page.tsx              # / → redirect на /auth/login
│   ├── globals.css           # Дизайн-токены (CSS-переменные) + base Tailwind
│   ├── actions/
│   │   └── locale.ts         # Server Action для смены языка (cookie NEXT_LOCALE)
│   ├── auth/
│   │   ├── login/page.tsx    # Страница входа (Client Component)
│   │   └── register/page.tsx # Страница регистрации (Client Component)
│   ├── profile/
│   │   └── page.tsx          # Профиль клиента (Client Component, требует авторизации)
│   ├── vehicles/
│   │   ├── page.tsx          # Публичный поиск авто по VIN
│   │   └── [vin]/page.tsx    # Публичная карточка владельца авто по VIN (SSR)
│   ├── dashboard/
│   │   └── vehicles/
│   │       ├── page.tsx      # Список авто + форма добавления
│   │       └── [id]/
│   │           ├── page.tsx  # Редактирование авто + смена статуса
│   │           └── service-records/
│   │               ├── page.tsx            # Список + форма добавления записи ТО
│   │               └── [recordId]/page.tsx # Детали записи ТО (просмотр PDF)
│   └── partner/
│       ├── register/page.tsx         # Регистрация партнёра
│       └── cabinet/
│           ├── page.tsx              # Кабинет партнёра (просмотр + редактирование)
│           └── ad-campaigns/page.tsx # Управление рекламными кампаниями
│
├── components/
│   ├── AppLogo.tsx           # Логотип AutoHelper (navy квадрат + текст, с href)
│   ├── AppHeader.tsx         # Sticky header для auth/dashboard страниц
│   ├── LocaleSwitcher.tsx    # Переключатель RU/EN (pill-стиль, встроен в AppHeader)
│   ├── pdf-preview-modal.tsx # Модальный просмотр PDF
│   ├── ads/
│   │   ├── AdBanner.tsx      # Рекламный баннер
│   │   └── OffersBlock.tsx   # Блок рекламных предложений
│   ├── chat/
│   │   ├── ChatSidebar.tsx                 # Боковая панель: выбор авто, режима, история чатов, кнопка "Новый чат"
│   │   ├── ChatWindow.tsx                  # Окно чата (сообщения, инпут, статусы)
│   │   ├── DiagnosticResultCard.tsx        # Карточка диагноза FaultHelp (urgency, проблемы, риски, рекомендации, safe-to-drive)
│   │   ├── WorkClarificationResultCard.tsx # Карточка анализа работ (рыночные бенчмарки, оценка стоимости)
│   │   ├── PartnerAdviceResultCard.tsx     # Карточка результата поиска партнёров (список партнёров с бейджами priority/warning, адрес, телефон, сайт, рейтинг, open/closed)
│   │   ├── DiagnosticsForm.tsx             # Форма создания FaultHelp чата
│   │   ├── WorkClarificationForm.tsx       # Форма создания WorkClarification чата
│   │   └── PartnerAdviceForm.tsx           # Форма создания PartnerAdvice чата (request + geolocation + urgency dropdown)
│   ├── partners/
│   │   └── PartnersMap.tsx   # Карта партнёров (Leaflet/OSM); принимает i18n-метки yourLocationLabel/openLabel/closedLabel/kmLabel как props
│   └── ui/
│       ├── button.tsx        # Button (варианты: default, accent, outline, secondary, ghost, link)
│       ├── input.tsx         # Input (rounded-xl, ring=accent)
│       └── label.tsx         # Label
│
├── lib/
│   ├── utils.ts              # cn() — clsx + tailwind-merge
│   └── form-styles.ts        # nativeSelectCn, nativeTextareaCn — классы для <select> и <textarea>
│
├── services/
│   ├── authService.ts        # POST /api/auth/* (withCredentials, httpOnly cookies)
│   ├── adminAuthService.ts   # POST /api/admin/auth/* (withCredentials)
│   ├── adminService.ts       # GET/POST /api/admin/* (withCredentials, signal support)
│   ├── profileService.ts     # GET/PATCH /api/clients/me (Bearer token)
│   ├── vehicleService.ts     # /api/vehicles (auth CRUD + public SSR)
│   ├── serviceRecordService.ts # /api/service-records
│   ├── partnerService.ts     # /api/partners
│   └── adCampaignService.ts  # /api/ad-campaigns
│
├── types/
│   ├── auth.ts               # LoginRequest, RegisterRequest, TokenResponse, AuthUser
│   ├── client.ts             # ClientProfile, UpdateProfileRequest
│   ├── vehicle.ts            # Vehicle, VehicleStatus, VehicleOwner, Create/UpdateRequest
│   ├── serviceRecord.ts      # ServiceRecord, CreateServiceRecordRequest
│   ├── partner.ts            # PartnerProfile, PARTNER_TYPES, etc.
│   ├── adCampaign.ts         # AdCampaign, AD_TYPES, TARGET_CATEGORIES
│   └── chat.ts               # ChatMode, ChatMessage, DiagnosticsInput, WorkClarificationInput,
│                             # PartnerAdviceInput, PartnerAdviceUrgency, PARTNER_ADVICE_URGENCY_VALUES,
│                             # DiagnosticResult, DiagnosticProblem, PartnerAdviceResult, PartnerAdviceEntry,
│                             # WorkClarificationResult, SendMessageResponse, CreateChatResponse
│
├── i18n/
│   └── request.ts            # next-intl server конфиг (locale из cookie)
│
├── messages/
│   ├── ru.json               # Русские переводы
│   └── en.json               # Английские переводы
│
├── middleware.ts             # Auth guard (проверка refreshToken cookie)
├── next.config.ts            # Next.js конфиг + next-intl plugin
└── tailwind.config.ts        # Дизайн-токены: colors (card, accent, success), borderRadius, boxShadow
```

---

## Shared UI-компоненты

### AppLogo

Логотип — navy квадрат с буквой «A» + текст AutoHelper. Принимает `href` (по умолчанию `/`).

```tsx
import { AppLogo } from "@/components/AppLogo";
<AppLogo href="/" />
```

### AppHeader

Sticky header для внутренних страниц. Содержит AppLogo + LocaleSwitcher. Принимает `children` для дополнительных кнопок (например, Logout).

```tsx
import { AppHeader } from "@/components/AppHeader";

// Без дополнительных кнопок
<AppHeader />

// С кнопкой
<AppHeader>
  <Button variant="ghost" size="sm" onClick={handleLogout}>
    <LogOut className="h-4 w-4" />
    {t("logoutButton")}
  </Button>
</AppHeader>
```

### Button

Варианты: `default` (navy), `accent` (cyan), `outline`, `secondary`, `ghost`, `link`.
Размеры: `sm`, `default` (h-10), `lg` (h-12), `icon`.

```tsx
<Button variant="accent" size="lg">Основной CTA</Button>
<Button variant="outline" size="sm">Вторичное действие</Button>
```

### Input

`rounded-xl`, `bg-input` token, cyan focus ring. Для ошибки добавляй `className="border-destructive"`.

### form-styles.ts

Для нативных `<select>` и `<textarea>` используй готовые классы:

```tsx
import { nativeSelectCn, nativeTextareaCn } from "@/lib/form-styles";

<select className={nativeSelectCn} {...register("type")} />
<textarea className={nativeTextareaCn} rows={3} {...register("description")} />

// При ошибке:
<select className={errors.type ? `${nativeSelectCn} border-destructive` : nativeSelectCn} />
```

---

## Маршрутизация

### Публичные маршруты

```typescript
// middleware.ts
const PUBLIC_ROUTES = ["/auth/login", "/auth/register"];
// Также публичны: /vehicles/*, /partners/*
```

### Реализованные маршруты

| Путь | Тип | Авторизация | Описание |
|------|-----|-------------|----------|
| `/` | Server | — | Redirect → `/auth/login` |
| `/auth/login` | Client | Нет | Форма входа |
| `/auth/register` | Client | Нет | Форма регистрации |
| `/profile` | Client | Да | Профиль клиента |
| `/vehicles` | Server | Нет | Публичный поиск авто по VIN |
| `/vehicles/[vin]` | Server (SSR) | Нет | Публичная карточка владельца |
| `/dashboard/vehicles` | Client | Да | Список авто + добавление |
| `/dashboard/vehicles/[id]` | Client | Да | Редактирование авто, смена статуса |
| `/dashboard/vehicles/[id]/service-records` | Client | Да | Список и создание записей ТО |
| `/dashboard/vehicles/[id]/service-records/[recordId]` | Client | Да | Детали записи ТО |
| `/partners` | Client | Нет | Геолокационный поиск + карта Leaflet |
| `/partners/[id]` | Server (SSR) | Нет | Публичный профиль партнёра |
| `/partner/register` | Client | Да | Регистрация как партнёр |
| `/partner/cabinet` | Client | Да | Кабинет партнёра (профиль) |
| `/partner/cabinet/ad-campaigns` | Client | Да | Рекламные кампании партнёра |
| `/dashboard/chat` | Client | Да | AI-чат (FaultHelp, WorkClarification, PartnerAdvice) |

---

## Чат-компоненты (`components/chat/`)

### DiagnosticResultCard

Отображает структурированный результат диагностики FaultHelp (`responseStage = "diagnostic_result"`).

Рендерится вместо текстового сообщения ассистента когда `message.diagnosticResultJson != null`. Парсинг происходит в `ChatMessageBubble`:

```tsx
const diagnosticResult: DiagnosticResult | null = (() => {
  if (!message.diagnosticResultJson) return null;
  try { return JSON.parse(message.diagnosticResultJson) as DiagnosticResult; }
  catch { return null; }
})();
```

**Секции карточки:**
- Заголовок с бейджем (summary)
- Urgency (цветовой бейдж: low/medium/high/stop_driving)
- Potential problems — список с вероятностями (цветовое кодирование: green <0.4, yellow 0.4–0.7, red >0.7)
- Current risks (AlertCircle, destructive)
- Recommended actions (нумерованный список)
- Safe to drive (ShieldCheck/ShieldOff)
- Disclaimer

**i18n ключи:** `chat.diagnosticResult.*` в `messages/ru.json` и `messages/en.json`

### PartnerAdviceResultCard

Отображает структурированный результат поиска партнёров (`PartnerAdvice` режим). Рендерится вместо текстового сообщения ассистента когда `message.partnerAdviceResultJson != null`.

**Структура карточки:**
- Заголовок-бейдж с количеством найденных партнёров
- Опциональный summary (совет/рекомендация от LLM)
- Список карточек партнёров (`PartnerCard`):
  - Индекс + имя + бейдж `★ Verified partner` (is_priority) + бейдж `⚠ Warning` (has_warning)
  - Meta-строка: расстояние, open/closed, рейтинг + кол-во отзывов
  - Детали: адрес, услуги, телефон (href=tel:), веб-сайт (href с валидацией URL)
- Empty state при `partners.length === 0`

**i18n ключи:** `chat.partnerAdviceResult.*`

### ChatSidebar

Боковая панель чат-страницы. Содержит:
- Список автомобилей пользователя (выбор авто для сессии)
- Переключатель режима чата (FaultHelp / WorkClarification / PartnerAdvice)
- Кнопку "Новый чат"
- Историю чатов с пагинацией (Load more) и возможностью удаления
- Закрытие через кнопку `X` (мобильный вид)

Props: `vehicles`, `chats`, `hasNextPage`, `isLoadingMore`, `subscription`, `selectedVehicleId`, `selectedMode`, `activeChatId`, `isOpen`, `onClose`, `onVehicleSelect`, `onModeChange`, `onChatSelect`, `onNewChat`, `onLoadMore`, `onDeleteChat`.

---

## i18n — next-intl

### Использование

**Client Component:**
```typescript
'use client';
import { useTranslations } from 'next-intl';

const t = useTranslations('auth.login');
return <h1>{t('title')}</h1>;
```

**Server Component:**
```typescript
import { getTranslations } from 'next-intl/server';

const t = await getTranslations('auth.login');
return <h1>{t('title')}</h1>;
```

### Язык

- Язык определяется из cookie `NEXT_LOCALE` → `i18n/request.ts`
- `LocaleSwitcher` меняет язык через Server Action (`app/actions/locale.ts`)
- Переводы в `messages/ru.json` и `messages/en.json`

### Ключи верхнего уровня

```
auth.login, auth.register, auth.errors
profile, profile.validation, profile.errors
vehicles.list, vehicles.form, vehicles.status
serviceRecords.list, serviceRecords.form
partner.register, partner.register.validation, partner.register.errors
partner.cabinet, partner.cabinet.errors
adCampaigns, adCampaigns.errors, adCampaigns.validation
chat.window, chat.modes, chat.modeSubtitles
chat.diagnosticResult           ← карточка диагноза FaultHelp
chat.workClarificationResult    ← карточка анализа работ WorkClarification
chat.partnerAdviceResult        ← карточка результата поиска партнёров PartnerAdvice (title, priorityBadge, warningBadge, km, openNow, closed, reviews, noPartners)
chat.diagnosticsForm, chat.workClarificationForm, chat.partnerAdviceForm  ← формы режимов
partners.search                 ← поиск партнёров (incl. yourLocationLabel, openLabel, closedLabel, kmLabel)
common
```

---

## HTTP-клиент (Axios)

Три паттерна:

**1. Auth (withCredentials):**
```typescript
// services/authService.ts — httpOnly cookie (refresh token)
const api = axios.create({ baseURL: ..., withCredentials: true });
```

**2. Authenticated (Bearer token):**
```typescript
// services/profileService.ts, vehicleService.ts — interceptor добавляет Authorization
```

**3. Public SSR (fetch):**
```typescript
// services/vehicleService.ts — fetch + revalidate: 60
```

**Переменная окружения:** `NEXT_PUBLIC_API_URL` (по умолчанию `http://localhost:8080`)

---

## Auth Flow

```
1. Пользователь → защищённый маршрут
2. middleware.ts проверяет наличие cookie 'refreshToken' (presence check, не валидация)
3. Если нет → redirect /auth/login
4. Login → authService.login() → POST /api/auth/login
5. Бэкенд устанавливает accessToken + refreshToken как httpOnly, Secure, SameSite=Strict cookies
6. При 401 → authService.refreshToken() → POST /api/auth/refresh
7. Logout → authService.logout() → POST /api/auth/logout → clear cookies
```

**Безопасность cookies:**
- `HttpOnly=true` — JS не может читать токены (защита от XSS)
- `Secure=true` — только HTTPS
- `SameSite=Strict` — CSRF-защита (cross-site запросы не отправляют куки)

**Admin Auth Flow:** аналогичен, но использует `/api/admin/auth/*` и куки `adminAccessToken` / `adminRefreshToken`.

---

## Security Headers (next.config.ts)

В продакшене добавляются следующие HTTP-заголовки для всех страниц:

| Заголовок | Значение | Цель |
|-----------|----------|------|
| `X-Frame-Options` | `SAMEORIGIN` | Защита от clickjacking |
| `X-Content-Type-Options` | `nosniff` | Отключение MIME-sniffing |
| `Referrer-Policy` | `strict-origin-when-cross-origin` | Контроль реферера |
| `Permissions-Policy` | `geolocation=(self)` | Геолокация только self |
| `Content-Security-Policy` | `default-src 'self'` + разрешения | Базовый CSP |

CSP разрешает `*.tile.openstreetmap.org` для Leaflet-карты на странице `/partners`.

---

## Docker (Frontend)

`frontend/Dockerfile` — multi-stage:
1. `deps` → `npm ci`
2. `builder` → `npm run build` (bakes `NEXT_PUBLIC_*`)
3. `runner` → `.next/standalone`, `node server.js`

`NEXT_PUBLIC_API_URL` задаётся через `build args` в `docker-compose.yml`.
