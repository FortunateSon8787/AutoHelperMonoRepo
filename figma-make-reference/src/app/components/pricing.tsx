import { Check, Sparkles } from 'lucide-react';

interface PricingProps {
  language: 'en' | 'ru';
}

const content = {
  en: {
    title: "Choose Your Plan",
    subtitle: "Flexible AI subscription plans for every car owner",
    regular: "Regular",
    pro: "Pro",
    maximum: "Maximum",
    perMonth: "/month",
    regularFeatures: [
      "Basic AI assistant access",
      "5 consultations per month",
      "Vehicle history storage",
      "Email support",
    ],
    proFeatures: [
      "Unlimited AI consultations",
      "Priority response time",
      "Advanced diagnostics",
      "Repair quote analysis",
      "Service reminders",
      "24/7 chat support",
    ],
    maximumFeatures: [
      "Everything in Pro",
      "Dedicated account manager",
      "Expert mechanic consultation (2/month)",
      "Custom maintenance schedules",
      "Multiple vehicle support",
      "Priority phone support",
    ],
    selectPlan: "Select Plan",
    popular: "Most Popular",
  },
  ru: {
    title: "Выберите ваш тариф",
    subtitle: "Гибкие AI-подписки для каждого автовладельца",
    regular: "Обычный",
    pro: "Про",
    maximum: "Максимум",
    perMonth: "/месяц",
    regularFeatures: [
      "Базовый доступ к AI-ассистенту",
      "5 консультаций в месяц",
      "Хранение истории автомобиля",
      "Поддержка по email",
    ],
    proFeatures: [
      "Безлимитные AI-консультации",
      "Приоритетное время ответа",
      "Продвинутая диагностика",
      "Анализ предложений по ремонту",
      "Напоминания о сервисе",
      "Поддержка в чате 24/7",
    ],
    maximumFeatures: [
      "Всё из тарифа Про",
      "Персональный менеджер",
      "Консультация эксперта-механика (2/месяц)",
      "Индивидуальные графики обслуживания",
      "Поддержка нескольких автомобилей",
      "Приоритетная телефонная поддержка",
    ],
    selectPlan: "Выбрать тариф",
    popular: "Популярный",
  },
};

export function Pricing({ language }: PricingProps) {
  const t = content[language];

  const plans = [
    {
      name: t.regular,
      price: "$9",
      features: t.regularFeatures,
      featured: false,
    },
    {
      name: t.pro,
      price: "$29",
      features: t.proFeatures,
      featured: true,
    },
    {
      name: t.maximum,
      price: "$79",
      features: t.maximumFeatures,
      featured: false,
    },
  ];

  return (
    <section id="pricing" className="py-16 lg:py-24 bg-white">
      <div className="container mx-auto px-4 lg:px-8">
        <div className="text-center max-w-3xl mx-auto mb-12 lg:mb-16">
          <h2 className="text-3xl lg:text-4xl font-semibold text-foreground mb-4">
            {t.title}
          </h2>
          <p className="text-lg text-foreground/60">
            {t.subtitle}
          </p>
        </div>

        <div className="grid md:grid-cols-3 gap-6 lg:gap-8 max-w-6xl mx-auto">
          {plans.map((plan, index) => (
            <div
              key={index}
              className={`relative bg-card border-2 rounded-2xl p-8 space-y-6 hover:shadow-xl transition-all ${
                plan.featured
                  ? 'border-accent shadow-lg scale-105'
                  : 'border-border'
              }`}
            >
              {plan.featured && (
                <div className="absolute -top-4 left-1/2 -translate-x-1/2">
                  <div className="bg-accent text-white px-4 py-1.5 rounded-full text-xs font-medium flex items-center gap-1 shadow-lg">
                    <Sparkles className="w-3 h-3" />
                    {t.popular}
                  </div>
                </div>
              )}

              <div>
                <div className="text-lg font-medium text-foreground mb-2">
                  {plan.name}
                </div>
                <div className="flex items-baseline gap-1">
                  <span className="text-4xl font-semibold text-foreground">{plan.price}</span>
                  <span className="text-foreground/50">{t.perMonth}</span>
                </div>
              </div>

              <ul className="space-y-3">
                {plan.features.map((feature, featureIndex) => (
                  <li key={featureIndex} className="flex items-start gap-3">
                    <Check className={`w-5 h-5 flex-shrink-0 mt-0.5 ${plan.featured ? 'text-accent' : 'text-success'}`} />
                    <span className="text-sm text-foreground/70">{feature}</span>
                  </li>
                ))}
              </ul>

              <button
                className={`w-full py-3 rounded-xl font-medium transition-all ${
                  plan.featured
                    ? 'bg-accent text-white hover:bg-accent/90 shadow-md hover:shadow-lg'
                    : 'bg-secondary text-foreground hover:bg-secondary/80'
                }`}
              >
                {t.selectPlan}
              </button>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}
