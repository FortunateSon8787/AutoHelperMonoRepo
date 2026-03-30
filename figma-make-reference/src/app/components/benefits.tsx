import { LifeBuoy, Eye, FolderOpen, Gauge, Zap, Shield } from 'lucide-react';

interface BenefitsProps {
  language: 'en' | 'ru';
}

const content = {
  en: {
    title: "Why Choose AutoHelper",
    subtitle: "The advantages that set us apart",
    benefit1Title: "Expert Guidance in Stressful Moments",
    benefit1Desc: "Get calm, professional advice when car problems catch you off guard.",
    benefit2Title: "Transparency & Better Decisions",
    benefit2Desc: "Understand repair quotes and make choices based on facts, not fear.",
    benefit3Title: "Vehicle History in One Place",
    benefit3Desc: "Never lose track of services, repairs, or maintenance records again.",
    benefit4Title: "Simple & Convenient Interface",
    benefit4Desc: "Clean, modern design that's easy to use, even in stressful situations.",
    benefit5Title: "AI-Powered Intelligence",
    benefit5Desc: "Advanced automotive AI trained on expert knowledge and real-world scenarios.",
    benefit6Title: "Secure & Reliable",
    benefit6Desc: "Your vehicle data and history are protected with enterprise-grade security.",
  },
  ru: {
    title: "Почему выбирают AutoHelper",
    subtitle: "Преимущества, которые нас выделяют",
    benefit1Title: "Экспертные рекомендации в стрессовые моменты",
    benefit1Desc: "Получайте спокойные профессиональные советы, когда проблемы с авто застают врасплох.",
    benefit2Title: "Прозрачность и лучшие решения",
    benefit2Desc: "Понимайте предложения по ремонту и принимайте решения на основе фактов, а не страха.",
    benefit3Title: "История автомобиля в одном месте",
    benefit3Desc: "Больше не теряйте записи о сервисах, ремонтах или обслуживании.",
    benefit4Title: "Простой и удобный интерфейс",
    benefit4Desc: "Чистый, современный дизайн, которым легко пользоваться даже в стрессовых ситуациях.",
    benefit5Title: "AI-интеллект",
    benefit5Desc: "Продвинутый автомобильный AI, обученный на экспертных знаниях и реальных сценариях.",
    benefit6Title: "Безопасность и надёжность",
    benefit6Desc: "Данные вашего автомобиля и история защищены корпоративной безопасностью.",
  },
};

export function Benefits({ language }: BenefitsProps) {
  const t = content[language];

  const benefits = [
    {
      icon: LifeBuoy,
      title: t.benefit1Title,
      description: t.benefit1Desc,
      color: 'text-accent',
      bgColor: 'bg-accent/10',
    },
    {
      icon: Eye,
      title: t.benefit2Title,
      description: t.benefit2Desc,
      color: 'text-primary',
      bgColor: 'bg-primary/10',
    },
    {
      icon: FolderOpen,
      title: t.benefit3Title,
      description: t.benefit3Desc,
      color: 'text-purple-500',
      bgColor: 'bg-purple-50',
    },
    {
      icon: Gauge,
      title: t.benefit4Title,
      description: t.benefit4Desc,
      color: 'text-success',
      bgColor: 'bg-success/10',
    },
    {
      icon: Zap,
      title: t.benefit5Title,
      description: t.benefit5Desc,
      color: 'text-amber-500',
      bgColor: 'bg-amber-50',
    },
    {
      icon: Shield,
      title: t.benefit6Title,
      description: t.benefit6Desc,
      color: 'text-blue-500',
      bgColor: 'bg-blue-50',
    },
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

        <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-6">
          {benefits.map((benefit, index) => (
            <div
              key={index}
              className="bg-card border border-border rounded-2xl p-6 space-y-4 hover:shadow-lg transition-all group"
            >
              <div className={`w-12 h-12 ${benefit.bgColor} rounded-xl flex items-center justify-center group-hover:scale-110 transition-transform`}>
                <benefit.icon className={`w-6 h-6 ${benefit.color}`} />
              </div>
              <h3 className="text-lg font-medium text-foreground">
                {benefit.title}
              </h3>
              <p className="text-sm text-foreground/60 leading-relaxed">
                {benefit.description}
              </p>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}
