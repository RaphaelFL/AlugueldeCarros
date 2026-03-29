import { Filter } from 'lucide-react';
import { Link } from 'react-router-dom';
import { Button, Card, EmptyState, Field, Input, LoadingPanel, SectionHeader, VehicleStatusBadge } from '@/components/ui';
import { formatCurrency } from '@/lib/utils';
import type { CatalogPageViewModel } from './catalog-page.logic';
import './catalog-page.css';

export function CatalogPageView({ draft, setDraft, branches, categories, vehicles, isLoading, handleSearch, resolveCategoryName, resolveBranchLabel }: Readonly<CatalogPageViewModel>) {
  return (
    <div className="catalog-page-shell mx-auto flex max-w-7xl flex-col gap-8 px-6 py-10 lg:px-8">
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
          {isLoading ? <LoadingPanel title="Buscando disponibilidade..." /> : null}
          {!isLoading && !vehicles.length ? (
            <EmptyState
              title="Nenhum veiculo encontrado"
              description="Ajuste os filtros para ampliar a busca. O backend atual retorna apenas veiculos AVAILABLE dentro do periodo informado."
            />
          ) : null}
          <div className="grid gap-4 xl:grid-cols-2">
            {vehicles.map((vehicle) => (
              <Card key={vehicle.id} className="flex flex-col gap-5">
                <div className="flex items-start justify-between gap-4">
                  <div>
                    <p className="text-sm uppercase tracking-[0.28em] text-slate-400">{resolveCategoryName(vehicle.categoryId, vehicle.category?.name)}</p>
                    <h2 className="mt-2 font-display text-2xl font-semibold text-ink">{vehicle.model}</h2>
                    <p className="mt-2 text-sm text-slate-500">{resolveBranchLabel(vehicle.branchId, vehicle.branch?.name, vehicle.branch?.address)}</p>
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
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}