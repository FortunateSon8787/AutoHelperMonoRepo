import { redirect } from "next/navigation";

// Главная страница — редирект на логин
export default function Home() {
  redirect("/auth/login");
}
