import { Cookie, X } from 'lucide-react';

interface CookieBannerProps {
  language: 'en' | 'ru';
  onAccept: () => void;
  onDecline: () => void;
}

const content = {
  en: {
    title: "Cookie Preferences",
    message: "We use cookies to enhance your experience, analyze site traffic, and personalize content. By clicking 'Accept', you consent to our use of cookies.",
    accept: "Accept All",
    decline: "Decline",
    learnMore: "Learn More",
  },
  ru: {
    title: "Настройки Cookie",
    message: "Мы используем cookies для улучшения вашего опыта, анализа трафика и персонализации контента. Нажимая 'Принять', вы соглашаетесь на использование cookies.",
    accept: "Принять все",
    decline: "Отклонить",
    learnMore: "Узнать больше",
  },
};

export function CookieBanner({ language, onAccept, onDecline }: CookieBannerProps) {
  const t = content[language];

  return (
    <div className="fixed bottom-0 left-0 right-0 z-50 p-4 lg:p-6">
      <div className="container mx-auto">
        <div className="bg-white border border-border rounded-2xl shadow-2xl max-w-4xl mx-auto">
          <div className="p-6 lg:p-8">
            <div className="flex items-start gap-4">
              <div className="w-10 h-10 bg-accent/10 rounded-xl flex items-center justify-center flex-shrink-0">
                <Cookie className="w-5 h-5 text-accent" />
              </div>
              <div className="flex-1 space-y-3">
                <div className="flex items-start justify-between gap-4">
                  <h3 className="font-medium text-foreground">{t.title}</h3>
                  <button
                    onClick={onDecline}
                    className="text-foreground/40 hover:text-foreground transition-colors"
                  >
                    <X className="w-5 h-5" />
                  </button>
                </div>
                <p className="text-sm text-foreground/70 leading-relaxed">
                  {t.message}
                </p>
                <div className="flex flex-col sm:flex-row gap-3 pt-2">
                  <button
                    onClick={onAccept}
                    className="px-6 py-2.5 bg-primary text-primary-foreground rounded-lg hover:bg-primary/90 transition-all"
                  >
                    {t.accept}
                  </button>
                  <button
                    onClick={onDecline}
                    className="px-6 py-2.5 bg-secondary text-foreground rounded-lg hover:bg-secondary/80 transition-all"
                  >
                    {t.decline}
                  </button>
                  <a
                    href="#"
                    className="px-6 py-2.5 text-foreground/70 hover:text-foreground transition-colors flex items-center justify-center"
                  >
                    {t.learnMore}
                  </a>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
