import { useMemo, useState } from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { adminService, catalogService } from '@/api/services';
import {
  Badge,
  Button,
  Card,
  EmptyState,
  Field,
  InlineMessage,
  Input,
  LoadingPanel,
  SectionHeader,
  Select,
  StatCard,
  VehicleStatusBadge,
} from '@/components/ui';
import { formatCurrency, formatDateTime, getErrorMessage } from '@/lib/utils';
import type { PricingRulePayload, VehicleStatus, VehicleUpsertPayload } from '@/types/domain';

const vehicleSchema = z.object({
  licensePlate: z.string().min(3, 'Informe a placa.'),
  model: z.string().min(2, 'Informe o modelo.'),
  year: z.coerce.number().min(2000, 'Ano invalido.'),
  categoryId: z.coerce.number().positive('Selecione a categoria.'),
  branchId: z.coerce.number().positive('Selecione a filial.'),
  dailyRate: z.coerce.number().positive('Informe uma diaria valida.'),
  status: z.enum(['AVAILABLE', 'RESERVED', 'RENTED', 'MAINTENANCE', 'BLOCKED']),
});

const pricingSchema = z.object({
  categoryId: z.coerce.number().positive('Selecione a categoria.'),
  baseDailyRate: z.coerce.number().positive('Informe a diaria base.'),
  weekendMultiplier: z.coerce.number().positive('Informe o multiplicador de fim de semana.'),
  peakSeasonMultiplier: z.coerce.number().positive('Informe o multiplicador de alta temporada.'),
});

const vehicleStatuses: VehicleStatus[] = ['AVAILABLE', 'RESERVED', 'RENTED', 'MAINTENANCE', 'BLOCKED'];

export function AdminDashboardPage() {
  const { data: users = [], isLoading: loadingUsers } = useQuery({ queryKey: ['admin-users'], queryFn: adminService.getUsers });
  const { data: vehicles = [], isLoading: loadingVehicles } = useQuery({ queryKey: ['admin-vehicles'], queryFn: () => catalogService.searchVehicles({}) });
  const { data: pricingRules = [], isLoading: loadingPricing } = useQuery({ queryKey: ['pricing-rules'], queryFn: adminService.getPricingRules });

  const summary = useMemo(() => ({
    admins: users.filter((user) => user.roles.includes('Admin')).length,
    customers: users.filter((user) => user.roles.includes('Customer')).length,
    available: vehicles.filter((vehicle) => vehicle.status === 'AVAILABLE').length,
  }), [users, vehicles]);

  if (loadingUsers || loadingVehicles || loadingPricing) return <LoadingPanel title="Carregando painel administrativo..." />;

  return (
    <div className="space-y-8">
      <SectionHeader title="Dashboard admin" description="Visao consolidada da operacao com foco em usuarios, frota e regras de preco." />
      <div className="grid gap-4 xl:grid-cols-4">
        <StatCard label="Usuarios" value={users.length} caption={`${summary.customers} customers e ${summary.admins} admins`} />
        <StatCard label="Veiculos" value={vehicles.length} caption={`${summary.available} disponiveis para busca`} />
        <StatCard label="Pricing rules" value={pricingRules.length} caption="Regras expostas pela API" />
        <StatCard label="Painel" value="RBAC" caption="Rotas administrativas protegidas por role Admin" />
      </div>
      <div className="grid gap-6 xl:grid-cols-2">
        <Card>
          <h2 className="text-xl font-semibold text-ink">Usuarios recentes</h2>
          <div className="mt-6 space-y-3">
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
          <div className="mt-6 space-y-3">
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

export function AdminUsersPage() {
  const queryClient = useQueryClient();
  const { data: users = [], isLoading } = useQuery({ queryKey: ['admin-users'], queryFn: adminService.getUsers });
  const [feedback, setFeedback] = useState<string | null>(null);

  const roleMutation = useMutation({
    mutationFn: ({ userId, roles }: { userId: number; roles: string[] }) => adminService.assignRoles(userId, roles),
    onSuccess: async () => {
      setFeedback('Perfis atualizados com sucesso.');
      await queryClient.invalidateQueries({ queryKey: ['admin-users'] });
    },
    onError: (error) => setFeedback(getErrorMessage(error)),
  });

  if (isLoading) return <LoadingPanel title="Carregando usuarios..." />;

  return (
    <div className="space-y-8">
      <SectionHeader title="Usuarios e perfis" description="Atribuicao de roles usando o endpoint administrativo real do backend." />
      {feedback ? <InlineMessage tone={roleMutation.isError ? 'error' : 'success'}>{feedback}</InlineMessage> : null}
      <div className="space-y-4">
        {users.map((user) => {
          const selectedRoles = new Set(user.roles);
          return (
            <Card key={user.id} className="space-y-4">
              <div className="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
                <div>
                  <p className="font-display text-2xl font-semibold text-ink">{user.firstName} {user.lastName}</p>
                  <p className="text-sm text-slate-500">{user.email} • criado em {formatDateTime(user.createdAt)}</p>
                </div>
                <div className="flex gap-2">
                  {user.roles.map((role) => <Badge key={role} tone={role === 'Admin' ? 'info' : 'neutral'}>{role}</Badge>)}
                </div>
              </div>
              <div className="flex flex-wrap items-center gap-6">
                {['Customer', 'Admin'].map((role) => (
                  <label key={role} className="flex items-center gap-2 text-sm font-medium text-slate-700">
                    <input
                      type="checkbox"
                      defaultChecked={selectedRoles.has(role as never)}
                      onChange={(event) => {
                        if (event.target.checked) {
                          selectedRoles.add(role as never);
                        } else {
                          selectedRoles.delete(role as never);
                        }
                      }}
                    />
                    {role}
                  </label>
                ))}
                <Button onClick={() => roleMutation.mutate({ userId: user.id, roles: Array.from(selectedRoles) })} disabled={roleMutation.isPending}>
                  Salvar roles
                </Button>
              </div>
            </Card>
          );
        })}
      </div>
    </div>
  );
}

export function AdminVehiclesPage() {
  const { data: vehicles = [], isLoading } = useQuery({ queryKey: ['admin-vehicles'], queryFn: () => catalogService.searchVehicles({}) });

  if (isLoading) return <LoadingPanel title="Carregando frota..." />;

  return (
    <div className="space-y-8">
      <SectionHeader title="Gestao de veiculos" description="Leitura e operacao da frota usando a busca publica para leitura e os endpoints admin para cadastro e edicao." action={<Link to="/admin/vehicles/new"><Button>Novo veiculo</Button></Link>} />
      <div className="space-y-4">
        {vehicles.map((vehicle) => (
          <Card key={vehicle.id} className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
            <div>
              <p className="font-display text-2xl font-semibold text-ink">{vehicle.model}</p>
              <p className="mt-2 text-sm text-slate-500">{vehicle.licensePlate} • Categoria #{vehicle.categoryId} • Filial #{vehicle.branchId}</p>
            </div>
            <div className="flex flex-wrap items-center gap-3">
              <VehicleStatusBadge status={vehicle.status} />
              <span className="text-sm font-semibold text-ink">{formatCurrency(vehicle.dailyRate)}</span>
              <Link to={`/admin/vehicles/${vehicle.id}`}><Button>Editar</Button></Link>
            </div>
          </Card>
        ))}
        {!vehicles.length ? <EmptyState title="Nenhum veiculo encontrado" description="A busca sem filtros nao retornou veiculos. Cadastre uma nova unidade administrativa." /> : null}
      </div>
    </div>
  );
}

export function AdminVehicleEditorPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { vehicleId } = useParams();
  const isEditing = Boolean(vehicleId);
  const { data: branches = [] } = useQuery({ queryKey: ['branches'], queryFn: catalogService.getBranches });
  const { data: categories = [] } = useQuery({ queryKey: ['categories'], queryFn: catalogService.getCategories });
  const { data: vehicle } = useQuery({
    queryKey: ['vehicle', vehicleId],
    queryFn: () => catalogService.getVehicle(Number(vehicleId)),
    enabled: isEditing,
  });

  const form = useForm<z.infer<typeof vehicleSchema>>({
    resolver: zodResolver(vehicleSchema),
    values: vehicle
      ? {
          licensePlate: vehicle.licensePlate,
          model: vehicle.model,
          year: vehicle.year,
          categoryId: vehicle.categoryId,
          branchId: vehicle.branchId,
          dailyRate: vehicle.dailyRate,
          status: vehicle.status,
        }
      : {
          licensePlate: '',
          model: '',
          year: 2024,
          categoryId: categories[0]?.id ?? 0,
          branchId: branches[0]?.id ?? 0,
          dailyRate: 0,
          status: 'AVAILABLE',
        },
  });

  const mutation = useMutation({
    mutationFn: async (payload: VehicleUpsertPayload) => {
      return isEditing ? adminService.updateVehicle(Number(vehicleId), payload) : adminService.createVehicle(payload);
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['admin-vehicles'] });
      navigate('/admin/vehicles');
    },
  });

  return (
    <div className="space-y-8">
      <SectionHeader title={isEditing ? 'Editar veiculo' : 'Novo veiculo'} description="Formulario alinhado aos contratos reais de CreateVehicleRequest e UpdateVehicleRequest." />
      <Card className="max-w-3xl">
        <form className="grid gap-5 md:grid-cols-2" onSubmit={form.handleSubmit((values) => mutation.mutate(values))}>
          <Field label="Placa" error={form.formState.errors.licensePlate?.message}>
            <Input {...form.register('licensePlate')} />
          </Field>
          <Field label="Modelo" error={form.formState.errors.model?.message}>
            <Input {...form.register('model')} />
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
          <div className="md:col-span-2 flex gap-3">
            <Button type="submit" disabled={mutation.isPending}>{mutation.isPending ? 'Salvando...' : 'Salvar veiculo'}</Button>
            <Link to="/admin/vehicles"><Button type="button" className="bg-slate-200 text-slate-800 hover:bg-slate-300">Cancelar</Button></Link>
          </div>
          {mutation.isError ? <div className="md:col-span-2"><InlineMessage tone="error">{getErrorMessage(mutation.error)}</InlineMessage></div> : null}
        </form>
      </Card>
    </div>
  );
}

export function AdminPricingPage() {
  const queryClient = useQueryClient();
  const { data: rules = [], isLoading } = useQuery({ queryKey: ['pricing-rules'], queryFn: adminService.getPricingRules });
  const { data: categories = [] } = useQuery({ queryKey: ['categories'], queryFn: catalogService.getCategories });
  const [editingRuleId, setEditingRuleId] = useState<number | null>(null);

  const editingRule = rules.find((rule) => rule.id === editingRuleId);
  const form = useForm<z.infer<typeof pricingSchema>>({
    resolver: zodResolver(pricingSchema),
    values: editingRule
      ? {
          categoryId: editingRule.categoryId,
          baseDailyRate: editingRule.baseDailyRate,
          weekendMultiplier: editingRule.weekendMultiplier,
          peakSeasonMultiplier: editingRule.peakSeasonMultiplier,
        }
      : {
          categoryId: categories[0]?.id ?? 0,
          baseDailyRate: 0,
          weekendMultiplier: 1,
          peakSeasonMultiplier: 1,
        },
  });

  const mutation = useMutation({
    mutationFn: async (payload: PricingRulePayload) => {
      return editingRuleId ? adminService.updatePricingRule(editingRuleId, payload) : adminService.createPricingRule(payload);
    },
    onSuccess: async () => {
      setEditingRuleId(null);
      form.reset({ categoryId: categories[0]?.id ?? 0, baseDailyRate: 0, weekendMultiplier: 1, peakSeasonMultiplier: 1 });
      await queryClient.invalidateQueries({ queryKey: ['pricing-rules'] });
    },
  });

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
          {!rules.length ? <EmptyState title="Sem regras cadastradas" description="Crie a primeira pricing rule usando o formulario lateral." /> : null}
        </Card>

        <Card>
          <h2 className="font-display text-2xl font-semibold tracking-tight text-ink">{editingRuleId ? `Editar regra #${editingRuleId}` : 'Nova pricing rule'}</h2>
          <form className="mt-6 space-y-4" onSubmit={form.handleSubmit((values) => mutation.mutate(values))}>
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
              <Button type="submit" disabled={mutation.isPending}>{mutation.isPending ? 'Salvando...' : editingRuleId ? 'Atualizar regra' : 'Criar regra'}</Button>
              {editingRuleId ? <Button type="button" className="bg-slate-200 text-slate-800 hover:bg-slate-300" onClick={() => setEditingRuleId(null)}>Cancelar edicao</Button> : null}
            </div>
            {mutation.isError ? <InlineMessage tone="error">{getErrorMessage(mutation.error)}</InlineMessage> : null}
          </form>
        </Card>
      </div>
    </div>
  );
}
