import { Link } from 'react-router-dom';
import { Button, Card, EmptyState, LoadingPanel, ReservationStatusBadge, SecondaryButton, SectionHeader, StatCard } from '@/components/ui';
import { formatCurrency, formatDate } from '@/lib/utils';
import type { UserDashboardViewModel } from './user-dashboard.logic';
import './user-dashboard.css';

export function UserDashboardView({ userFirstName, reservations, summary, isLoading }: Readonly<UserDashboardViewModel>) {
  if (isLoading) return <LoadingPanel title="Montando sua area..." />;

  return (
    <div className="user-dashboard-shell space-y-8">
      <SectionHeader title="Dashboard do cliente" description="Acompanhe o que precisa de acao imediata e o que ja esta confirmado na sua jornada de locacao." />
      <div className="grid gap-4 xl:grid-cols-3">
        <StatCard label="Reservas pendentes" value={summary.pending} caption="Reservas aguardando acao no fluxo de pagamento" />
        <StatCard label="Reservas confirmadas" value={summary.confirmed} caption="Operacoes prontas para acompanhamento" />
        <StatCard label="Volume contratado" value={formatCurrency(summary.upcomingValue)} caption={`Visao consolidada da conta de ${userFirstName}`} />
      </div>
      <Card>
        <div className="flex items-center justify-between gap-4">
          <div>
            <h2 className="text-xl font-semibold text-ink">Proximas reservas</h2>
            <p className="mt-2 text-sm text-slate-500">Use a lista para revisar detalhes, remarcar datas ou concluir pagamento.</p>
          </div>
          <Link to="/catalogo"><SecondaryButton>Nova reserva</SecondaryButton></Link>
        </div>
        <div className="mt-6 space-y-4">
          {reservations.slice(0, 4).map((reservation) => (
            <div key={reservation.id} className="surface-muted flex flex-col gap-4 p-4 md:flex-row md:items-center md:justify-between">
              <div>
                <p className="text-sm font-semibold text-ink">Reserva #{reservation.id}</p>
                <p className="mt-1 text-sm text-slate-500">{formatDate(reservation.startDate)} ate {formatDate(reservation.endDate)}</p>
              </div>
              <div className="flex flex-wrap items-center gap-3">
                <ReservationStatusBadge status={reservation.status} />
                <span className="text-sm font-semibold text-ink">{formatCurrency(reservation.totalAmount)}</span>
                <Link to={`/app/reservas/${reservation.id}`}><Button>Detalhes</Button></Link>
              </div>
            </div>
          ))}
          {!reservations.length ? (
            <EmptyState
              title="Nenhuma reserva por aqui"
              description="Comece pelo catalogo para criar sua primeira reserva com o fluxo real da API."
              action={<Link to="/catalogo"><Button>Ir para o catalogo</Button></Link>}
            />
          ) : null}
        </div>
      </Card>
    </div>
  );
}