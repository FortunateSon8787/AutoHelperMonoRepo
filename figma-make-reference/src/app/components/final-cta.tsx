import { ArrowRight } from 'lucide-react';

interface FinalCTAProps {
  language: 'en' | 'ru';
}

const content = {
  en: {
    title: "Ready to Take Control of Your Car Issues?",
    subtitle: "Join thousands of car owners who trust AutoHelper for expert AI guidance",
    cta: "Start Using AutoHelper",
    secondary: "No credit card required",
  },
  ru: {
    title: "Готовы взять контроль над проблемами автомобиля?",
    subtitle: "Присоединяйтесь к тысячам автовладельцев, которые доверяют экспертным AI-рекомендациям AutoHelper",
    cta: "Начать использовать AutoHelper",
    secondary: "Кредитная карта не требуется",
  },
};

export function FinalCTA({ language }: FinalCTAProps) {
  const t = content[language];

  return (
    <section className="py-20 lg:py-32 bg-gradient-to-br from-primary via-primary to-primary/90 relative overflow-hidden">
      <div className="absolute inset-0 bg-[url('data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNjAiIGhlaWdodD0iNjAiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyI+PGRlZnM+PHBhdHRlcm4gaWQ9ImdyaWQiIHdpZHRoPSI2MCIgaGVpZ2h0PSI2MCIgcGF0dGVyblVuaXRzPSJ1c2VyU3BhY2VPblVzZSI+PHBhdGggZD0iTSAxMCAwIEwgMCAwIDAgMTAiIGZpbGw9Im5vbmUiIHN0cm9rZT0id2hpdGUiIHN0cm9rZS1vcGFjaXR5PSIwLjA1IiBzdHJva2Utd2lkdGg9IjEiLz48L3BhdHRlcm4+PC9kZWZzPjxyZWN0IHdpZHRoPSIxMDAlIiBoZWlnaHQ9IjEwMCUiIGZpbGw9InVybCgjZ3JpZCkiLz48L3N2Zz4=')] opacity-40"></div>

      <div className="container mx-auto px-4 lg:px-8 relative">
        <div className="max-w-4xl mx-auto text-center space-y-8">
          <h2 className="text-3xl lg:text-5xl font-semibold text-white leading-tight">
            {t.title}
          </h2>
          <p className="text-lg lg:text-xl text-white/80">
            {t.subtitle}
          </p>

          <div className="flex flex-col sm:flex-row gap-4 justify-center items-center pt-4">
            <button className="group px-8 py-4 bg-white text-primary rounded-xl hover:bg-white/95 transition-all shadow-xl hover:shadow-2xl flex items-center gap-2">
              {t.cta}
              <ArrowRight className="w-5 h-5 group-hover:translate-x-1 transition-transform" />
            </button>
            <span className="text-sm text-white/70">{t.secondary}</span>
          </div>
        </div>
      </div>
    </section>
  );
}
