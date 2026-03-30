import { AlertCircle, CheckCircle2, ArrowRight, Clock } from 'lucide-react';

interface AIShowcaseProps {
  language: 'en' | 'ru';
}

const content = {
  en: {
    title: "Your Specialized Automotive AI Assistant",
    subtitle: "Not just a chatbot — a structured, intelligent automotive expert",
    userMessage: "My check engine light came on and the car feels sluggish. Is it safe to drive?",
    probableIssue: "Probable Issue",
    issueValue: "Oxygen Sensor Malfunction",
    urgency: "Urgency Level",
    urgencyValue: "Medium - Can drive short distances",
    recommendations: "Recommendations",
    rec1: "Schedule diagnostic scan within 3-5 days",
    rec2: "Avoid long trips and highway driving",
    rec3: "Monitor for additional warning lights",
    nextSteps: "Next Steps",
    step1: "Book appointment with certified mechanic",
    step2: "Request full diagnostic report",
    step3: "Expected repair cost: $150-$400",
    linkedContext: "Linked to Vehicle",
    vehicleInfo: "Toyota Camry 2019 • 45,320 km",
    lastService: "Last service: 2 months ago",
  },
  ru: {
    title: "Ваш специализированный автомобильный AI-ассистент",
    subtitle: "Не просто чат-бот — структурированный, интеллектуальный автомобильный эксперт",
    userMessage: "Загорелся индикатор двигателя и машина стала вялой. Можно ли ехать?",
    probableIssue: "Вероятная проблема",
    issueValue: "Неисправность кислородного датчика",
    urgency: "Уровень срочности",
    urgencyValue: "Средний - можно ездить на короткие расстояния",
    recommendations: "Рекомендации",
    rec1: "Запланировать диагностику в течение 3-5 дней",
    rec2: "Избегать длительных поездок и скоростных трасс",
    rec3: "Следить за дополнительными индикаторами",
    nextSteps: "Следующие шаги",
    step1: "Записаться к сертифицированному механику",
    step2: "Запросить полный диагностический отчёт",
    step3: "Ожидаемая стоимость ремонта: $150-$400",
    linkedContext: "Связано с автомобилем",
    vehicleInfo: "Toyota Camry 2019 • 45,320 км",
    lastService: "Последний сервис: 2 месяца назад",
  },
};

export function AIShowcase({ language }: AIShowcaseProps) {
  const t = content[language];

  return (
    <section className="py-16 lg:py-24 bg-gradient-to-b from-background to-white">
      <div className="container mx-auto px-4 lg:px-8">
        <div className="text-center max-w-3xl mx-auto mb-12 lg:mb-16">
          <h2 className="text-3xl lg:text-4xl font-semibold text-foreground mb-4">
            {t.title}
          </h2>
          <p className="text-lg text-foreground/60">
            {t.subtitle}
          </p>
        </div>

        <div className="max-w-5xl mx-auto">
          <div className="bg-white rounded-3xl shadow-2xl border border-border overflow-hidden">
            <div className="bg-gradient-to-r from-primary to-primary/80 px-6 py-4">
              <div className="flex items-center gap-3">
                <div className="w-3 h-3 rounded-full bg-white/40"></div>
                <div className="w-3 h-3 rounded-full bg-white/40"></div>
                <div className="w-3 h-3 rounded-full bg-white/40"></div>
              </div>
            </div>

            <div className="p-6 lg:p-8 space-y-6">
              <div className="bg-secondary rounded-2xl p-5 text-foreground/80">
                {t.userMessage}
              </div>

              <div className="space-y-4">
                <div className="bg-accent/5 border-2 border-accent/30 rounded-2xl p-6 space-y-4">
                  <div className="flex items-start gap-4">
                    <div className="w-10 h-10 bg-accent rounded-xl flex items-center justify-center flex-shrink-0">
                      <AlertCircle className="w-5 h-5 text-white" />
                    </div>
                    <div className="flex-1 space-y-3">
                      <div>
                        <div className="text-xs text-foreground/50 mb-1">{t.probableIssue}</div>
                        <div className="text-lg font-medium text-foreground">{t.issueValue}</div>
                      </div>
                      <div className="flex items-center gap-2">
                        <Clock className="w-4 h-4 text-amber-500" />
                        <div>
                          <span className="text-xs text-foreground/50">{t.urgency}: </span>
                          <span className="text-sm font-medium text-amber-600">{t.urgencyValue}</span>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>

                <div className="grid md:grid-cols-2 gap-4">
                  <div className="bg-white border border-border rounded-xl p-5 space-y-3">
                    <div className="text-sm font-medium text-foreground flex items-center gap-2">
                      <CheckCircle2 className="w-4 h-4 text-success" />
                      {t.recommendations}
                    </div>
                    <ul className="space-y-2">
                      <li className="text-sm text-foreground/70 flex items-start gap-2">
                        <span className="text-success mt-0.5">•</span>
                        <span>{t.rec1}</span>
                      </li>
                      <li className="text-sm text-foreground/70 flex items-start gap-2">
                        <span className="text-success mt-0.5">•</span>
                        <span>{t.rec2}</span>
                      </li>
                      <li className="text-sm text-foreground/70 flex items-start gap-2">
                        <span className="text-success mt-0.5">•</span>
                        <span>{t.rec3}</span>
                      </li>
                    </ul>
                  </div>

                  <div className="bg-white border border-border rounded-xl p-5 space-y-3">
                    <div className="text-sm font-medium text-foreground flex items-center gap-2">
                      <ArrowRight className="w-4 h-4 text-accent" />
                      {t.nextSteps}
                    </div>
                    <ul className="space-y-2">
                      <li className="text-sm text-foreground/70 flex items-start gap-2">
                        <span className="text-accent mt-0.5">1.</span>
                        <span>{t.step1}</span>
                      </li>
                      <li className="text-sm text-foreground/70 flex items-start gap-2">
                        <span className="text-accent mt-0.5">2.</span>
                        <span>{t.step2}</span>
                      </li>
                      <li className="text-sm text-foreground/70 flex items-start gap-2">
                        <span className="text-accent mt-0.5">3.</span>
                        <span>{t.step3}</span>
                      </li>
                    </ul>
                  </div>
                </div>

                <div className="bg-primary/5 border border-primary/20 rounded-xl p-4">
                  <div className="text-xs text-foreground/50 mb-2">{t.linkedContext}</div>
                  <div className="text-sm font-medium text-foreground">{t.vehicleInfo}</div>
                  <div className="text-xs text-foreground/40 mt-1">{t.lastService}</div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}
