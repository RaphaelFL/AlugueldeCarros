import { Card, LoadingPanel, SectionHeader } from '@/components/ui';
import { formatDateTime } from '@/lib/utils';
import type { ProfilePageViewModel } from './profile-page.logic';
import './profile-page.css';

export function ProfilePageView({ user, isLoading }: Readonly<ProfilePageViewModel>) {
  if (isLoading || !user) {
    return <LoadingPanel title="Carregando perfil..." />;
  }

  return (
    <div className="profile-page-shell space-y-8">
      <SectionHeader title="Meu perfil" description="Dados retornados diretamente por /api/v1/users/me, sem campos inventados pelo front-end." />
      <div className="grid gap-6 xl:grid-cols-[1fr_0.8fr]">
        <Card className="space-y-6">
          <div className="grid gap-4 md:grid-cols-2">
            <Card className="surface-muted p-5">
              <p className="text-xs uppercase tracking-[0.24em] text-slate-400">Nome</p>
              <p className="mt-2 text-lg font-semibold text-ink">{user.firstName} {user.lastName}</p>
            </Card>
            <Card className="surface-muted p-5">
              <p className="text-xs uppercase tracking-[0.24em] text-slate-400">E-mail</p>
              <p className="mt-2 text-lg font-semibold text-ink">{user.email}</p>
            </Card>
            <Card className="surface-muted p-5">
              <p className="text-xs uppercase tracking-[0.24em] text-slate-400">Criado em</p>
              <p className="mt-2 text-lg font-semibold text-ink">{formatDateTime(user.createdAt)}</p>
            </Card>
            <Card className="surface-muted p-5">
              <p className="text-xs uppercase tracking-[0.24em] text-slate-400">Perfis</p>
              <p className="mt-2 text-lg font-semibold text-ink">{user.roles.join(', ')}</p>
            </Card>
          </div>
        </Card>
        <Card className="bg-hero-glow p-6">
          <h2 className="font-display text-2xl font-semibold tracking-tight text-ink">Sessao e seguranca</h2>
          <p className="mt-3 text-sm leading-7 text-slate-600">
            Este front utiliza JWT do backend e tenta renovar a sessao quando o token se aproxima da expiracao. Em caso de 401, a aplicacao encerra a sessao e redireciona para login.
          </p>
        </Card>
      </div>
    </div>
  );
}