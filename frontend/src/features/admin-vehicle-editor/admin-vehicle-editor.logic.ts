import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { zodResolver } from '@hookform/resolvers/zod';
import { useForm, type UseFormReturn } from 'react-hook-form';
import { useNavigate, useParams } from 'react-router-dom';
import { z } from 'zod';
import { adminService, catalogService } from '@/api/services';
import type { VehicleUpsertPayload } from '@/types/domain';

export const vehicleSchema = z.object({
  licensePlate: z.string().trim().min(3, 'Informe a placa.').max(10, 'A placa deve ter no maximo 10 caracteres.').transform((value) => value.toUpperCase()),
  model: z.string().trim().min(2, 'Informe o modelo.').max(80, 'O modelo deve ter no maximo 80 caracteres.'),
  year: z.coerce.number().min(2000, 'Ano invalido.').max(2100, 'Ano invalido.'),
  categoryId: z.coerce.number().positive('Selecione a categoria.'),
  branchId: z.coerce.number().positive('Selecione a filial.'),
  dailyRate: z.coerce.number().positive('Informe uma diaria valida.').max(1000000, 'Informe uma diaria menor.'),
  status: z.enum(['AVAILABLE', 'RESERVED', 'RENTED', 'MAINTENANCE', 'BLOCKED']),
});

export type AdminVehicleEditorFormValues = z.infer<typeof vehicleSchema>;

export const vehicleStatuses = ['AVAILABLE', 'RESERVED', 'RENTED', 'MAINTENANCE', 'BLOCKED'] as const;

export interface AdminVehicleEditorViewModel {
  isEditing: boolean;
  title: string;
  description: string;
  form: UseFormReturn<AdminVehicleEditorFormValues>;
  categories: Array<{ id: number; name: string }>;
  branches: Array<{ id: number; name: string }>;
  vehicleStatuses: readonly string[];
  isSubmitting: boolean;
  isError: boolean;
  errorMessage: string | null;
  submitLabel: string;
  onSubmit: (values: AdminVehicleEditorFormValues) => void;
}

export function useAdminVehicleEditorLogic(getErrorMessage: (error: unknown) => string): AdminVehicleEditorViewModel {
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

  const form = useForm<AdminVehicleEditorFormValues>({
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

  return {
    isEditing,
    title: isEditing ? 'Editar veiculo' : 'Novo veiculo',
    description: 'Formulario alinhado aos contratos reais de CreateVehicleRequest e UpdateVehicleRequest.',
    form,
    categories: categories.map((category) => ({ id: category.id, name: category.name })),
    branches: branches.map((branch) => ({ id: branch.id, name: branch.name })),
    vehicleStatuses,
    isSubmitting: mutation.isPending,
    isError: mutation.isError,
    errorMessage: mutation.isError ? getErrorMessage(mutation.error) : null,
    submitLabel: mutation.isPending ? 'Salvando...' : 'Salvar veiculo',
    onSubmit: (values) => mutation.mutate(values),
  };
}
