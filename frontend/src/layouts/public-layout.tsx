import { CarFront, LogOut, ShieldCheck, UserRound } from 'lucide-react';
import { Link, NavLink, Outlet } from 'react-router-dom';
import { useAuth } from '@/hooks/use-auth';
import { cn } from '@/lib/utils';
import { useAuthStore } from '@/store/auth-store';

const publicLinks = [
  { label: 'Inicio', to: '/' },
  { label: 'Catalogo', to: '/catalogo' },
];

export function PublicLayout() {
  const { isAuthenticated, isAdmin, user } = useAuth();
  const signOut = useAuthStore((state) => state.signOut);

  return (
    <div className="min-h-screen">
      <header className="sticky top-0 z-40 border-b border-white/70 bg-white/75 backdrop-blur">
        <div className="mx-auto flex max-w-7xl items-center justify-between px-6 py-4 lg:px-8">
          <Link to="/" className="flex items-center gap-3">
            <div className="rounded-2xl bg-ink p-3 text-white shadow-soft">
              <CarFront className="size-5" />
            </div>
            <div>
              <p className="font-display text-lg font-semibold tracking-tight text-ink">Aluguel de Carros</p>
              <p className="text-xs uppercase tracking-[0.24em] text-slate-500">SPA operacional</p>
            </div>
          </Link>
          <nav className="hidden items-center gap-2 md:flex">
            {publicLinks.map((link) => (
              <NavLink
                key={link.to}
                to={link.to}
                className={({ isActive }) =>
                  cn(
                    'rounded-full px-4 py-2 text-sm font-medium text-slate-600 transition hover:bg-white hover:text-ink',
                    isActive && 'bg-white text-ink shadow-soft',
                  )
                }
              >
                {link.label}
              </NavLink>
            ))}
          </nav>
          <div className="flex items-center gap-3">
            {isAuthenticated && user ? (
              <>
                <Link to={isAdmin ? '/admin/dashboard' : '/app/dashboard'} className="hidden rounded-full border border-slate-200 px-4 py-2 text-sm font-semibold text-slate-700 md:inline-flex">
                  {isAdmin ? <ShieldCheck className="mr-2 size-4" /> : <UserRound className="mr-2 size-4" />}
                  {isAdmin ? 'Area admin' : 'Minha area'}
                </Link>
                <button onClick={() => signOut()} className="inline-flex rounded-full bg-ink px-4 py-2 text-sm font-semibold text-white">
                  <LogOut className="mr-2 size-4" />
                  Sair
                </button>
              </>
            ) : (
              <Link to="/login" className="inline-flex rounded-full bg-ink px-4 py-2 text-sm font-semibold text-white">
                Entrar
              </Link>
            )}
          </div>
        </div>
      </header>
      <main>
        <Outlet />
      </main>
    </div>
  );
}
