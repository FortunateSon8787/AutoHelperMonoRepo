import { Mail } from 'lucide-react';

interface FooterProps {
  language: 'en' | 'ru';
}

const content = {
  en: {
    product: "Product",
    features: "Features",
    pricing: "Pricing",
    howItWorks: "How It Works",
    useCases: "Use Cases",
    company: "Company",
    about: "About Us",
    blog: "Blog",
    careers: "Careers",
    support: "Support",
    helpCenter: "Help Center",
    contact: "Contact",
    faq: "FAQ",
    legal: "Legal",
    privacy: "Privacy Policy",
    terms: "Terms of Service",
    cookies: "Cookie Policy",
    supportEmail: "support@autohelper.com",
    copyright: "© 2026 AutoHelper. All rights reserved.",
    description: "Your trusted AI automotive assistant for confident vehicle decisions.",
  },
  ru: {
    product: "Продукт",
    features: "Возможности",
    pricing: "Тарифы",
    howItWorks: "Как это работает",
    useCases: "Сценарии",
    company: "Компания",
    about: "О нас",
    blog: "Блог",
    careers: "Карьера",
    support: "Поддержка",
    helpCenter: "Центр помощи",
    contact: "Контакты",
    faq: "FAQ",
    legal: "Правовая информация",
    privacy: "Политика конфиденциальности",
    terms: "Условия использования",
    cookies: "Политика cookies",
    supportEmail: "support@autohelper.com",
    copyright: "© 2026 AutoHelper. Все права защищены.",
    description: "Ваш надёжный AI-помощник для уверенных решений об автомобиле.",
  },
};

export function Footer({ language }: FooterProps) {
  const t = content[language];

  return (
    <footer className="bg-primary text-white/80">
      <div className="container mx-auto px-4 lg:px-8 py-12 lg:py-16">
        <div className="grid sm:grid-cols-2 lg:grid-cols-5 gap-8 lg:gap-12 mb-12">
          <div className="lg:col-span-2 space-y-4">
            <div className="text-2xl font-semibold text-white">
              AutoHelper
            </div>
            <p className="text-sm text-white/60 max-w-xs">
              {t.description}
            </p>
            <div className="flex items-center gap-2 text-sm">
              <Mail className="w-4 h-4" />
              <a href={`mailto:${t.supportEmail}`} className="hover:text-white transition-colors">
                {t.supportEmail}
              </a>
            </div>
          </div>

          <div>
            <div className="text-sm font-medium text-white mb-4">{t.product}</div>
            <ul className="space-y-2">
              <li>
                <a href="#features" className="text-sm hover:text-white transition-colors">
                  {t.features}
                </a>
              </li>
              <li>
                <a href="#pricing" className="text-sm hover:text-white transition-colors">
                  {t.pricing}
                </a>
              </li>
              <li>
                <a href="#" className="text-sm hover:text-white transition-colors">
                  {t.howItWorks}
                </a>
              </li>
              <li>
                <a href="#" className="text-sm hover:text-white transition-colors">
                  {t.useCases}
                </a>
              </li>
            </ul>
          </div>

          <div>
            <div className="text-sm font-medium text-white mb-4">{t.company}</div>
            <ul className="space-y-2">
              <li>
                <a href="#" className="text-sm hover:text-white transition-colors">
                  {t.about}
                </a>
              </li>
              <li>
                <a href="#" className="text-sm hover:text-white transition-colors">
                  {t.blog}
                </a>
              </li>
              <li>
                <a href="#" className="text-sm hover:text-white transition-colors">
                  {t.careers}
                </a>
              </li>
            </ul>
          </div>

          <div>
            <div className="text-sm font-medium text-white mb-4">{t.support}</div>
            <ul className="space-y-2">
              <li>
                <a href="#" className="text-sm hover:text-white transition-colors">
                  {t.helpCenter}
                </a>
              </li>
              <li>
                <a href="#" className="text-sm hover:text-white transition-colors">
                  {t.contact}
                </a>
              </li>
              <li>
                <a href="#faq" className="text-sm hover:text-white transition-colors">
                  {t.faq}
                </a>
              </li>
            </ul>
          </div>
        </div>

        <div className="pt-8 border-t border-white/10">
          <div className="flex flex-col md:flex-row justify-between items-center gap-4">
            <div className="text-sm text-white/50">
              {t.copyright}
            </div>
            <div className="flex flex-wrap justify-center gap-6">
              <a href="#" className="text-sm hover:text-white transition-colors">
                {t.privacy}
              </a>
              <a href="#" className="text-sm hover:text-white transition-colors">
                {t.terms}
              </a>
              <a href="#" className="text-sm hover:text-white transition-colors">
                {t.cookies}
              </a>
            </div>
          </div>
        </div>
      </div>
    </footer>
  );
}
