import { Link } from 'react-router-dom';
import { Button, Card, EmptyState, Field, InlineMessage, Input, LoadingPanel, PaymentStatusBadge, ReservationStatusBadge, SectionHeader, SecondaryButton, StatCard } from '@/components/ui';
import { formatCurrency, formatDate } from '@/lib/utils';
import type { ReservationDetailsPageViewModel } from './reservation-details-page.logic';
import './reservation-details-page.css';

export function ReservationDetailsPageView({ reservation, isLoading, canCancel, form, paymentLookup, isUpdating, isCancelling, updateError, cancelError, submitUpdate, submitCancel }: Readonly<ReservationDetailsPageViewModel>) {
  if (isLoading) return <LoadingPanel title="Carregando detalhe da reserva..." />;
  if (!reservation) return <EmptyState title="Reserva nao encontrada" description="A API nao retornou a reserva solicitada para esta sessao." />;

  return (
    <div className="reservation-details-page-shell space-y-8">
      <SectionHeader title={`Reserva #${reservation.id}`} description="Painel de acompanhamento da reserva, com remarcacao e observacao do fluxo de pagamento." action={<Link to={`/app/reservas/${reservation.id}/pagamento`}><Button>Ir para pagamento</Button></Link>} />
      <div className="grid gap-6 xl:grid-cols-[1.1fr_0.9fr]">
        <Card className="space-y-6">
          <div className="flex flex-wrap items-center gap-3">
            <ReservationStatusBadge status={reservation.status} />
            <span className="text-sm font-semibold text-ink">{formatCurrency(reservation.totalAmount)}</span>
          </div>
          <div className="grid gap-4 md:grid-cols-3">
            <StatCard label="Retirada" value={formatDate(reservation.startDate)} caption="Data enviada pela API" />
            <StatCard label="Devolucao" value={formatDate(reservation.endDate)} caption="Data enviada pela API" />
            <StatCard label="Categoria" value={`#${reservation.categoryId}`} caption={reservation.vehicleId ? `Veiculo #${reservation.vehicleId}` : 'Veiculo nao associado'} />
          </div>
          <form className="grid gap-4 md:grid-cols-2" onSubmit={form.handleSubmit(submitUpdate)}>
            <Field label="Nova retirada" error={form.formState.errors.startDate?.message}>
              <Input type="date" {...form.register('startDate')} />
            </Field>
            <Field label="Nova devolucao" error={form.formState.errors.endDate?.message}>
              <Input type="date" {...form.register('endDate')} />
            </Field>
            <div className="md:col-span-2 flex flex-wrap gap-3">
              <Button type="submit" disabled={isUpdating}>{isUpdating ? 'Salvando...' : 'Salvar remarcacao'}</Button>
              {canCancel ? <SecondaryButton type="button" onClick={submitCancel} disabled={isCancelling}>{isCancelling ? 'Cancelando...' : 'Cancelar reserva'}</SecondaryButton> : null}
            </div>
          </form>
          {updateError ? <InlineMessage tone="error">{updateError}</InlineMessage> : null}
          {cancelError ? <InlineMessage tone="error">{cancelError}</InlineMessage> : null}
          {!canCancel ? <InlineMessage>A reserva nao atende a janela atual de cancelamento do backend ou ja nao esta mais em um estado cancelavel.</InlineMessage> : null}
        </Card>

        <Card className="space-y-5">
          <h2 className="font-display text-2xl font-semibold tracking-tight text-ink">Pagamento vinculado</h2>
          {paymentLookup.isLoading ? <LoadingPanel title="Consultando pagamento associado..." /> : null}
          {paymentLookup.data?.payment ? (
            <div className="space-y-4">
              <div className="flex items-center justify-between gap-4">
                <div>
                  <p className="text-sm text-slate-500">Pagamento #{paymentLookup.data.payment.id}</p>
                  <p className="text-lg font-semibold text-ink">{formatCurrency(paymentLookup.data.payment.amount)}</p>
                </div>
                <PaymentStatusBadge status={paymentLookup.data.payment.status} />
              </div>
              <p className="text-sm text-slate-500">Origem da associacao: {paymentLookup.data.source === 'registry' ? 'operacoes executadas nesta sessao' : 'correspondencia validada no seed atual'}.</p>
              <Link to={`/app/reservas/${reservation.id}/pagamento`}><Button>Gerenciar pagamento</Button></Link>
            </div>
          ) : (
            <InlineMessage>Esta reserva ainda nao tem um pagamento resolvido pelo front. Use o fluxo de pagamento para iniciar a pre-autorizacao ou consultar o estado mais recente.</InlineMessage>
          )}
        </Card>
      </div>
    </div>
  );
}