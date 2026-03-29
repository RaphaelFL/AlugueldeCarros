import { Badge, Card, LoadingPanel, StatCard, SectionHeader } from '@/components/ui';
import { formatCurrency } from '@/lib/utils';
import type { AdminDashboardViewModel } from './admin-dashboard.logic';
import './admin-dashboard.css';

export function AdminDashboardView({ users, pricingRules, summary, isLoading }: Readonly<AdminDashboardViewModel>) {
  if (isLoading) return <LoadingPanel title="Carregando painel administrativo..." />;

  return (
    <div className="space-y-8">
      <SectionHeader title="Dashboard admin" description="Visao consolidada da operacao com foco em usuarios, frota e regras de preco." />
      <div className="grid gap-4 xl:grid-cols-4">
        <StatCard label="Usuarios" value={users.length} caption={`${summary.customers} customers e ${summary.admins} admins`} />
        <StatCard label="Veiculos" value={summary.totalVehicles} caption={`${summary.available} disponiveis para busca`} />
        <StatCard label="Pricing rules" value={pricingRules.length} caption="Regras expostas pela API" />
        <StatCard label="Painel" value="RBAC" caption="Rotas administrativas protegidas por role Admin" />
      </div>
      <div className="grid gap-6 xl:grid-cols-2">
        <Card>
          <h2 className="text-xl font-semibold text-ink">Usuarios recentes</h2>
          <div className="admin-dashboard-list mt-6 space-y-3">
            {users.slice(0, 5).map((user) => (
              <div key={user.id} className="surface-muted flex items-center justify-between gap-4 p-4">
                <div>
                  <p className="font-semibold text-ink">{user.firstName} {user.lastName}</p>
                  <p className="text-sm text-slate-500">{user.email}</p>
                </div>
                <div className="flex gap-2">
                  {user.roles.map((role) => <Badge key={role} tone={role === 'Admin' ? 'info' : 'neutral'}>{role}</Badge>)}
                </div>
              </div>
            ))}
          </div>
        </Card>
        <Card>
          <h2 className="text-xl font-semibold text-ink">Pricing rules ativas</h2>
          <div className="admin-dashboard-list mt-6 space-y-3">
            {pricingRules.map((rule) => (
              <div key={rule.id} className="surface-muted flex items-center justify-between gap-4 p-4">
                <div>
                  <p className="font-semibold text-ink">Regra #{rule.id}</p>
                  <p className="text-sm text-slate-500">Categoria #{rule.categoryId}</p>
                </div>
                <div className="text-right text-sm text-slate-600">
                  <p>{formatCurrency(rule.baseDailyRate)}</p>
                  <p>Weekend {rule.weekendMultiplier}x</p>
                </div>
              </div>
            ))}
          </div>
        </Card>
      </div>
    </div>
  );
}
