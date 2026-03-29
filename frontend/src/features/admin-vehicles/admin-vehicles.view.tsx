import { Link } from 'react-router-dom';
import { Button, Card, EmptyState, LoadingPanel, SectionHeader, VehicleStatusBadge } from '@/components/ui';
import { formatCurrency } from '@/lib/utils';
import type { AdminVehiclesViewModel } from './admin-vehicles.logic';
import './admin-vehicles.css';

export function AdminVehiclesView({ vehicles, isLoading }: Readonly<AdminVehiclesViewModel>) {
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
            <div className="admin-vehicles-actions flex flex-wrap items-center gap-3">
              <VehicleStatusBadge status={vehicle.status} />
              <span className="text-sm font-semibold text-ink">{formatCurrency(vehicle.dailyRate)}</span>
              <Link to={`/admin/vehicles/${vehicle.id}`}><Button>Editar</Button></Link>
            </div>
          </Card>
        ))}
        {vehicles.length > 0 ? null : <EmptyState title="Nenhum veiculo encontrado" description="A busca sem filtros nao retornou veiculos. Cadastre uma nova unidade administrativa." />}
      </div>
    </div>
  );
}
