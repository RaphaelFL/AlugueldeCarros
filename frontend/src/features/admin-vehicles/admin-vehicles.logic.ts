import { useQuery } from '@tanstack/react-query';
import { catalogService } from '@/api/services';

export interface AdminVehiclesViewModel {
  vehicles: Awaited<ReturnType<typeof catalogService.searchVehicles>>;
  isLoading: boolean;
}

export function useAdminVehiclesLogic(): AdminVehiclesViewModel {
  const { data: vehicles = [], isLoading } = useQuery({ queryKey: ['admin-vehicles'], queryFn: () => catalogService.searchVehicles({}) });

  return {
    vehicles,
    isLoading,
  };
}
