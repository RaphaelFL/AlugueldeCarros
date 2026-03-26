import { useMemo, useState } from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation, useQuery } from '@tanstack/react-query';
import { ArrowRight, CalendarClock, CheckCircle2, Compass, Filter, MapPin, ShieldCheck, Sparkles, WalletCards } from 'lucide-react';
import { Link, Navigate, useNavigate, useParams, useSearchParams } from 'react-router-dom';
import { authService, catalogService, reservationService } from '@/api/services';
import { Button, Card, EmptyState, Field, InlineMessage, Input, LoadingPanel, SecondaryButton, SectionHeader, StatCard, VehicleStatusBadge } from '@/components/ui';
import { useAuth } from '@/hooks/use-auth';
import { daysBetween, formatCurrency, formatDate, formatPhone, getErrorMessage } from '@/lib/utils';
import { useAuthStore } from '@/store/auth-store';

const authSchema = z.object({
  email: z.string().email('Informe um e-mail valido.'),
  password: z.string().min(6, 'A senha precisa ter ao menos 6 caracteres.'),
});

const registerSchema = authSchema.extend({
  firstName: z.string().min(2, 'Informe o nome.'),
  lastName: z.string().min(2, 'Informe o sobrenome.'),
});

const reservationSchema = z
  .object({
    startDate: z.string().min(1, 'Selecione a data de retirada.'),
    endDate: z.string().min(1, 'Selecione a data de devolucao.'),
  })
  .refine((value) => new Date(value.endDate) > new Date(value.startDate), {
    message: 'A devolucao deve ser posterior a retirada.',
    path: ['endDate'],
  });

export function HomePage() {
  const { data: branches = [] } = useQuery({ queryKey: ['branches'], queryFn: catalogService.getBranches });
  const { data: categories = [] } = useQuery({ queryKey: ['categories'], queryFn: catalogService.getCategories });

  const highlights = [
    {
      title: 'Busca com contexto operacional',
      copy: 'O cliente encontra categorias e veiculos com filtros reais de filial, periodo e faixa de diaria, sem promessas fora do backend atual.',
      icon: Compass,
    },
    {
      title: 'Fluxo de reserva com pagamento conectado',
      copy: 'A jornada segue a regra do sistema: disponibilidade, criacao da reserva, pre-autorizacao e captura.',
      icon: WalletCards,
    },
    {
      title: 'Operacao administrativa integrada',
      copy: 'Admins operam usuarios, frota e pricing rules na mesma SPA, respeitando JWT e role-based access.',
      icon: ShieldCheck,
    },
  ];

  return (
    <div className="mx-auto flex max-w-7xl flex-col gap-12 px-6 py-10 lg:px-8">
      <section className="grid gap-8 lg:grid-cols-[1.1fr_0.9fr] lg:items-stretch">
        <div className="surface bg-hero-glow p-8 md:p-12">
          <div className="inline-flex items-center gap-2 rounded-full border border-white/80 bg-white/80 px-4 py-2 text-xs font-semibold uppercase tracking-[0.28em] text-accent">
            <Sparkles className="size-4" />
            Plataforma de locacao orientada a conversao
          </div>
          <h1 className="mt-6 font-display text-5xl font-semibold leading-tight tracking-tight text-ink md:text-6xl">
            Experiencia web para reservar, pagar e operar a frota sem improviso.
          </h1>
          <p className="mt-6 max-w-2xl text-lg leading-8 text-slate-600">
            Interface profissional para o sistema de aluguel de carros, conectada aos contratos reais da API e pronta para evolucao sem refazer a base.
          </p>
          <div className="mt-8 flex flex-wrap gap-3">
            <Link to="/catalogo">
              <Button>
                Explorar catalogo
                <ArrowRight className="ml-2 size-4" />
              </Button>
            </Link>
            <Link to="/login">
              <SecondaryButton>Entrar na operacao</SecondaryButton>
            </Link>
          </div>
          <div className="mt-10 grid gap-4 md:grid-cols-3">
            <StatCard label="Filiais ativas" value={branches.length} caption="Base operacional carregada do backend mockado" />
            <StatCard label="Categorias" value={categories.length} caption="Inventario pronto para busca e reserva" />
            <StatCard label="Autenticacao" value="JWT" caption="Sessao protegida para Customer e Admin" />
          </div>
        </div>
        <div className="grid gap-4">
          {highlights.map((item) => (
            <Card key={item.title} className="animate-rise">
              <div className="flex items-start gap-4">
                <div className="rounded-2xl bg-accentSoft p-3 text-accent">
                  <item.icon className="size-5" />
                </div>
                <div>
                  <h2 className="text-lg font-semibold text-ink">{item.title}</h2>
                  <p className="mt-2 text-sm leading-6 text-slate-600">{item.copy}</p>
                </div>
              </div>
            </Card>
          ))}
          <Card className="border-slate-200 bg-ink text-white">
            <p className="text-sm uppercase tracking-[0.24em] text-white/60">Fluxo suportado hoje</p>
            <div className="mt-6 flex flex-wrap items-center gap-3 text-sm font-medium text-white/80">
              <span className="rounded-full border border-white/20 px-3 py-2">Busca</span>
              <ArrowRight className="size-4 text-white/40" />
              <span className="rounded-full border border-white/20 px-3 py-2">Reserva por categoria</span>
              <ArrowRight className="size-4 text-white/40" />
              <span className="rounded-full border border-white/20 px-3 py-2">Pre-auth</span>
              <ArrowRight className="size-4 text-white/40" />
              <span className="rounded-full border border-white/20 px-3 py-2">Capture / Refund</span>
            </div>
          </Card>
        </div>
      </section>

      <section className="grid gap-4 md:grid-cols-3">
        <Card>
          <div className="flex items-center gap-3">
            <MapPin className="size-5 text-accent" />
            <h3 className="text-lg font-semibold">Base atual de atendimento</h3>
          </div>
          <p className="mt-3 text-sm leading-6 text-slate-600">A operacao carrega hoje {branches[0]?.name ?? 'uma filial principal'}, pronta para buscas publicas e atendimento autenticado.</p>
        </Card>
        <Card>
          <div className="flex items-center gap-3">
            <CalendarClock className="size-5 text-accent" />
            <h3 className="text-lg font-semibold">Estados claros de jornada</h3>
          </div>
          <p className="mt-3 text-sm leading-6 text-slate-600">Reservas e pagamentos exibem status reais do backend para orientar proxima acao sem ambiguidade.</p>
        </Card>
        <Card>
          <div className="flex items-center gap-3">
            <CheckCircle2 className="size-5 text-accent" />
            <h3 className="text-lg font-semibold">RBAC aplicado no produto</h3>
          </div>
          <p className="mt-3 text-sm leading-6 text-slate-600">Menus, rotas e acoes mudam conforme o papel da sessao, separando Customer e Admin de forma objetiva.</p>
        </Card>
      </section>
    </div>
  );
}

export function LoginPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const redirect = searchParams.get('redirect');
  const { isAuthenticated, isAdmin } = useAuth();
  const setSession = useAuthStore((state) => state.setSession);
  const [mode, setMode] = useState<'login' | 'register'>('login');
  const [feedback, setFeedback] = useState<string | null>(null);

  const loginForm = useForm<z.infer<typeof authSchema>>({
    resolver: zodResolver(authSchema),
    defaultValues: { email: 'customer@example.com', password: '123456' },
  });

  const registerForm = useForm<z.infer<typeof registerSchema>>({
    resolver: zodResolver(registerSchema),
    defaultValues: { email: '', password: '', firstName: '', lastName: '' },
  });

  const authMutation = useMutation({
    mutationFn: async (values: z.infer<typeof authSchema>) => {
      const auth = await authService.login(values);
      const user = await authService.me();
      setSession(auth.token, user);
      return user;
    },
    onSuccess: (user) => {
      navigate(redirect || (user.roles.includes('Admin') ? '/admin/dashboard' : '/app/dashboard'));
    },
    onError: (error) => setFeedback(getErrorMessage(error)),
  });

  const registerMutation = useMutation({
    mutationFn: async (values: z.infer<typeof registerSchema>) => {
      const auth = await authService.register(values);
      const user = await authService.me();
      setSession(auth.token, user);
      return user;
    },
    onSuccess: () => navigate('/app/dashboard'),
    onError: (error) => setFeedback(getErrorMessage(error)),
  });

  if (isAuthenticated) {
    return <Navigate to={isAdmin ? '/admin/dashboard' : '/app/dashboard'} replace />;
  }

  return (
    <div className="mx-auto grid min-h-[calc(100vh-88px)] max-w-7xl gap-8 px-6 py-10 lg:grid-cols-[0.95fr_1.05fr] lg:px-8">
      <Card className="bg-hero-glow p-8 md:p-10">
        <p className="text-sm uppercase tracking-[0.32em] text-accent">Autenticacao oficial</p>
        <h1 className="mt-4 font-display text-4xl font-semibold tracking-tight text-ink">Acesse a jornada certa para cliente ou operacao admin.</h1>
        <p className="mt-4 text-sm leading-7 text-slate-600">
          A sessao do front reflete o comportamento real da API: JWT no cliente, refresh quando suportado, rotas protegidas e menus guiados por roles.
        </p>
        <div className="mt-8 space-y-4 text-sm text-slate-600">
          <div className="surface-muted p-4">
            <p className="font-semibold text-ink">Customer de teste</p>
            <p>customer@example.com</p>
            <p>123456</p>
          </div>
          <div className="surface-muted p-4">
            <p className="font-semibold text-ink">Admin de teste</p>
            <p>admin@aluguel.com</p>
            <p>admin123</p>
          </div>
        </div>
      </Card>

      <Card className="p-8 md:p-10">
        <div className="inline-flex rounded-full bg-slate-100 p-1">
          <button
            onClick={() => setMode('login')}
            className={`rounded-full px-4 py-2 text-sm font-semibold ${mode === 'login' ? 'bg-white text-ink shadow-soft' : 'text-slate-500'}`}
          >
            Login
          </button>
          <button
            onClick={() => setMode('register')}
            className={`rounded-full px-4 py-2 text-sm font-semibold ${mode === 'register' ? 'bg-white text-ink shadow-soft' : 'text-slate-500'}`}
          >
            Cadastro
          </button>
        </div>

        {feedback ? <div className="mt-6"><InlineMessage tone="error">{feedback}</InlineMessage></div> : null}

        {mode === 'login' ? (
          <form className="mt-6 space-y-5" onSubmit={loginForm.handleSubmit((values) => authMutation.mutate(values))}>
            <Field label="E-mail" error={loginForm.formState.errors.email?.message}>
              <Input type="email" {...loginForm.register('email')} />
            </Field>
            <Field label="Senha" error={loginForm.formState.errors.password?.message}>
              <Input type="password" {...loginForm.register('password')} />
            </Field>
            <Button type="submit" className="w-full" disabled={authMutation.isPending}>
              {authMutation.isPending ? 'Autenticando...' : 'Entrar'}
            </Button>
          </form>
        ) : (
          <form className="mt-6 grid gap-5 md:grid-cols-2" onSubmit={registerForm.handleSubmit((values) => registerMutation.mutate(values))}>
            <Field label="Nome" error={registerForm.formState.errors.firstName?.message}>
              <Input {...registerForm.register('firstName')} />
            </Field>
            <Field label="Sobrenome" error={registerForm.formState.errors.lastName?.message}>
              <Input {...registerForm.register('lastName')} />
            </Field>
            <div className="md:col-span-2">
              <Field label="E-mail" error={registerForm.formState.errors.email?.message}>
                <Input type="email" {...registerForm.register('email')} />
              </Field>
            </div>
            <div className="md:col-span-2">
              <Field label="Senha" error={registerForm.formState.errors.password?.message} hint="A API aceita no minimo 6 caracteres no fluxo atual.">
                <Input type="password" {...registerForm.register('password')} />
              </Field>
            </div>
            <div className="md:col-span-2">
              <Button type="submit" className="w-full" disabled={registerMutation.isPending}>
                {registerMutation.isPending ? 'Criando conta...' : 'Criar conta e iniciar sessao'}
              </Button>
            </div>
          </form>
        )}
      </Card>
    </div>
  );
}

export function CatalogPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const [draft, setDraft] = useState({
    branchId: searchParams.get('branchId') || '',
    categoryId: searchParams.get('categoryId') || '',
    from: searchParams.get('from') || '',
    to: searchParams.get('to') || '',
    priceMin: searchParams.get('priceMin') || '',
    priceMax: searchParams.get('priceMax') || '',
  });

  const filters = useMemo(() => ({
    branchId: draft.branchId ? Number(draft.branchId) : undefined,
    categoryId: draft.categoryId ? Number(draft.categoryId) : undefined,
    from: draft.from || undefined,
    to: draft.to || undefined,
    priceMin: draft.priceMin ? Number(draft.priceMin) : undefined,
    priceMax: draft.priceMax ? Number(draft.priceMax) : undefined,
  }), [draft]);

  const { data: branches = [] } = useQuery({ queryKey: ['branches'], queryFn: catalogService.getBranches });
  const { data: categories = [] } = useQuery({ queryKey: ['categories'], queryFn: catalogService.getCategories });
  const vehiclesQuery = useQuery({ queryKey: ['vehicles', filters], queryFn: () => catalogService.searchVehicles(filters) });

  const categoryMap = new Map(categories.map((category) => [category.id, category]));
  const branchMap = new Map(branches.map((branch) => [branch.id, branch]));

  const handleSearch = () => {
    const params = new URLSearchParams();
    Object.entries(draft).forEach(([key, value]) => {
      if (value) params.set(key, value);
    });
    setSearchParams(params);
    void vehiclesQuery.refetch();
  };

  return (
    <div className="mx-auto flex max-w-7xl flex-col gap-8 px-6 py-10 lg:px-8">
      <SectionHeader title="Catalogo de disponibilidade" description="Busque categorias e veiculos com os filtros reais expostos pela API. A reserva continua sendo por categoria, mas a interface usa o veiculo encontrado para dar contexto visual e facilitar a decisao." />

      <div className="grid gap-8 lg:grid-cols-[320px_minmax(0,1fr)]">
        <Card className="h-fit space-y-5">
          <div className="flex items-center gap-2">
            <Filter className="size-4 text-accent" />
            <h2 className="text-lg font-semibold text-ink">Filtros</h2>
          </div>
          <Field label="Filial">
            <select className="w-full rounded-2xl border border-slate-200 px-4 py-3 text-sm" value={draft.branchId} onChange={(event) => setDraft((state) => ({ ...state, branchId: event.target.value }))}>
              <option value="">Todas</option>
              {branches.map((branch) => (
                <option key={branch.id} value={branch.id}>{branch.name}</option>
              ))}
            </select>
          </Field>
          <Field label="Categoria">
            <select className="w-full rounded-2xl border border-slate-200 px-4 py-3 text-sm" value={draft.categoryId} onChange={(event) => setDraft((state) => ({ ...state, categoryId: event.target.value }))}>
              <option value="">Todas</option>
              {categories.map((category) => (
                <option key={category.id} value={category.id}>{category.name}</option>
              ))}
            </select>
          </Field>
          <Field label="Retirada">
            <Input type="date" value={draft.from} onChange={(event) => setDraft((state) => ({ ...state, from: event.target.value }))} />
          </Field>
          <Field label="Devolucao">
            <Input type="date" value={draft.to} onChange={(event) => setDraft((state) => ({ ...state, to: event.target.value }))} />
          </Field>
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-1">
            <Field label="Diaria minima">
              <Input type="number" min="0" value={draft.priceMin} onChange={(event) => setDraft((state) => ({ ...state, priceMin: event.target.value }))} />
            </Field>
            <Field label="Diaria maxima">
              <Input type="number" min="0" value={draft.priceMax} onChange={(event) => setDraft((state) => ({ ...state, priceMax: event.target.value }))} />
            </Field>
          </div>
          <Button type="button" className="w-full" onClick={handleSearch}>Atualizar busca</Button>
        </Card>

        <div className="space-y-4">
          {vehiclesQuery.isLoading ? <LoadingPanel title="Buscando disponibilidade..." /> : null}
          {!vehiclesQuery.isLoading && !vehiclesQuery.data?.length ? (
            <EmptyState
              title="Nenhum veiculo encontrado"
              description="Ajuste os filtros para ampliar a busca. O backend atual retorna apenas veiculos AVAILABLE dentro do periodo informado."
            />
          ) : null}
          <div className="grid gap-4 xl:grid-cols-2">
            {vehiclesQuery.data?.map((vehicle) => {
              const category = vehicle.category ?? categoryMap.get(vehicle.categoryId);
              const branch = vehicle.branch ?? branchMap.get(vehicle.branchId);
              return (
                <Card key={vehicle.id} className="flex flex-col gap-5">
                  <div className="flex items-start justify-between gap-4">
                    <div>
                      <p className="text-sm uppercase tracking-[0.28em] text-slate-400">{category?.name ?? 'Categoria'}</p>
                      <h2 className="mt-2 font-display text-2xl font-semibold text-ink">{vehicle.model}</h2>
                      <p className="mt-2 text-sm text-slate-500">{branch?.name ?? 'Filial nao informada'} • {branch?.address ?? 'Endereco indisponivel'}</p>
                    </div>
                    <VehicleStatusBadge status={vehicle.status} />
                  </div>
                  <div className="grid gap-3 text-sm text-slate-600 md:grid-cols-2">
                    <div className="surface-muted p-4">
                      <p className="text-xs uppercase tracking-[0.24em] text-slate-400">Placa</p>
                      <p className="mt-2 font-semibold text-ink">{vehicle.licensePlate}</p>
                    </div>
                    <div className="surface-muted p-4">
                      <p className="text-xs uppercase tracking-[0.24em] text-slate-400">Diaria</p>
                      <p className="mt-2 font-semibold text-ink">{formatCurrency(vehicle.dailyRate)}</p>
                    </div>
                  </div>
                  <div className="flex flex-wrap gap-3">
                    <Link to={`/catalogo/${vehicle.id}?from=${draft.from}&to=${draft.to}`}>
                      <Button>Ver detalhes e reservar</Button>
                    </Link>
                  </div>
                </Card>
              );
            })}
          </div>
        </div>
      </div>
    </div>
  );
}

export function VehicleDetailsPage() {
  const navigate = useNavigate();
  const { vehicleId } = useParams();
  const [searchParams] = useSearchParams();
  const { isAuthenticated } = useAuth();
  const { data: vehicle, isLoading } = useQuery({
    queryKey: ['vehicle', vehicleId],
    queryFn: () => catalogService.getVehicle(Number(vehicleId)),
    enabled: Boolean(vehicleId),
  });
  const { data: branches = [] } = useQuery({ queryKey: ['branches'], queryFn: catalogService.getBranches });
  const { data: categories = [] } = useQuery({ queryKey: ['categories'], queryFn: catalogService.getCategories });

  const form = useForm<z.infer<typeof reservationSchema>>({
    resolver: zodResolver(reservationSchema),
    defaultValues: {
      startDate: searchParams.get('from') || '',
      endDate: searchParams.get('to') || '',
    },
  });

  const createReservationMutation = useMutation({
    mutationFn: async (values: z.infer<typeof reservationSchema>) =>
      reservationService.create({
        categoryId: vehicle!.categoryId,
        startDate: values.startDate,
        endDate: values.endDate,
      }),
    onSuccess: (reservation) => navigate(`/app/reservas/${reservation.id}/pagamento`),
  });

  const branch = branches.find((item) => item.id === vehicle?.branchId);
  const category = categories.find((item) => item.id === vehicle?.categoryId);
  const previewAmount = form.watch('startDate') && form.watch('endDate') && vehicle
    ? daysBetween(form.watch('startDate'), form.watch('endDate')) * vehicle.dailyRate
    : vehicle?.dailyRate;

  if (isLoading) return <div className="mx-auto max-w-7xl px-6 py-10 lg:px-8"><LoadingPanel title="Carregando detalhe do veiculo..." /></div>;
  if (!vehicle) return <NotFoundPage />;

  return (
    <div className="mx-auto grid max-w-7xl gap-8 px-6 py-10 lg:grid-cols-[1.1fr_0.9fr] lg:px-8">
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
            <p className="mt-2 text-lg font-semibold text-ink">{category?.name ?? `Categoria #${vehicle.categoryId}`}</p>
            <p className="mt-2 text-sm text-slate-500">{category?.description ?? 'Descricao indisponivel.'}</p>
          </Card>
          <Card className="surface-muted p-5">
            <p className="text-xs uppercase tracking-[0.24em] text-slate-400">Filial</p>
            <p className="mt-2 text-lg font-semibold text-ink">{branch?.name ?? `Filial #${vehicle.branchId}`}</p>
            <p className="mt-2 text-sm text-slate-500">{branch?.address ?? 'Endereco nao informado'} • {formatPhone(branch?.phone)}</p>
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
        {!isAuthenticated ? (
          <div className="mt-6 space-y-4">
            <InlineMessage>Para reservar, faca login. O sistema preserva o retorno para esta pagina.</InlineMessage>
            <Link to={`/login?redirect=${encodeURIComponent(`/catalogo/${vehicle.id}`)}`}>
              <Button>Entrar para reservar</Button>
            </Link>
          </div>
        ) : (
          <form className="mt-6 space-y-5" onSubmit={form.handleSubmit((values) => createReservationMutation.mutate(values))}>
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
            {createReservationMutation.isError ? <InlineMessage tone="error">{getErrorMessage(createReservationMutation.error)}</InlineMessage> : null}
            <Button type="submit" className="w-full" disabled={createReservationMutation.isPending}>
              {createReservationMutation.isPending ? 'Criando reserva...' : 'Criar reserva e seguir para pagamento'}
            </Button>
          </form>
        )}
      </Card>
    </div>
  );
}

export function NotFoundPage() {
  return (
    <div className="mx-auto flex min-h-screen max-w-3xl items-center justify-center px-6 py-10">
      <EmptyState
        title="Pagina nao encontrada"
        description="A rota solicitada nao existe nesta SPA. Use o menu principal para voltar a uma area valida do produto."
        action={<Link to="/"><Button>Voltar ao inicio</Button></Link>}
      />
    </div>
  );
}
