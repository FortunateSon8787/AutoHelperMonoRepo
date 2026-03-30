import { Volume2, FileQuestion, FolderCheck, Car } from 'lucide-react';

interface UseCasesProps {
  language: 'en' | 'ru';
}

const content = {
  en: {
    title: "Real Scenarios Where AutoHelper Helps",
    subtitle: "Common situations car owners face every day",
    case1Title: "Strange Engine Noise",
    case1Desc: "You hear an unusual sound from under the hood. Is it serious? AutoHelper analyzes the symptoms and tells you what to do next.",
    case2Title: "Unclear Repair Quote",
    case2Desc: "The mechanic gave you a $2,000 estimate. Is it fair? Our AI reviews the quote and helps you understand what you're paying for.",
    case3Title: "Service History Check",
    case3Desc: "When was the last oil change? What repairs have been done? Access your complete vehicle history instantly in one place.",
    case4Title: "Safe to Drive?",
    case4Desc: "A warning light appeared. Can you still drive to work? Get immediate guidance on urgency and safety considerations.",
  },
  ru: {
    title: "Реальные сценарии, где помогает AutoHelper",
    subtitle: "Обычные ситуации, с которыми автовладельцы сталкиваются каждый день",
    case1Title: "Странный шум двигателя",
    case1Desc: "Вы слышите необычный звук из-под капота. Это серьёзно? AutoHelper анализирует симптомы и подсказывает, что делать.",
    case2Title: "Непонятное предложение по ремонту",
    case2Desc: "Механик дал оценку в $2,000. Это справедливо? Наш AI проверяет предложение и помогает понять, за что вы платите.",
    case3Title: "Проверка истории обслуживания",
    case3Desc: "Когда была последняя замена масла? Какие ремонты были сделаны? Мгновенный доступ к полной истории автомобиля.",
    case4Title: "Можно ли ехать?",
    case4Desc: "Появился индикатор. Можно ли ехать на работу? Получите немедленное руководство по срочности и безопасности.",
  },
};

export function UseCases({ language }: UseCasesProps) {
  const t = content[language];

  const cases = [
    {
      icon: Volume2,
      title: t.case1Title,
      description: t.case1Desc,
      gradient: 'from-accent to-cyan-600',
    },
    {
      icon: FileQuestion,
      title: t.case2Title,
      description: t.case2Desc,
      gradient: 'from-primary to-blue-900',
    },
    {
      icon: FolderCheck,
      title: t.case3Title,
      description: t.case3Desc,
      gradient: 'from-purple-500 to-purple-700',
    },
    {
      icon: Car,
      title: t.case4Title,
      description: t.case4Desc,
      gradient: 'from-success to-emerald-600',
    },
  ];

  return (
    <section className="py-16 lg:py-24 bg-background">
      <div className="container mx-auto px-4 lg:px-8">
        <div className="text-center max-w-3xl mx-auto mb-12 lg:mb-16">
          <h2 className="text-3xl lg:text-4xl font-semibold text-foreground mb-4">
            {t.title}
          </h2>
          <p className="text-lg text-foreground/60">
            {t.subtitle}
          </p>
        </div>

        <div className="grid sm:grid-cols-2 gap-6 max-w-5xl mx-auto">
          {cases.map((useCase, index) => (
            <div
              key={index}
              className="group bg-white border border-border rounded-2xl p-8 space-y-4 hover:shadow-xl transition-all"
            >
              <div className={`w-14 h-14 bg-gradient-to-br ${useCase.gradient} rounded-xl flex items-center justify-center shadow-lg group-hover:scale-110 transition-transform`}>
                <useCase.icon className="w-7 h-7 text-white" />
              </div>
              <h3 className="text-xl font-medium text-foreground">
                {useCase.title}
              </h3>
              <p className="text-sm text-foreground/60 leading-relaxed">
                {useCase.description}
              </p>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}
