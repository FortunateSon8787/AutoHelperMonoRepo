import { Star } from 'lucide-react';

interface TestimonialsProps {
  language: 'en' | 'ru';
}

const content = {
  en: {
    title: "What Our Users Say",
    subtitle: "Real feedback from car owners who trust AutoHelper",
    testimonials: [
      {
        name: "Dmitri Volkov",
        role: "Car Owner, Chisinau",
        quote: "AutoHelper saved me from an unnecessary $800 repair. The AI explained the issue clearly and I found a much better solution.",
        rating: 5,
      },
      {
        name: "Elena Popescu",
        role: "Freelancer, Bucharest",
        quote: "As someone who knows nothing about cars, this is a lifesaver. No more anxiety when the check engine light comes on.",
        rating: 5,
      },
      {
        name: "Andrei Sokolov",
        role: "Business Owner, Moscow",
        quote: "The service history feature alone is worth it. Finally, all my vehicle records are organized in one place.",
        rating: 5,
      },
      {
        name: "Maria Ionescu",
        role: "Teacher, Iasi",
        quote: "I feel confident making decisions about my car now. The AI is like having a trusted mechanic friend always available.",
        rating: 5,
      },
      {
        name: "Pavel Ivanov",
        role: "Developer, Minsk",
        quote: "Clean interface, smart AI, and genuinely helpful. This is what modern car ownership should look like.",
        rating: 5,
      },
      {
        name: "Natalia Kozlov",
        role: "Marketing Manager, Kiev",
        quote: "AutoHelper helped me understand a complex transmission issue. I avoided a rushed decision and saved money.",
        rating: 5,
      },
    ],
  },
  ru: {
    title: "Что говорят наши пользователи",
    subtitle: "Реальные отзывы автовладельцев, которые доверяют AutoHelper",
    testimonials: [
      {
        name: "Дмитрий Волков",
        role: "Автовладелец, Кишинёв",
        quote: "AutoHelper спас меня от ненужного ремонта за $800. AI чётко объяснил проблему и я нашёл лучшее решение.",
        rating: 5,
      },
      {
        name: "Елена Попеску",
        role: "Фрилансер, Бухарест",
        quote: "Как человек, который ничего не понимает в машинах, это спасение. Больше никакого беспокойства при индикаторах.",
        rating: 5,
      },
      {
        name: "Андрей Соколов",
        role: "Владелец бизнеса, Москва",
        quote: "Функция истории обслуживания сама по себе стоит денег. Наконец-то все записи организованы в одном месте.",
        rating: 5,
      },
      {
        name: "Мария Ионеску",
        role: "Учитель, Яссы",
        quote: "Теперь я уверенно принимаю решения о своём автомобиле. AI как надёжный друг-механик всегда доступен.",
        rating: 5,
      },
      {
        name: "Павел Иванов",
        role: "Разработчик, Минск",
        quote: "Чистый интерфейс, умный AI и действительно полезно. Вот как должно выглядеть владение автомобилем.",
        rating: 5,
      },
      {
        name: "Наталья Козлова",
        role: "Менеджер по маркетингу, Киев",
        quote: "AutoHelper помог разобраться со сложной проблемой трансмиссии. Избежала спешного решения и сэкономила деньги.",
        rating: 5,
      },
    ],
  },
};

export function Testimonials({ language }: TestimonialsProps) {
  const t = content[language];

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

        <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-6 max-w-7xl mx-auto">
          {t.testimonials.map((testimonial, index) => (
            <div
              key={index}
              className="bg-white border border-border rounded-2xl p-6 space-y-4 hover:shadow-lg transition-shadow"
            >
              <div className="flex gap-1">
                {[...Array(testimonial.rating)].map((_, i) => (
                  <Star key={i} className="w-4 h-4 fill-amber-400 text-amber-400" />
                ))}
              </div>
              <p className="text-sm text-foreground/70 leading-relaxed">
                "{testimonial.quote}"
              </p>
              <div className="pt-4 border-t border-border">
                <div className="flex items-center gap-3">
                  <div className="w-10 h-10 bg-gradient-to-br from-accent to-primary rounded-full flex items-center justify-center text-white font-medium">
                    {testimonial.name.charAt(0)}
                  </div>
                  <div>
                    <div className="text-sm font-medium text-foreground">
                      {testimonial.name}
                    </div>
                    <div className="text-xs text-foreground/50">
                      {testimonial.role}
                    </div>
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}
