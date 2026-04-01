import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";

// Маршруты доступные без авторизации обычным пользователям
const PUBLIC_ROUTES = ["/auth/login", "/auth/register", "/vehicles", "/partners"];

// Admin-маршруты (все /admin/* кроме /admin/login)
const ADMIN_LOGIN_ROUTE = "/admin/login";
const ADMIN_PREFIX = "/admin";

export function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;

  // ── Admin routes ──────────────────────────────────────────────────────────

  // Страница логина для администратора всегда публична
  if (pathname === ADMIN_LOGIN_ROUTE) {
    return NextResponse.next();
  }

  // Все остальные /admin/* маршруты требуют adminRefreshToken
  if (pathname.startsWith(ADMIN_PREFIX)) {
    const adminRefreshToken = request.cookies.get("adminRefreshToken");
    if (!adminRefreshToken) {
      const loginUrl = new URL(ADMIN_LOGIN_ROUTE, request.url);
      return NextResponse.redirect(loginUrl);
    }
    return NextResponse.next();
  }

  // ── Regular user routes ───────────────────────────────────────────────────

  // Пропускаем публичные маршруты
  if (PUBLIC_ROUTES.some((route) => pathname.startsWith(route))) {
    return NextResponse.next();
  }

  // Проверяем наличие refresh token в cookie
  const refreshToken = request.cookies.get("refreshToken");

  if (!refreshToken) {
    const loginUrl = new URL("/auth/login", request.url);
    return NextResponse.redirect(loginUrl);
  }

  return NextResponse.next();
}

export const config = {
  matcher: [
    // Применяем middleware ко всем маршрутам кроме статики и API
    "/((?!api|_next/static|_next/image|favicon.ico).*)",
  ],
};
