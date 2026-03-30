import { Globe } from 'lucide-react';

interface HeaderProps {
  language: 'en' | 'ru';
  onLanguageChange: (lang: 'en' | 'ru') => void;
}

const content = {
  en: {
    features: 'Features',
    pricing: 'Pricing',
    faq: 'FAQ',
    login: 'Login',
    getStarted: 'Get Started',
  },
  ru: {
    features: 'Возможности',
    pricing: 'Тарифы',
    faq: 'FAQ',
    login: 'Войти',
    getStarted: 'Начать',
  },
};

export function Header({ language, onLanguageChange }: HeaderProps) {
  const t = content[language];

  return (
    <header className="fixed top-0 left-0 right-0 z-50 bg-white/80 backdrop-blur-lg border-b border-border">
      <div className="container mx-auto px-4 lg:px-8">
        <div className="flex items-center justify-between h-16 lg:h-20">
          <div className="flex items-center gap-12">
            <div className="text-xl lg:text-2xl font-semibold text-primary">
              AutoHelper
            </div>
            <nav className="hidden md:flex items-center gap-8">
              <a href="#features" className="text-sm text-foreground/70 hover:text-foreground transition-colors">
                {t.features}
              </a>
              <a href="#pricing" className="text-sm text-foreground/70 hover:text-foreground transition-colors">
                {t.pricing}
              </a>
              <a href="#faq" className="text-sm text-foreground/70 hover:text-foreground transition-colors">
                {t.faq}
              </a>
            </nav>
          </div>

          <div className="flex items-center gap-3 lg:gap-4">
            <div className="flex items-center gap-2 bg-secondary rounded-lg p-1">
              <button
                onClick={() => onLanguageChange('en')}
                className={`px-3 py-1.5 text-xs lg:text-sm rounded-md transition-all ${
                  language === 'en'
                    ? 'bg-white text-primary shadow-sm'
                    : 'text-foreground/60 hover:text-foreground'
                }`}
              >
                EN
              </button>
              <button
                onClick={() => onLanguageChange('ru')}
                className={`px-3 py-1.5 text-xs lg:text-sm rounded-md transition-all ${
                  language === 'ru'
                    ? 'bg-white text-primary shadow-sm'
                    : 'text-foreground/60 hover:text-foreground'
                }`}
              >
                RU
              </button>
            </div>

            <button className="hidden md:block px-4 py-2 text-sm text-foreground/70 hover:text-foreground transition-colors">
              {t.login}
            </button>

            <button className="px-4 lg:px-6 py-2 lg:py-2.5 text-sm bg-primary text-primary-foreground rounded-lg hover:bg-primary/90 transition-all shadow-sm hover:shadow-md">
              {t.getStarted}
            </button>
          </div>
        </div>
      </div>
    </header>
  );
}
