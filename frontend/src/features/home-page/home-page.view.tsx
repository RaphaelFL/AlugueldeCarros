import { ArrowRight, MapPin } from 'lucide-react';
import { Link } from 'react-router-dom';
import { Button, Card, SecondaryButton, StatCard } from '@/components/ui';
import { homePageCapabilities, type HomePageViewModel } from './home-page.logic';
import './home-page.css';

export function HomePageView({ branchesCount, categoriesCount, primaryBranchName, highlights }: Readonly<HomePageViewModel>) {
  return (
    <div className="home-page-shell mx-auto flex max-w-7xl flex-col gap-12 px-6 py-10 lg:px-8">
      <section className="grid gap-8 lg:grid-cols-[1.1fr_0.9fr] lg:items-stretch">
        <div className="surface bg-hero-glow p-8 md:p-12">
          <div className="inline-flex items-center gap-2 rounded-full border border-white/80 bg-white/80 px-4 py-2 text-xs font-semibold uppercase tracking-[0.28em] text-accent">
            Plataforma de locacao orientada a conversao
          </div>
          <h1 className="mt-6 font-display text-5xl font-semibold leading-tight tracking-tight text-ink md:text-6xl">
            Experiencia web para reservar, pagar e operar a frota sem improviso.
          </h1>
          <p className="mt-6 max-w-2xl text-lg leading-8 text-slate-600">
            Interface profissional para o sistema de aluguel de carros, conectada aos contratos reais da API e pronta para evolucao sem refazer a base.
          </p>
          <div className="mt-8 flex flex-wrap gap-3">
            <Link to="/catalogo">
              <Button>
                Explorar catalogo
                <ArrowRight className="ml-2 size-4" />
              </Button>
            </Link>
            <Link to="/login">
              <SecondaryButton>Entrar na operacao</SecondaryButton>
            </Link>
          </div>
          <div className="mt-10 grid gap-4 md:grid-cols-3">
            <StatCard label="Filiais ativas" value={branchesCount} caption="Base operacional carregada do backend mockado" />
            <StatCard label="Categorias" value={categoriesCount} caption="Inventario pronto para busca e reserva" />
            <StatCard label="Autenticacao" value="JWT" caption="Sessao protegida para Customer e Admin" />
          </div>
        </div>
        <div className="grid gap-4">
          {highlights.map((item) => (
            <Card key={item.title} className="animate-rise">
              <div className="flex items-start gap-4">
                <div className="rounded-2xl bg-accentSoft p-3 text-accent">
                  <item.icon className="size-5" />
                </div>
                <div>
                  <h2 className="text-lg font-semibold text-ink">{item.title}</h2>
                  <p className="mt-2 text-sm leading-6 text-slate-600">{item.copy}</p>
                </div>
              </div>
            </Card>
          ))}
          <Card className="border-slate-200 bg-ink text-white">
            <p className="text-sm uppercase tracking-[0.24em] text-white/60">Fluxo suportado hoje</p>
            <div className="mt-6 flex flex-wrap items-center gap-3 text-sm font-medium text-white/80">
              <span className="rounded-full border border-white/20 px-3 py-2">Busca</span>
              <ArrowRight className="size-4 text-white/40" />
              <span className="rounded-full border border-white/20 px-3 py-2">Reserva por categoria</span>
              <ArrowRight className="size-4 text-white/40" />
              <span className="rounded-full border border-white/20 px-3 py-2">Pre-auth</span>
              <ArrowRight className="size-4 text-white/40" />
              <span className="rounded-full border border-white/20 px-3 py-2">Capture / Refund</span>
            </div>
          </Card>
        </div>
      </section>

      <section className="grid gap-4 md:grid-cols-3">
        {homePageCapabilities.map((item) => (
          <Card key={item.title}>
            <div className="flex items-center gap-3">
              {item.title === 'Base atual de atendimento' ? <MapPin className="size-5 text-accent" /> : <item.icon className="size-5 text-accent" />}
              <h3 className="text-lg font-semibold">{item.title}</h3>
            </div>
            <p className="mt-3 text-sm leading-6 text-slate-600">{item.copy(primaryBranchName)}</p>
          </Card>
        ))}
      </section>
    </div>
  );
}