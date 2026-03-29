import { Link } from 'react-router-dom';
import { Button, Card, Field, InlineMessage, Input, SectionHeader, VehicleStatusBadge } from '@/components/ui';
import { formatCurrency } from '@/lib/utils';
import type { VehicleDetailsPageViewModel } from './vehicle-details-page.logic';
import './vehicle-details-page.css';

export function VehicleDetailsPageView({ vehicle, branchLabel, categoryName, categoryDescription, shouldShowLoginPrompt, loginRedirect, form, previewAmount, isSubmitting, errorMessage, submitReservation }: Readonly<VehicleDetailsPageViewModel>) {
  if (!vehicle) {
    return null;
  }

  return (
    <div className="vehicle-details-page-shell mx-auto grid max-w-7xl gap-8 px-6 py-10 lg:grid-cols-[1.1fr_0.9fr] lg:px-8">
      <Card className="overflow-hidden p-0">
        <div className="bg-hero-glow px-8 py-10">
          <p className="text-sm uppercase tracking-[0.28em] text-accent">Detalhe operacional</p>
          <h1 className="mt-3 font-display text-4xl font-semibold tracking-tight text-ink">{vehicle.model}</h1>
          <p className="mt-4 max-w-2xl text-sm leading-7 text-slate-600">
            A reserva do backend e criada por categoria. Este veiculo representa uma unidade disponivel encontrada para a categoria selecionada, mas a confirmacao final segue a regra atual da API.
          </p>
        </div>
        <div className="grid gap-4 p-8 md:grid-cols-2">
          <Card className="surface-muted p-5">
            <p className="text-xs uppercase tracking-[0.24em] text-slate-400">Categoria</p>
            <p className="mt-2 text-lg font-semibold text-ink">{categoryName}</p>
            <p className="mt-2 text-sm text-slate-500">{categoryDescription}</p>
          </Card>
          <Card className="surface-muted p-5">
            <p className="text-xs uppercase tracking-[0.24em] text-slate-400">Filial</p>
            <p className="mt-2 text-lg font-semibold text-ink">{branchLabel}</p>
          </Card>
          <Card className="surface-muted p-5">
            <p className="text-xs uppercase tracking-[0.24em] text-slate-400">Status</p>
            <div className="mt-3"><VehicleStatusBadge status={vehicle.status} /></div>
          </Card>
          <Card className="surface-muted p-5">
            <p className="text-xs uppercase tracking-[0.24em] text-slate-400">Diaria base</p>
            <p className="mt-2 text-lg font-semibold text-ink">{formatCurrency(vehicle.dailyRate)}</p>
          </Card>
        </div>
      </Card>

      <Card className="p-8">
        <SectionHeader title="Reservar categoria" description="A criacao de reserva envia categoryId, startDate e endDate exatamente como o backend espera hoje." />
        {shouldShowLoginPrompt ? (
          <div className="mt-6 space-y-4">
            <InlineMessage>Para reservar, faca login. O sistema preserva o retorno para esta pagina.</InlineMessage>
            <Link to={loginRedirect}>
              <Button>Entrar para reservar</Button>
            </Link>
          </div>
        ) : (
          <form className="mt-6 space-y-5" onSubmit={form.handleSubmit(submitReservation)}>
            <Field label="Retirada" error={form.formState.errors.startDate?.message}>
              <Input type="date" {...form.register('startDate')} />
            </Field>
            <Field label="Devolucao" error={form.formState.errors.endDate?.message}>
              <Input type="date" {...form.register('endDate')} />
            </Field>
            <Card className="surface-muted p-5">
              <p className="text-xs uppercase tracking-[0.24em] text-slate-400">Previsao de valor</p>
              <p className="mt-2 font-display text-3xl font-semibold tracking-tight text-ink">{formatCurrency(previewAmount || 0)}</p>
              <p className="mt-2 text-sm text-slate-500">Baseado na diaria do veiculo, conforme a regra atual de criacao de reserva.</p>
            </Card>
            {errorMessage ? <InlineMessage tone="error">{errorMessage}</InlineMessage> : null}
            <Button type="submit" className="w-full" disabled={isSubmitting}>
              {isSubmitting ? 'Criando reserva...' : 'Criar reserva e seguir para pagamento'}
            </Button>
          </form>
        )}
      </Card>
    </div>
  );
}