import { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { adminService, catalogService } from '@/api/services';

export interface AdminDashboardViewModel {
  users: Awaited<ReturnType<typeof adminService.getUsers>>;
  pricingRules: Awaited<ReturnType<typeof adminService.getPricingRules>>;
  summary: {
    admins: number;
    customers: number;
    available: number;
    totalVehicles: number;
  };
  isLoading: boolean;
}

export function useAdminDashboardLogic(): AdminDashboardViewModel {
  const { data: users = [], isLoading: loadingUsers } = useQuery({ queryKey: ['admin-users'], queryFn: adminService.getUsers });
  const { data: vehicles = [], isLoading: loadingVehicles } = useQuery({ queryKey: ['admin-vehicles'], queryFn: () => catalogService.searchVehicles({}) });
  const { data: pricingRules = [], isLoading: loadingPricing } = useQuery({ queryKey: ['pricing-rules'], queryFn: adminService.getPricingRules });

  const summary = useMemo(() => ({
    admins: users.filter((user) => user.roles.includes('Admin')).length,
    customers: users.filter((user) => user.roles.includes('Customer')).length,
    available: vehicles.filter((vehicle) => vehicle.status === 'AVAILABLE').length,
    totalVehicles: vehicles.length,
  }), [users, vehicles]);

  return {
    users,
    pricingRules,
    summary,
    isLoading: loadingUsers || loadingVehicles || loadingPricing,
  };
}
