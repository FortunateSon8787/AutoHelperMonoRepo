interface HowItWorksProps {
  language: 'en' | 'ru';
}

const content = {
  en: {
    title: "How It Works",
    subtitle: "Four simple steps to confident vehicle decisions",
    step1Title: "Describe the Problem",
    step1Desc: "Tell our AI assistant what's happening with your vehicle in plain language.",
    step2Title: "Get AI Guidance",
    step2Desc: "Receive expert analysis, possible causes, and clear recommendations instantly.",
    step3Title: "Track Vehicle History",
    step3Desc: "Log all service records, repairs, and maintenance in your personal dashboard.",
    step4Title: "Make Smarter Decisions",
    step4Desc: "Use AI insights to evaluate repair quotes and make informed service choices.",
  },
  ru: {
    title: "Как это работает",
    subtitle: "Четыре простых шага к уверенным решениям об автомобиле",
    step1Title: "Опишите проблему",
    step1Desc: "Расскажите нашему AI-ассистенту, что происходит с автомобилем, обычными словами.",
    step2Title: "Получите рекомендации AI",
    step2Desc: "Получите экспертный анализ, возможные причины и чёткие рекомендации мгновенно.",
    step3Title: "Отслеживайте историю",
    step3Desc: "Записывайте все сервисные записи, ремонты и обслуживание в личном дашборде.",
    step4Title: "Принимайте умные решения",
    step4Desc: "Используйте AI-инсайты для оценки предложений по ремонту и обоснованных решений.",
  },
};

export function HowItWorks({ language }: HowItWorksProps) {
  const t = content[language];

  const steps = [
    { title: t.step1Title, description: t.step1Desc, number: '01' },
    { title: t.step2Title, description: t.step2Desc, number: '02' },
    { title: t.step3Title, description: t.step3Desc, number: '03' },
    { title: t.step4Title, description: t.step4Desc, number: '04' },
  ];

  return (
    <section className="py-16 lg:py-24 bg-white">
      <div className="container mx-auto px-4 lg:px-8">
        <div className="text-center max-w-3xl mx-auto mb-12 lg:mb-16">
          <h2 className="text-3xl lg:text-4xl font-semibold text-foreground mb-4">
            {t.title}
          </h2>
          <p className="text-lg text-foreground/60">
            {t.subtitle}
          </p>
        </div>

        <div className="grid md:grid-cols-2 lg:grid-cols-4 gap-8">
          {steps.map((step, index) => (
            <div key={index} className="relative">
              {index < steps.length - 1 && (
                <div className="hidden lg:block absolute top-12 left-[calc(100%-2rem)] w-[calc(100%+4rem)] h-0.5 bg-gradient-to-r from-accent/30 to-accent/10" />
              )}
              <div className="relative space-y-4">
                <div className="text-5xl font-bold text-accent/20">
                  {step.number}
                </div>
                <h3 className="text-xl font-medium text-foreground">
                  {step.title}
                </h3>
                <p className="text-sm text-foreground/60 leading-relaxed">
                  {step.description}
                </p>
              </div>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}
