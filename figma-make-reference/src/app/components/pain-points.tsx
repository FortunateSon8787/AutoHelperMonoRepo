import { HelpCircle, DollarSign, AlertTriangle, FileX } from 'lucide-react';

interface PainPointsProps {
  language: 'en' | 'ru';
}

const content = {
  en: {
    title: "Common Car Owner Frustrations",
    subtitle: "We understand the stress of dealing with vehicle issues",
    pain1Title: "I don't understand what's wrong",
    pain1Desc: "Technical jargon and complex diagnostics leave you confused and uncertain about your car's condition.",
    pain2Title: "Fear of being overcharged",
    pain2Desc: "Service centers may suggest unnecessary repairs or inflate prices, taking advantage of your lack of expertise.",
    pain3Title: "Uncertain about necessity",
    pain3Desc: "Is this repair urgent or can it wait? You don't have the knowledge to make confident decisions.",
    pain4Title: "No organized history",
    pain4Desc: "Receipts are lost, service records are scattered, and you can't track what's been done to your vehicle.",
  },
  ru: {
    title: "Типичные проблемы автовладельцев",
    subtitle: "Мы понимаем стресс от проблем с автомобилем",
    pain1Title: "Я не понимаю, что не так",
    pain1Desc: "Технический жаргон и сложная диагностика оставляют вас в замешательстве относительно состояния автомобиля.",
    pain2Title: "Боязнь переплатить",
    pain2Desc: "Сервисные центры могут предложить ненужный ремонт или завышать цены, пользуясь отсутствием экспертизы.",
    pain3Title: "Неуверенность в необходимости",
    pain3Desc: "Этот ремонт срочный или можно подождать? У вас нет знаний для уверенных решений.",
    pain4Title: "Нет организованной истории",
    pain4Desc: "Чеки потеряны, записи о сервисе разбросаны, невозможно отследить, что делалось с автомобилем.",
  },
};

export function PainPoints({ language }: PainPointsProps) {
  const t = content[language];

  const pains = [
    {
      icon: HelpCircle,
      title: t.pain1Title,
      description: t.pain1Desc,
      color: 'text-accent',
      bgColor: 'bg-accent/5',
    },
    {
      icon: DollarSign,
      title: t.pain2Title,
      description: t.pain2Desc,
      color: 'text-destructive',
      bgColor: 'bg-destructive/5',
    },
    {
      icon: AlertTriangle,
      title: t.pain3Title,
      description: t.pain3Desc,
      color: 'text-amber-500',
      bgColor: 'bg-amber-50',
    },
    {
      icon: FileX,
      title: t.pain4Title,
      description: t.pain4Desc,
      color: 'text-purple-500',
      bgColor: 'bg-purple-50',
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

        <div className="grid sm:grid-cols-2 lg:grid-cols-4 gap-6">
          {pains.map((pain, index) => (
            <div
              key={index}
              className="bg-card border border-border rounded-2xl p-6 space-y-4 hover:shadow-lg transition-shadow"
            >
              <div className={`w-12 h-12 ${pain.bgColor} rounded-xl flex items-center justify-center`}>
                <pain.icon className={`w-6 h-6 ${pain.color}`} />
              </div>
              <h3 className="text-lg font-medium text-foreground">
                {pain.title}
              </h3>
              <p className="text-sm text-foreground/60 leading-relaxed">
                {pain.description}
              </p>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}
