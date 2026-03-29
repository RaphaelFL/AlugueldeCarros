import { Link } from 'react-router-dom';
import { Button, Card, Field, InlineMessage, Input, SectionHeader, Select } from '@/components/ui';
import type { AdminVehicleEditorViewModel } from './admin-vehicle-editor.logic';
import './admin-vehicle-editor.css';

export function AdminVehicleEditorView({
  title,
  description,
  form,
  categories,
  branches,
  vehicleStatuses,
  isSubmitting,
  isError,
  errorMessage,
  submitLabel,
  onSubmit,
}: Readonly<AdminVehicleEditorViewModel>) {
  return (
    <div className="space-y-8">
      <SectionHeader title={title} description={description} />
      <Card className="admin-vehicle-editor-card">
        <form className="grid gap-5 md:grid-cols-2" onSubmit={form.handleSubmit(onSubmit)}>
          <Field label="Placa" error={form.formState.errors.licensePlate?.message}>
            <Input maxLength={10} {...form.register('licensePlate')} />
          </Field>
          <Field label="Modelo" error={form.formState.errors.model?.message}>
            <Input maxLength={80} {...form.register('model')} />
          </Field>
          <Field label="Ano" error={form.formState.errors.year?.message}>
            <Input type="number" {...form.register('year', { valueAsNumber: true })} />
          </Field>
          <Field label="Diaria" error={form.formState.errors.dailyRate?.message}>
            <Input type="number" step="0.01" {...form.register('dailyRate', { valueAsNumber: true })} />
          </Field>
          <Field label="Categoria" error={form.formState.errors.categoryId?.message}>
            <Select {...form.register('categoryId', { valueAsNumber: true })}>
              <option value="">Selecione</option>
              {categories.map((category) => <option key={category.id} value={category.id}>{category.name}</option>)}
            </Select>
          </Field>
          <Field label="Filial" error={form.formState.errors.branchId?.message}>
            <Select {...form.register('branchId', { valueAsNumber: true })}>
              <option value="">Selecione</option>
              {branches.map((branch) => <option key={branch.id} value={branch.id}>{branch.name}</option>)}
            </Select>
          </Field>
          <Field label="Status" error={form.formState.errors.status?.message}>
            <Select {...form.register('status')}>
              {vehicleStatuses.map((status) => <option key={status} value={status}>{status}</option>)}
            </Select>
          </Field>
          <div className="admin-vehicle-editor-actions">
            <Button type="submit" disabled={isSubmitting}>{submitLabel}</Button>
            <Link to="/admin/vehicles"><Button type="button" className="admin-vehicle-editor-cancel">Cancelar</Button></Link>
          </div>
          {isError && errorMessage ? <div className="admin-vehicle-editor-error"><InlineMessage tone="error">{errorMessage}</InlineMessage></div> : null}
        </form>
      </Card>
    </div>
  );
}
