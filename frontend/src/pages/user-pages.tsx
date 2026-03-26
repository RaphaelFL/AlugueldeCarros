import { useMemo, useState } from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { CalendarDays, CircleDollarSign, Clock3, CreditCard, ReceiptText } from 'lucide-react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { paymentService, reservationService } from '@/api/services';
import {
  Button,
  Card,
  EmptyState,
  Field,
  InlineMessage,
  Input,
  LoadingPanel,
  PaymentStatusBadge,
  ReservationStatusBadge,
  SectionHeader,
  SecondaryButton,
  StatCard,
} from '@/components/ui';
import { useAuth } from '@/hooks/use-auth';
import { useReservationPayment } from '@/hooks/use-reservation-payment';
import { daysBetween, formatCurrency, formatDate, formatDateTime, getErrorMessage, toDateInputValue } from '@/lib/utils';

const updateReservationSchema = z
  .object({
    startDate: z.string().min(1, 'Selecione a nova retirada.'),
    endDate: z.string().min(1, 'Selecione a nova devolucao.'),
  })
  .refine((value) => new Date(value.endDate) > new Date(value.startDate), {
    message: 'A devolucao deve ser posterior a retirada.',
    path: ['endDate'],
  });

const preauthSchema = z.object({
  amount: z.coerce.number().positive('Informe um valor positivo.'),
});

export function UserDashboardPage() {
  const { user } = useAuth();
  const { data: reservations = [], isLoading } = useQuery({ queryKey: ['my-reservations'], queryFn: reservationService.getMine });

  const summary = useMemo(() => {
    return {
      pending: reservations.filter((reservation) => reservation.status === 'PENDING_PAYMENT').length,
      confirmed: reservations.filter((reservation) => reservation.status === 'CONFIRMED').length,
      upcomingValue: reservations.reduce((total, reservation) => total + reservation.totalAmount, 0),
    };
  }, [reservations]);

  if (isLoading) return <LoadingPanel title="Montando sua area..." />;

  return (
    <div className="space-y-8">
      <SectionHeader title="Dashboard do cliente" description="Acompanhe o que precisa de acao imediata e o que ja esta confirmado na sua jornada de locacao." />
      <div className="grid gap-4 xl:grid-cols-3">
        <StatCard label="Reservas pendentes" value={summary.pending} caption="Reservas aguardando acao no fluxo de pagamento" />
        <StatCard label="Reservas confirmadas" value={summary.confirmed} caption="Operacoes prontas para acompanhamento" />
        <StatCard label="Volume contratado" value={formatCurrency(summary.upcomingValue)} caption={`Visao consolidada da conta de ${user?.firstName ?? 'cliente'}`} />
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

export function ProfilePage() {
  const { user } = useAuth();

  if (!user) {
    return <LoadingPanel title="Carregando perfil..." />;
  }

  return (
    <div className="space-y-8">
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

export function ReservationsPage() {
  const { data: reservations = [], isLoading } = useQuery({ queryKey: ['my-reservations'], queryFn: reservationService.getMine });

  if (isLoading) return <LoadingPanel title="Carregando reservas..." />;

  return (
    <div className="space-y-8">
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

export function ReservationDetailsPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { reservationId } = useParams();
  const reservationQuery = useQuery({
    queryKey: ['reservation', reservationId],
    queryFn: () => reservationService.getById(Number(reservationId)),
    enabled: Boolean(reservationId),
  });
  const paymentLookup = useReservationPayment(Number(reservationId));

  const form = useForm<z.infer<typeof updateReservationSchema>>({
    resolver: zodResolver(updateReservationSchema),
    values: reservationQuery.data
      ? {
          startDate: toDateInputValue(reservationQuery.data.startDate),
          endDate: toDateInputValue(reservationQuery.data.endDate),
        }
      : undefined,
  });

  const updateMutation = useMutation({
    mutationFn: (values: z.infer<typeof updateReservationSchema>) => reservationService.update(Number(reservationId), values),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['reservation', reservationId] });
      await queryClient.invalidateQueries({ queryKey: ['my-reservations'] });
    },
  });

  const cancelMutation = useMutation({
    mutationFn: () => reservationService.cancel(Number(reservationId)),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['reservation', reservationId] });
      await queryClient.invalidateQueries({ queryKey: ['my-reservations'] });
      await paymentLookup.refetch();
    },
  });

  if (reservationQuery.isLoading) return <LoadingPanel title="Carregando detalhe da reserva..." />;
  if (!reservationQuery.data) return <EmptyState title="Reserva nao encontrada" description="A API nao retornou a reserva solicitada para esta sessao." />;

  const reservation = reservationQuery.data;
  const canCancel = new Date(reservation.startDate).getTime() > Date.now() + 2 * 60 * 60 * 1000 && ['PENDING_PAYMENT', 'CONFIRMED'].includes(reservation.status);

  return (
    <div className="space-y-8">
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
          <form className="grid gap-4 md:grid-cols-2" onSubmit={form.handleSubmit((values) => updateMutation.mutate(values))}>
            <Field label="Nova retirada" error={form.formState.errors.startDate?.message}>
              <Input type="date" {...form.register('startDate')} />
            </Field>
            <Field label="Nova devolucao" error={form.formState.errors.endDate?.message}>
              <Input type="date" {...form.register('endDate')} />
            </Field>
            <div className="md:col-span-2 flex flex-wrap gap-3">
              <Button type="submit" disabled={updateMutation.isPending}>{updateMutation.isPending ? 'Salvando...' : 'Salvar remarcacao'}</Button>
              {canCancel ? <SecondaryButton type="button" onClick={() => cancelMutation.mutate()} disabled={cancelMutation.isPending}>{cancelMutation.isPending ? 'Cancelando...' : 'Cancelar reserva'}</SecondaryButton> : null}
            </div>
          </form>
          {updateMutation.isError ? <InlineMessage tone="error">{getErrorMessage(updateMutation.error)}</InlineMessage> : null}
          {cancelMutation.isError ? <InlineMessage tone="error">{getErrorMessage(cancelMutation.error)}</InlineMessage> : null}
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

export function PaymentFlowPage() {
  const queryClient = useQueryClient();
  const { reservationId } = useParams();
  const reservationQuery = useQuery({
    queryKey: ['reservation', reservationId],
    queryFn: () => reservationService.getById(Number(reservationId)),
    enabled: Boolean(reservationId),
  });
  const paymentLookup = useReservationPayment(Number(reservationId));
  const [feedback, setFeedback] = useState<string | null>(null);

  const form = useForm<z.infer<typeof preauthSchema>>({
    resolver: zodResolver(preauthSchema),
    values: reservationQuery.data ? { amount: reservationQuery.data.totalAmount } : undefined,
  });

  const preauthMutation = useMutation({
    mutationFn: (values: z.infer<typeof preauthSchema>) => paymentService.preauthorize({ reservationId: Number(reservationId), amount: values.amount }),
    onSuccess: async () => {
      setFeedback('Pre-autorizacao criada com sucesso.');
      await paymentLookup.refetch();
    },
    onError: (error) => setFeedback(getErrorMessage(error)),
  });

  const captureMutation = useMutation({
    mutationFn: (paymentId: number) => paymentService.capture(paymentId),
    onSuccess: async () => {
      setFeedback('Captura processada. O status da reserva foi atualizado quando aplicavel.');
      await queryClient.invalidateQueries({ queryKey: ['reservation', reservationId] });
      await queryClient.invalidateQueries({ queryKey: ['my-reservations'] });
      await paymentLookup.refetch();
    },
    onError: (error) => setFeedback(getErrorMessage(error)),
  });

  const refundMutation = useMutation({
    mutationFn: (paymentId: number) => paymentService.refund(paymentId),
    onSuccess: async () => {
      setFeedback('Reembolso solicitado com sucesso.');
      await paymentLookup.refetch();
    },
    onError: (error) => setFeedback(getErrorMessage(error)),
  });

  if (reservationQuery.isLoading) return <LoadingPanel title="Montando fluxo de pagamento..." />;
  if (!reservationQuery.data) return <EmptyState title="Reserva nao encontrada" description="Nao foi possivel abrir o fluxo de pagamento para esta reserva." />;

  const reservation = reservationQuery.data;
  const payment = paymentLookup.data?.payment ?? null;

  return (
    <div className="space-y-8">
      <SectionHeader title={`Pagamento da reserva #${reservation.id}`} description="Fluxo real da API: pre-autorizacao, captura e refund. Sem endpoints adicionais e sem simulacao fora do backend existente." />
      <div className="grid gap-6 xl:grid-cols-[1fr_0.95fr]">
        <Card className="space-y-6">
          <div className="grid gap-4 md:grid-cols-3">
            <StatCard label="Status da reserva" value={<ReservationStatusBadge status={reservation.status} />} caption={`Categoria #${reservation.categoryId}`} />
            <StatCard label="Periodo" value={`${daysBetween(reservation.startDate, reservation.endDate)} dia(s)`} caption={`${formatDate(reservation.startDate)} ate ${formatDate(reservation.endDate)}`} />
            <StatCard label="Valor total" value={formatCurrency(reservation.totalAmount)} caption="Calculado pelo backend com DailyRate do veiculo" />
          </div>
          {feedback ? <InlineMessage tone="success">{feedback}</InlineMessage> : null}
          <form className="space-y-4" onSubmit={form.handleSubmit((values) => preauthMutation.mutate(values))}>
            <Field label="Valor para pre-autorizacao" error={form.formState.errors.amount?.message} hint="Use o valor total da reserva ou outro valor permitido pelo fluxo atual.">
              <Input type="number" step="0.01" {...form.register('amount', { valueAsNumber: true })} />
            </Field>
            <Button type="submit" disabled={preauthMutation.isPending}>{preauthMutation.isPending ? 'Criando pre-autorizacao...' : 'Criar pre-autorizacao'}</Button>
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
                {payment.status === 'PENDING' ? <Button onClick={() => captureMutation.mutate(payment.id)} disabled={captureMutation.isPending}>{captureMutation.isPending ? 'Capturando...' : 'Capturar pagamento'}</Button> : null}
                {payment.status === 'APPROVED' ? <SecondaryButton onClick={() => refundMutation.mutate(payment.id)} disabled={refundMutation.isPending}>{refundMutation.isPending ? 'Processando refund...' : 'Solicitar refund'}</SecondaryButton> : null}
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
