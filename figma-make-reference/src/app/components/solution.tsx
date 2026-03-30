import { Brain, Search, History, TrendingUp } from 'lucide-react';

interface SolutionProps {
  language: 'en' | 'ru';
}

const content = {
  en: {
    title: "How AutoHelper Solves These Problems",
    subtitle: "Your trusted AI companion for all vehicle-related decisions",
    feature1Title: "AI Explains Issues",
    feature1Desc: "Get clear, simple explanations of what might be wrong with your car in language you understand.",
    feature2Title: "Repair Situation Review",
    feature2Desc: "Analyze service quotes and repair recommendations to understand if they're fair and necessary.",
    feature3Title: "Organized History",
    feature3Desc: "Keep all your vehicle records, receipts, and service history in one secure, accessible place.",
    feature4Title: "Confident Decisions",
    feature4Desc: "Make informed choices about repairs and maintenance with expert AI guidance at your fingertips.",
  },
  ru: {
    title: "Как AutoHelper решает эти проблемы",
    subtitle: "Ваш надёжный AI-помощник для всех решений, связанных с автомобилем",
    feature1Title: "AI объясняет проблемы",
    feature1Desc: "Получайте понятные объяснения того, что может быть не так с вашим автомобилем, на доступном языке.",
    feature2Title: "Анализ ситуации с ремонтом",
    feature2Desc: "Анализируйте предложения сервиса и рекомендации по ремонту, чтобы понять их справедливость.",
    feature3Title: "Организованная история",
    feature3Desc: "Храните все записи об автомобиле, чеки и историю обслуживания в одном безопасном месте.",
    feature4Title: "Уверенные решения",
    feature4Desc: "Принимайте обоснованные решения о ремонте и обслуживании с экспертными AI-рекомендациями.",
  },
};

export function Solution({ language }: SolutionProps) {
  const t = content[language];

  const features = [
    {
      icon: Brain,
      title: t.feature1Title,
      description: t.feature1Desc,
      gradient: 'from-accent to-cyan-600',
    },
    {
      icon: Search,
      title: t.feature2Title,
      description: t.feature2Desc,
      gradient: 'from-primary to-blue-900',
    },
    {
      icon: History,
      title: t.feature3Title,
      description: t.feature3Desc,
      gradient: 'from-purple-500 to-purple-700',
    },
    {
      icon: TrendingUp,
      title: t.feature4Title,
      description: t.feature4Desc,
      gradient: 'from-success to-emerald-600',
    },
  ];

  return (
    <section id="features" className="py-16 lg:py-24 bg-background">
      <div className="container mx-auto px-4 lg:px-8">
        <div className="text-center max-w-3xl mx-auto mb-12 lg:mb-16">
          <h2 className="text-3xl lg:text-4xl font-semibold text-foreground mb-4">
            {t.title}
          </h2>
          <p className="text-lg text-foreground/60">
            {t.subtitle}
          </p>
        </div>

        <div className="grid sm:grid-cols-2 lg:grid-cols-4 gap-6">
          {features.map((feature, index) => (
            <div
              key={index}
              className="group bg-white border border-border rounded-2xl p-6 space-y-4 hover:shadow-xl transition-all"
            >
              <div className={`w-14 h-14 bg-gradient-to-br ${feature.gradient} rounded-xl flex items-center justify-center shadow-lg group-hover:scale-110 transition-transform`}>
                <feature.icon className="w-7 h-7 text-white" />
              </div>
              <h3 className="text-lg font-medium text-foreground">
                {feature.title}
              </h3>
              <p className="text-sm text-foreground/60 leading-relaxed">
                {feature.description}
              </p>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}
