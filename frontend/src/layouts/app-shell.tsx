import { Bell, CarFront, ChartColumn, CreditCard, LayoutDashboard, LogOut, ShieldCheck, SlidersHorizontal, UserRound, UsersRound } from 'lucide-react';
import { Link, NavLink, Outlet } from 'react-router-dom';
import { useAuth } from '@/hooks/use-auth';
import { cn } from '@/lib/utils';
import { useAuthStore } from '@/store/auth-store';

export function AppShell() {
  const { user, isAdmin } = useAuth();
  const signOut = useAuthStore((state) => state.signOut);

  const customerLinks = [
    { to: '/app/dashboard', label: 'Visao geral', icon: LayoutDashboard },
    { to: '/app/profile', label: 'Meu perfil', icon: UserRound },
    { to: '/app/reservas', label: 'Reservas', icon: CarFront },
  ];

  const adminLinks = [
    { to: '/admin/dashboard', label: 'Painel admin', icon: ShieldCheck },
    { to: '/admin/users', label: 'Usuarios', icon: UsersRound },
    { to: '/admin/vehicles', label: 'Veiculos', icon: SlidersHorizontal },
    { to: '/admin/pricing', label: 'Pricing rules', icon: ChartColumn },
  ];

  return (
    <div className="min-h-screen bg-slate-100/70">
      <div className="mx-auto grid min-h-screen max-w-[1600px] lg:grid-cols-[280px_minmax(0,1fr)]">
        <aside className="border-r border-white/80 bg-white/80 p-6 backdrop-blur">
          <Link to={isAdmin ? '/admin/dashboard' : '/app/dashboard'} className="flex items-center gap-3 rounded-3xl bg-hero-glow p-4">
            <div className="rounded-2xl bg-ink p-3 text-white shadow-soft">
              <CarFront className="size-5" />
            </div>
            <div>
              <p className="font-display text-lg font-semibold text-ink">Aluguel de Carros</p>
              <p className="text-xs uppercase tracking-[0.22em] text-slate-500">Operations UI</p>
            </div>
          </Link>

          <div className="mt-8 space-y-3">
            <p className="px-4 text-xs font-semibold uppercase tracking-[0.28em] text-slate-400">Customer</p>
            {customerLinks.map((link) => (
              <NavLink key={link.to} to={link.to} className={({ isActive }) => cn('sidebar-link', isActive && 'sidebar-link-active')}>
                <link.icon className="size-4" />
                {link.label}
              </NavLink>
            ))}
          </div>

          {isAdmin ? (
            <div className="mt-8 space-y-3">
              <p className="px-4 text-xs font-semibold uppercase tracking-[0.28em] text-slate-400">Administracao</p>
              {adminLinks.map((link) => (
                <NavLink key={link.to} to={link.to} className={({ isActive }) => cn('sidebar-link', isActive && 'sidebar-link-active')}>
                  <link.icon className="size-4" />
                  {link.label}
                </NavLink>
              ))}
            </div>
          ) : null}
        </aside>

        <div className="flex min-h-screen flex-col">
          <header className="flex flex-wrap items-center justify-between gap-4 border-b border-white/70 bg-white/75 px-6 py-5 backdrop-blur lg:px-10">
            <div>
              <p className="text-sm text-slate-500">Operacao conectada ao backend real</p>
              <h1 className="font-display text-2xl font-semibold tracking-tight text-ink">{user?.firstName} {user?.lastName}</h1>
            </div>
            <div className="flex items-center gap-3">
              <div className="hidden items-center gap-2 rounded-full border border-slate-200 bg-white px-4 py-2 text-sm text-slate-600 md:flex">
                <Bell className="size-4" />
                Sessao protegida por JWT
              </div>
              <Link to="/catalogo" className="rounded-full border border-slate-200 bg-white px-4 py-2 text-sm font-semibold text-slate-700">
                Catalogo
              </Link>
              <Link to="/app/reservas" className="rounded-full border border-slate-200 bg-white px-4 py-2 text-sm font-semibold text-slate-700">
                <CreditCard className="mr-2 inline size-4" />
                Reservas
              </Link>
              <button onClick={() => signOut()} className="rounded-full bg-ink px-4 py-2 text-sm font-semibold text-white">
                <LogOut className="mr-2 inline size-4" />
                Sair
              </button>
            </div>
          </header>
          <main className="flex-1 px-6 py-8 lg:px-10">
            <Outlet />
          </main>
        </div>
      </div>
    </div>
  );
}
