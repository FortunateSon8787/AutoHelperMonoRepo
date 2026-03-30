import { useState } from 'react';
import { Header } from './components/header';
import { Hero } from './components/hero';
import { PainPoints } from './components/pain-points';
import { Solution } from './components/solution';
import { HowItWorks } from './components/how-it-works';
import { AIShowcase } from './components/ai-showcase';
import { Benefits } from './components/benefits';
import { UseCases } from './components/use-cases';
import { Pricing } from './components/pricing';
import { Testimonials } from './components/testimonials';
import { FAQ } from './components/faq';
import { FinalCTA } from './components/final-cta';
import { Footer } from './components/footer';
import { CookieBanner } from './components/cookie-banner';

export default function App() {
  const [language, setLanguage] = useState<'en' | 'ru'>('en');
  const [showCookieBanner, setShowCookieBanner] = useState(true);

  return (
    <div className="min-h-screen bg-background text-foreground">
      <Header language={language} onLanguageChange={setLanguage} />
      <main>
        <Hero language={language} />
        <PainPoints language={language} />
        <Solution language={language} />
        <HowItWorks language={language} />
        <AIShowcase language={language} />
        <Benefits language={language} />
        <UseCases language={language} />
        <Pricing language={language} />
        <Testimonials language={language} />
        <FAQ language={language} />
        <FinalCTA language={language} />
      </main>
      <Footer language={language} />
      {showCookieBanner && (
        <CookieBanner
          language={language}
          onAccept={() => setShowCookieBanner(false)}
          onDecline={() => setShowCookieBanner(false)}
        />
      )}
    </div>
  );
}
