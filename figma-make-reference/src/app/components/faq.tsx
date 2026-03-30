import { useState } from 'react';
import { ChevronDown } from 'lucide-react';

interface FAQProps {
  language: 'en' | 'ru';
}

const content = {
  en: {
    title: "Frequently Asked Questions",
    subtitle: "Clear answers to common questions",
    faqs: [
      {
        question: "How does the AI assistant work?",
        answer: "Our AI is trained on extensive automotive knowledge and real-world repair scenarios. You describe your car issue in plain language, and the AI analyzes the symptoms to provide probable causes, urgency levels, and actionable recommendations. It's like having an experienced mechanic friend available 24/7.",
      },
      {
        question: "Is AutoHelper a replacement for a real mechanic?",
        answer: "No, AutoHelper is designed to help you understand vehicle issues and make informed decisions, not to replace professional mechanics. We provide guidance and analysis, but always recommend consulting certified professionals for actual repairs and diagnostics.",
      },
      {
        question: "How accurate is the AI diagnosis?",
        answer: "While our AI provides highly informed guidance based on your symptoms, it cannot physically inspect your vehicle. Think of it as a first-opinion tool that helps you understand the situation before visiting a mechanic. Always confirm issues with professional diagnostic equipment.",
      },
      {
        question: "Can I cancel my subscription anytime?",
        answer: "Yes, you can cancel your subscription at any time with no penalties. Your data and vehicle history remain accessible, but AI consultation features will be limited to the free tier after cancellation.",
      },
      {
        question: "Is my vehicle data secure?",
        answer: "Absolutely. We use enterprise-grade encryption to protect your data. Your vehicle information, service history, and personal details are stored securely and never shared with third parties without your explicit consent.",
      },
      {
        question: "Do you support vehicles from all manufacturers?",
        answer: "Yes, AutoHelper supports vehicles from all major manufacturers worldwide. Our AI is trained on a comprehensive database covering various makes, models, and years, including cars popular in Moldova, CIS regions, Europe, and beyond.",
      },
    ],
  },
  ru: {
    title: "Часто задаваемые вопросы",
    subtitle: "Чёткие ответы на распространённые вопросы",
    faqs: [
      {
        question: "Как работает AI-ассистент?",
        answer: "Наш AI обучен на обширных автомобильных знаниях и реальных сценариях ремонта. Вы описываете проблему обычными словами, а AI анализирует симптомы, чтобы предоставить вероятные причины, уровень срочности и рекомендации. Это как иметь опытного друга-механика, доступного 24/7.",
      },
      {
        question: "AutoHelper заменяет настоящего механика?",
        answer: "Нет, AutoHelper создан, чтобы помочь вам понять проблемы автомобиля и принимать обоснованные решения, а не заменять профессиональных механиков. Мы предоставляем руководство и анализ, но всегда рекомендуем консультироваться с сертифицированными специалистами для реального ремонта и диагностики.",
      },
      {
        question: "Насколько точна AI-диагностика?",
        answer: "Хотя наш AI предоставляет обоснованные рекомендации на основе ваших симптомов, он не может физически осмотреть автомобиль. Думайте о нём как об инструменте для первого мнения, который помогает понять ситуацию перед визитом к механику. Всегда подтверждайте проблемы профессиональным диагностическим оборудованием.",
      },
      {
        question: "Можно ли отменить подписку в любое время?",
        answer: "Да, вы можете отменить подписку в любое время без штрафов. Ваши данные и история автомобиля остаются доступными, но функции AI-консультаций будут ограничены бесплатным тарифом после отмены.",
      },
      {
        question: "Безопасны ли данные моего автомобиля?",
        answer: "Абсолютно. Мы используем корпоративное шифрование для защиты ваших данных. Информация о вашем автомобиле, история обслуживания и личные данные хранятся безопасно и никогда не передаются третьим лицам без вашего явного согласия.",
      },
      {
        question: "Поддерживаете ли вы автомобили всех производителей?",
        answer: "Да, AutoHelper поддерживает автомобили всех основных производителей по всему миру. Наш AI обучен на всеобъемлющей базе данных, охватывающей различные марки, модели и годы, включая популярные автомобили в Молдове, регионах СНГ, Европе и за их пределами.",
      },
    ],
  },
};

export function FAQ({ language }: FAQProps) {
  const t = content[language];
  const [openIndex, setOpenIndex] = useState<number | null>(0);

  return (
    <section id="faq" className="py-16 lg:py-24 bg-white">
      <div className="container mx-auto px-4 lg:px-8">
        <div className="text-center max-w-3xl mx-auto mb-12 lg:mb-16">
          <h2 className="text-3xl lg:text-4xl font-semibold text-foreground mb-4">
            {t.title}
          </h2>
          <p className="text-lg text-foreground/60">
            {t.subtitle}
          </p>
        </div>

        <div className="max-w-3xl mx-auto space-y-4">
          {t.faqs.map((faq, index) => (
            <div
              key={index}
              className="bg-card border border-border rounded-xl overflow-hidden hover:shadow-md transition-shadow"
            >
              <button
                onClick={() => setOpenIndex(openIndex === index ? null : index)}
                className="w-full px-6 py-5 flex items-center justify-between text-left hover:bg-secondary/50 transition-colors"
              >
                <span className="font-medium text-foreground pr-4">
                  {faq.question}
                </span>
                <ChevronDown
                  className={`w-5 h-5 text-foreground/50 flex-shrink-0 transition-transform ${
                    openIndex === index ? 'rotate-180' : ''
                  }`}
                />
              </button>
              {openIndex === index && (
                <div className="px-6 pb-5 text-sm text-foreground/70 leading-relaxed">
                  {faq.answer}
                </div>
              )}
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}
