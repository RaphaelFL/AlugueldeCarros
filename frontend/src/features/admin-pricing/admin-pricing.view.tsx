import { Button, Card, EmptyState, Field, InlineMessage, Input, LoadingPanel, SectionHeader, Select } from '@/components/ui';
import { formatCurrency } from '@/lib/utils';
import type { AdminPricingViewModel } from './admin-pricing.logic';
import './admin-pricing.css';

export function AdminPricingView({ rules, categories, editingRuleId, form, isLoading, isPending, isError, errorMessage, submitLabel, setEditingRuleId, onSubmit }: Readonly<AdminPricingViewModel>) {
  if (isLoading) return <LoadingPanel title="Carregando pricing rules..." />;

  return (
    <div className="space-y-8">
      <SectionHeader title="Pricing rules" description="Gestao das regras de preco expostas pelo backend. A reserva continua calculando pelo DailyRate do veiculo, mas a area admin opera o recurso existente do dominio." />
      <div className="grid gap-6 xl:grid-cols-[1.1fr_0.9fr]">
        <Card className="space-y-4">
          {rules.map((rule) => (
            <div key={rule.id} className="surface-muted flex flex-col gap-4 p-4 md:flex-row md:items-center md:justify-between">
              <div>
                <p className="font-semibold text-ink">Regra #{rule.id}</p>
                <p className="mt-1 text-sm text-slate-500">Categoria #{rule.categoryId}</p>
              </div>
              <div className="grid gap-2 text-sm text-slate-600 md:text-right">
                <p>Base {formatCurrency(rule.baseDailyRate)}</p>
                <p>Weekend {rule.weekendMultiplier}x</p>
                <p>Alta temporada {rule.peakSeasonMultiplier}x</p>
              </div>
              <Button onClick={() => setEditingRuleId(rule.id)}>Editar</Button>
            </div>
          ))}
          {rules.length > 0 ? null : <EmptyState title="Sem regras cadastradas" description="Crie a primeira pricing rule usando o formulario lateral." />}
        </Card>

        <Card>
          <h2 className="font-display text-2xl font-semibold tracking-tight text-ink">{editingRuleId ? `Editar regra #${editingRuleId}` : 'Nova pricing rule'}</h2>
          <form className="admin-pricing-form mt-6 space-y-4" onSubmit={form.handleSubmit(onSubmit)}>
            <Field label="Categoria" error={form.formState.errors.categoryId?.message}>
              <Select {...form.register('categoryId', { valueAsNumber: true })}>
                <option value="">Selecione</option>
                {categories.map((category) => <option key={category.id} value={category.id}>{category.name}</option>)}
              </Select>
            </Field>
            <Field label="Base daily rate" error={form.formState.errors.baseDailyRate?.message}>
              <Input type="number" step="0.01" {...form.register('baseDailyRate', { valueAsNumber: true })} />
            </Field>
            <Field label="Weekend multiplier" error={form.formState.errors.weekendMultiplier?.message}>
              <Input type="number" step="0.01" {...form.register('weekendMultiplier', { valueAsNumber: true })} />
            </Field>
            <Field label="Peak season multiplier" error={form.formState.errors.peakSeasonMultiplier?.message}>
              <Input type="number" step="0.01" {...form.register('peakSeasonMultiplier', { valueAsNumber: true })} />
            </Field>
            <div className="flex gap-3">
              <Button type="submit" disabled={isPending}>{submitLabel}</Button>
              {editingRuleId ? <Button type="button" className="admin-pricing-cancel" onClick={() => setEditingRuleId(null)}>Cancelar edicao</Button> : null}
            </div>
            {isError && errorMessage ? <InlineMessage tone="error">{errorMessage}</InlineMessage> : null}
          </form>
        </Card>
      </div>
    </div>
  );
}
