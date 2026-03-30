import { Play, MessageCircle, FileText, Shield, CheckCircle } from 'lucide-react';

interface HeroProps {
  language: 'en' | 'ru';
}

const content = {
  en: {
    headline: "Navigate Car Troubles with Confidence",
    subheadline: "AutoHelper's AI assistant helps you understand vehicle problems, avoid bad repair decisions, and stay in control when something goes wrong with your car.",
    primaryCTA: "Start with AutoHelper",
    secondaryCTA: "See How It Works",
    chatTitle: "AI Assistant",
    chatMessage: "I noticed a strange noise when braking. Should I be worried?",
    chatResponse: "Based on your description, this could indicate worn brake pads. Let me help you understand the situation.",
    diagnosticTitle: "Diagnostic Result",
    diagnosticIssue: "Probable Issue",
    diagnosticIssueValue: "Brake Pad Wear",
    diagnosticUrgency: "Urgency",
    diagnosticUrgencyValue: "Medium",
    vehicleTitle: "Your Vehicle",
    vehicleModel: "Toyota Camry 2019",
    vehicleMileage: "45,320 km",
    serviceTitle: "Service History",
    serviceRecent: "Oil Change",
    serviceDate: "2 months ago",
    trustBadge1: "Expert AI Guidance",
    trustBadge2: "Verified Information",
  },
  ru: {
    headline: "Решайте проблемы с автомобилем уверенно",
    subheadline: "AI-ассистент AutoHelper помогает понимать проблемы автомобиля, избегать плохих решений по ремонту и сохранять контроль, когда что-то идёт не так.",
    primaryCTA: "Начать с AutoHelper",
    secondaryCTA: "Как это работает",
    chatTitle: "AI Ассистент",
    chatMessage: "Я заметил странный шум при торможении. Стоит ли волноваться?",
    chatResponse: "Исходя из вашего описания, это может указывать на износ тормозных колодок. Давайте разберём ситуацию.",
    diagnosticTitle: "Результат диагностики",
    diagnosticIssue: "Вероятная проблема",
    diagnosticIssueValue: "Износ тормозных колодок",
    diagnosticUrgency: "Срочность",
    diagnosticUrgencyValue: "Средняя",
    vehicleTitle: "Ваш автомобиль",
    vehicleModel: "Toyota Camry 2019",
    vehicleMileage: "45,320 км",
    serviceTitle: "История обслуживания",
    serviceRecent: "Замена масла",
    serviceDate: "2 месяца назад",
    trustBadge1: "Экспертные AI-рекомендации",
    trustBadge2: "Проверенная информация",
  },
};

export function Hero({ language }: HeroProps) {
  const t = content[language];

  return (
    <section className="pt-32 lg:pt-40 pb-16 lg:pb-24 bg-gradient-to-b from-white to-background">
      <div className="container mx-auto px-4 lg:px-8">
        <div className="grid lg:grid-cols-2 gap-12 lg:gap-16 items-center">
          <div className="space-y-6 lg:space-y-8">
            <h1 className="text-4xl lg:text-5xl xl:text-6xl font-semibold text-foreground leading-tight">
              {t.headline}
            </h1>
            <p className="text-lg lg:text-xl text-foreground/70 leading-relaxed">
              {t.subheadline}
            </p>

            <div className="flex flex-col sm:flex-row gap-4 pt-4">
              <button className="px-8 py-4 bg-primary text-primary-foreground rounded-xl hover:bg-primary/90 transition-all shadow-lg hover:shadow-xl">
                {t.primaryCTA}
              </button>
              <button className="px-8 py-4 bg-white text-foreground border border-border rounded-xl hover:bg-secondary transition-all flex items-center justify-center gap-2 shadow-sm hover:shadow-md">
                <Play className="w-5 h-5" />
                {t.secondaryCTA}
              </button>
            </div>
          </div>

          <div className="relative">
            <div className="space-y-4">
              <div className="bg-white rounded-2xl shadow-2xl border border-border p-6 space-y-4">
                <div className="flex items-center gap-3">
                  <div className="w-10 h-10 bg-accent/10 rounded-full flex items-center justify-center">
                    <MessageCircle className="w-5 h-5 text-accent" />
                  </div>
                  <div className="font-medium text-foreground">{t.chatTitle}</div>
                </div>
                <div className="space-y-3">
                  <div className="bg-secondary p-4 rounded-xl text-sm text-foreground/80">
                    {t.chatMessage}
                  </div>
                  <div className="bg-accent/5 border border-accent/20 p-4 rounded-xl text-sm text-foreground/80">
                    {t.chatResponse}
                  </div>
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="bg-white rounded-xl shadow-lg border border-border p-5 space-y-3">
                  <div className="flex items-center gap-2">
                    <FileText className="w-4 h-4 text-accent" />
                    <div className="text-xs text-foreground/60">{t.diagnosticTitle}</div>
                  </div>
                  <div className="space-y-2">
                    <div>
                      <div className="text-xs text-foreground/50">{t.diagnosticIssue}</div>
                      <div className="text-sm font-medium text-foreground">{t.diagnosticIssueValue}</div>
                    </div>
                    <div>
                      <div className="text-xs text-foreground/50">{t.diagnosticUrgency}</div>
                      <div className="text-sm font-medium text-amber-600">{t.diagnosticUrgencyValue}</div>
                    </div>
                  </div>
                </div>

                <div className="bg-white rounded-xl shadow-lg border border-border p-5 space-y-3">
                  <div className="text-xs text-foreground/60">{t.vehicleTitle}</div>
                  <div className="space-y-2">
                    <div className="text-sm font-medium text-foreground">{t.vehicleModel}</div>
                    <div className="text-xs text-foreground/50">{t.vehicleMileage}</div>
                  </div>
                  <div className="pt-2 border-t border-border">
                    <div className="text-xs text-foreground/50">{t.serviceTitle}</div>
                    <div className="text-sm text-foreground">{t.serviceRecent}</div>
                    <div className="text-xs text-foreground/40">{t.serviceDate}</div>
                  </div>
                </div>
              </div>

              <div className="flex gap-3">
                <div className="flex-1 bg-success/5 border border-success/20 rounded-lg px-4 py-3 flex items-center gap-2">
                  <CheckCircle className="w-4 h-4 text-success flex-shrink-0" />
                  <span className="text-xs text-foreground/70">{t.trustBadge1}</span>
                </div>
                <div className="flex-1 bg-accent/5 border border-accent/20 rounded-lg px-4 py-3 flex items-center gap-2">
                  <Shield className="w-4 h-4 text-accent flex-shrink-0" />
                  <span className="text-xs text-foreground/70">{t.trustBadge2}</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}
