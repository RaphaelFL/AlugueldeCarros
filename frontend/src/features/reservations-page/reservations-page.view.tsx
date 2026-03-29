import { Link } from 'react-router-dom';
import { Button, Card, EmptyState, LoadingPanel, ReservationStatusBadge, SectionHeader } from '@/components/ui';
import { formatCurrency, formatDate } from '@/lib/utils';
import type { ReservationsPageViewModel } from './reservations-page.logic';
import './reservations-page.css';

export function ReservationsPageView({ reservations, isLoading }: Readonly<ReservationsPageViewModel>) {
  if (isLoading) return <LoadingPanel title="Carregando reservas..." />;

  return (
    <div className="reservations-page-shell space-y-8">
      <SectionHeader title="Minhas reservas" description="Visao operacional das reservas do usuario autenticado, com acesso a detalhe, remarcacao e pagamento." action={<Link to="/catalogo"><Button>Nova reserva</Button></Link>} />
      <div className="space-y-4">
        {reservations.map((reservation) => (
          <Card key={reservation.id} className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
            <div>
              <div className="flex flex-wrap items-center gap-3">
                <p className="font-display text-2xl font-semibold text-ink">Reserva #{reservation.id}</p>
                <ReservationStatusBadge status={reservation.status} />
              </div>
              <p className="mt-2 text-sm text-slate-500">{formatDate(reservation.startDate)} ate {formatDate(reservation.endDate)} • Categoria #{reservation.categoryId}</p>
            </div>
            <div className="flex flex-wrap items-center gap-3">
              <span className="text-sm font-semibold text-ink">{formatCurrency(reservation.totalAmount)}</span>
              <Link to={`/app/reservas/${reservation.id}`}><Button>Ver detalhe</Button></Link>
            </div>
          </Card>
        ))}
        {!reservations.length ? (
          <EmptyState title="Nenhuma reserva encontrada" description="Assim que uma reserva for criada, ela aparece aqui com o status real do backend." action={<Link to="/catalogo"><Button>Explorar catalogo</Button></Link>} />
        ) : null}
      </div>
    </div>
  );
}