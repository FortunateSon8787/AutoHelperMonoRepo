# AutoHelper — Frontend Guide

## Стек

- **Next.js 15** (App Router, TypeScript)
- **Tailwind CSS** — стилизация
- **next-intl** — мультиязычность (ru/en)
- **Axios** — HTTP-клиент (auth, profile)
- **react-hook-form + zod** — формы и валидация
- **Docker** — standalone output (`output: "standalone"` в `next.config.ts`)

---

## Структура `frontend/`

```
frontend/
├── app/                      # App Router
│   ├── layout.tsx            # Root layout (NextIntlClientProvider, шрифты)
│   ├── page.tsx              # Главная страница (/) → redirect на /auth/login
│   ├── globals.css           # Глобальные стили
│   ├── auth/
│   │   ├── login/page.tsx    # Страница входа (Client Component)
│   │   └── register/page.tsx # Страница регистрации (Client Component)
│   ├── profile/
│   │   └── page.tsx          # Профиль клиента (Client Component, требует авторизации)
│   ├── vehicles/
│   │   └── [vin]/page.tsx    # Публичная карточка владельца авто по VIN (Server Component, SSR)
│   └── actions/              # Server Actions
│
├── components/
│   ├── LocaleSwitcher.tsx    # Переключатель языка (ru/en)
│   └── ui/                   # Reusable UI компоненты
│       ├── button.tsx
│       ├── input.tsx
│       └── label.tsx
│
├── services/
│   ├── authService.ts        # HTTP-функции для auth API (axios, withCredentials)
│   ├── profileService.ts     # HTTP-функции для /api/clients/me (axios + Bearer token)
│   └── vehicleService.ts     # HTTP-функции для /api/vehicles (fetch, SSR-кэш 60s)
│
├── contexts/
│   └── AuthContext.tsx       # React Context: user, accessToken, isAuthenticated, login/logout
│
├── types/
│   ├── auth.ts               # LoginRequest, RegisterRequest, TokenResponse, AuthUser, AuthContextValue
│   ├── client.ts             # ClientProfile, UpdateProfileRequest
│   └── vehicle.ts            # VehicleOwner
│
├── i18n/
│   └── request.ts            # next-intl server-side конфиг (locale из cookie)
│
├── messages/
│   ├── ru.json               # Русские переводы
│   └── en.json               # Английские переводы
│
├── lib/                      # Утилиты
├── middleware.ts             # Auth guard (проверка refreshToken cookie)
├── next.config.ts            # Next.js конфиг + next-intl plugin
└── tailwind.config.ts
```

---

## Маршрутизация (App Router)

### Публичные маршруты (без авторизации)

```typescript
// middleware.ts
const PUBLIC_ROUTES = ["/auth/login", "/auth/register"];
// Также публичны по паттерну: /vehicles/* (проверяется отдельно)
```

### Реализованные маршруты

| Путь | Тип компонента | Авторизация | Описание |
|------|----------------|-------------|----------|
| `/` | Server | — | Redirect на `/auth/login` |
| `/auth/login` | Client | Нет | Форма входа |
| `/auth/register` | Client | Нет | Форма регистрации |
| `/profile` | Client | Да | Профиль клиента (имя, контакты, аватар, смена пароля) |
| `/vehicles/[vin]` | Server (SSR) | Нет | Публичная карточка владельца авто по VIN |

### Роутинг по ролям (планируется)

| Раздел | Путь | Роль |
|--------|------|------|
| Дашборд клиента | `/dashboard` | Customer |
| AI-чат | `/dashboard/chat` | Customer (Premium) |
| Мои авто | `/dashboard/vehicles` | Customer |
| Подписка | `/dashboard/subscription` | Customer |
| Кабинет партнёра | `/partner` | Partner |
| Админ-панель | `/admin` | Admin / Superadmin |

---

## i18n — next-intl

### Как использовать в компонентах

**Client Component:**
```typescript
'use client';
import { useTranslations } from 'next-intl';

export function LoginForm() {
  const t = useTranslations('auth.login');
  return <h1>{t('title')}</h1>;
}
```

**Server Component:**
```typescript
import { getTranslations } from 'next-intl/server';

export default async function LoginPage() {
  const t = await getTranslations('auth.login');
  return <h1>{t('title')}</h1>;
}
```

### Структура файлов переводов

```json
// messages/ru.json
{
  "auth": {
    "login": { "title": "Вход", ... },
    "register": { "title": "Регистрация", ... },
    "errors": { "invalidCredentials": "...", "emailTaken": "...", ... }
  },
  "profile": {
    "title": "Профиль",
    "name": "Имя",
    "contacts": "Контакты",
    ...
  },
  "vehicles": {
    "owner": "Владелец",
    ...
  },
  "common": {
    "submit": "Отправить",
    "loading": "Загрузка...",
    "or": "или"
  }
}
```

### Язык и LocaleSwitcher

- Язык определяется из cookie → `i18n/request.ts`
- `LocaleSwitcher` компонент — реализован в `components/LocaleSwitcher.tsx`
- Меняет язык через Server Action (cookie `NEXT_LOCALE`)

---

## HTTP-клиент (Axios / fetch)

Все API-вызовы через сервисы в `services/`. Три паттерна:

**1. Auth (withCredentials):**
```typescript
// services/authService.ts
const api = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL,
  headers: { 'Content-Type': 'application/json' },
  withCredentials: true,  // для httpOnly cookie (refresh token)
});
```

**2. Authenticated (Bearer token):**
```typescript
// services/profileService.ts — interceptor добавляет Authorization: Bearer <token>
```

**3. Public SSR (fetch с кэшем):**
```typescript
// services/vehicleService.ts — fetch + revalidate: 60
```

**Переменные окружения:**
- `NEXT_PUBLIC_API_URL` — URL бэкенда (по умолчанию `http://localhost:8080`)

### Паттерн нового сервиса (authenticated)

```typescript
// services/vehicleService.ts (будущий расширенный вариант)
export const vehicleService = {
  async getMyVehicles(): Promise<Vehicle[]> {
    const response = await api.get<Vehicle[]>('/api/vehicles');
    return response.data;
  },

  async create(data: CreateVehicleRequest): Promise<Vehicle> {
    const response = await api.post<Vehicle>('/api/vehicles', data);
    return response.data;
  },
};
```

---

## Auth Flow (текущая реализация)

```
1. Пользователь заходит на защищённый маршрут
2. middleware.ts проверяет cookie 'refreshToken'
3. Если нет → redirect на /auth/login
4. Login form → authService.login() → POST /api/auth/login
5. Сохранить accessToken (memory/context) и refreshToken (cookie)
6. При 401 → authService.refreshToken() → POST /api/auth/refresh
7. Logout → authService.logout() → POST /api/auth/logout → clear cookies
```

Стратегия хранения токенов:
- `refreshToken` → httpOnly cookie
- `accessToken` → in-memory (React state в AuthContext)

---

## SEO-страницы (Server Components + SSR)

Публичные страницы, требующие SSR:

| Страница | Путь | Данные |
|----------|------|--------|
| Карточка владельца авто | `/vehicles/[vin]` | SSR, данные по VIN |
| Профиль партнёра | `/partners/[id]` | SSR (планируется) |
| Каталог продаж | `/sale` | SSG/ISR (планируется) |

**Паттерн:**
```typescript
// app/vehicles/[vin]/page.tsx — Server Component
export async function generateMetadata({ params }: { params: { vin: string } }) {
  const owner = await vehicleService.getOwnerByVin(params.vin);
  return { title: `Владелец ${params.vin} — AutoHelper` };
}

export default async function VehiclePage({ params }: { params: { vin: string } }) {
  const owner = await vehicleService.getOwnerByVin(params.vin);
  return <OwnerCard owner={owner} />;
}
```

---

## Docker (Frontend)

`frontend/Dockerfile` — multi-stage build:
1. `deps` — `npm ci`
2. `builder` — `npm run build` (bakes `NEXT_PUBLIC_*` vars)
3. `runner` — копирует `.next/standalone`, запускает `node server.js`

**NEXT_PUBLIC_API_URL** задаётся через `build args` в docker-compose.yml.
