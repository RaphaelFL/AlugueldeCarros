import { CreditCard, ReceiptText } from 'lucide-react';
import { Button, Card, EmptyState, Field, InlineMessage, Input, LoadingPanel, PaymentStatusBadge, ReservationStatusBadge, SectionHeader, SecondaryButton, StatCard } from '@/components/ui';
import { daysBetween, formatCurrency, formatDate, formatDateTime } from '@/lib/utils';
import type { PaymentFlowPageViewModel } from './payment-flow-page.logic';
import './payment-flow-page.css';

export function PaymentFlowPageView({ reservation, paymentLookup, isLoading, feedback, form, preauthPending, capturePending, refundPending, submitPreauth, capturePayment, refundPayment }: Readonly<PaymentFlowPageViewModel>) {
  if (isLoading) return <LoadingPanel title="Montando fluxo de pagamento..." />;
  if (!reservation) return <EmptyState title="Reserva nao encontrada" description="Nao foi possivel abrir o fluxo de pagamento para esta reserva." />;

  const payment = paymentLookup.data?.payment ?? null;

  return (
    <div className="payment-flow-page-shell space-y-8">
      <SectionHeader title={`Pagamento da reserva #${reservation.id}`} description="Fluxo real da API: pre-autorizacao, captura e refund. Sem endpoints adicionais e sem simulacao fora do backend existente." />
      <div className="grid gap-6 xl:grid-cols-[1fr_0.95fr]">
        <Card className="space-y-6">
          <div className="grid gap-4 md:grid-cols-3">
            <StatCard label="Status da reserva" value={<ReservationStatusBadge status={reservation.status} />} caption={`Categoria #${reservation.categoryId}`} />
            <StatCard label="Periodo" value={`${daysBetween(reservation.startDate, reservation.endDate)} dia(s)`} caption={`${formatDate(reservation.startDate)} ate ${formatDate(reservation.endDate)}`} />
            <StatCard label="Valor total" value={formatCurrency(reservation.totalAmount)} caption="Calculado pelo backend com DailyRate do veiculo" />
          </div>
          {feedback ? <InlineMessage tone="success">{feedback}</InlineMessage> : null}
          <form className="space-y-4" onSubmit={form.handleSubmit(submitPreauth)}>
            <Field label="Valor para pre-autorizacao" error={form.formState.errors.amount?.message} hint="Use o valor total da reserva ou outro valor permitido pelo fluxo atual.">
              <Input type="number" step="0.01" {...form.register('amount', { valueAsNumber: true })} />
            </Field>
            <Button type="submit" disabled={preauthPending}>{preauthPending ? 'Criando pre-autorizacao...' : 'Criar pre-autorizacao'}</Button>
          </form>
        </Card>

        <Card className="space-y-5">
          <div className="flex items-center gap-3">
            <CreditCard className="size-5 text-accent" />
            <h2 className="font-display text-2xl font-semibold tracking-tight text-ink">Estado do pagamento</h2>
          </div>
          {paymentLookup.isLoading ? <LoadingPanel title="Consultando pagamento conhecido..." /> : null}
          {payment ? (
            <>
              <div className="surface-muted space-y-4 p-5">
                <div className="flex items-center justify-between gap-3">
                  <div>
                    <p className="text-sm text-slate-500">Pagamento #{payment.id}</p>
                    <p className="mt-2 text-2xl font-semibold text-ink">{formatCurrency(payment.amount)}</p>
                  </div>
                  <PaymentStatusBadge status={payment.status} />
                </div>
                <p className="text-sm text-slate-500">Criado em {formatDateTime(payment.createdAt)}</p>
              </div>
              <div className="flex flex-wrap gap-3">
                {payment.status === 'PENDING' ? <Button onClick={() => capturePayment(payment.id)} disabled={capturePending}>{capturePending ? 'Capturando...' : 'Capturar pagamento'}</Button> : null}
                {payment.status === 'APPROVED' ? <SecondaryButton onClick={() => refundPayment(payment.id)} disabled={refundPending}>{refundPending ? 'Processando refund...' : 'Solicitar refund'}</SecondaryButton> : null}
              </div>
            </>
          ) : (
            <InlineMessage>O front ainda nao conseguiu vincular um pagamento a esta reserva. Como a API nao oferece listagem por reserva, o produto usa operacoes executadas nesta sessao e correspondencia segura do seed para recuperar o registro quando possivel.</InlineMessage>
          )}
          <div className="surface-muted flex items-start gap-3 p-5 text-sm text-slate-600">
            <ReceiptText className="mt-0.5 size-5 text-accent" />
            <div>
              <p className="font-semibold text-ink">Estados exibidos no front</p>
              <p className="mt-2">PENDING, APPROVED, DECLINED e REFUNDED sao refletidos visualmente com badges, acoes habilitadas e mensagens de orientacao.</p>
            </div>
          </div>
        </Card>
      </div>
    </div>
  );
}