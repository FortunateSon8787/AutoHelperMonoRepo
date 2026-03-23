import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";

// Маршруты доступные без авторизации
const PUBLIC_ROUTES = ["/auth/login", "/auth/register"];

export function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;

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
