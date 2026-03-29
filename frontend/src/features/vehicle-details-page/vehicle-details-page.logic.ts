import { useMutation, useQuery } from '@tanstack/react-query';
import { useForm, type UseFormReturn } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useNavigate, useParams, useSearchParams } from 'react-router-dom';
import { catalogService, reservationService } from '@/api/services';
import { useAuth } from '@/hooks/use-auth';
import { daysBetween, formatPhone } from '@/lib/utils';

export const reservationSchema = z
  .object({
    startDate: z.string().min(1, 'Selecione a data de retirada.'),
    endDate: z.string().min(1, 'Selecione a data de devolucao.'),
  })
  .refine((value) => new Date(value.endDate) > new Date(value.startDate), {
    message: 'A devolucao deve ser posterior a retirada.',
    path: ['endDate'],
  });

export type ReservationFormValues = z.infer<typeof reservationSchema>;

export interface VehicleDetailsPageViewModel {
  isLoading: boolean;
  vehicle: Awaited<ReturnType<typeof catalogService.getVehicle>> | undefined;
  branchLabel: string;
  categoryName: string;
  categoryDescription: string;
  shouldShowLoginPrompt: boolean;
  loginRedirect: string;
  form: UseFormReturn<ReservationFormValues>;
  previewAmount: number;
  isSubmitting: boolean;
  errorMessage: string | null;
  submitReservation: (values: ReservationFormValues) => void;
}

export function useVehicleDetailsPageLogic(getErrorMessage: (error: unknown) => string): VehicleDetailsPageViewModel {
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

  const form = useForm<ReservationFormValues>({
    resolver: zodResolver(reservationSchema),
    defaultValues: {
      startDate: searchParams.get('from') || '',
      endDate: searchParams.get('to') || '',
    },
  });

  const createReservationMutation = useMutation({
    mutationFn: async (values: ReservationFormValues) =>
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
    : vehicle?.dailyRate ?? 0;

  return {
    isLoading,
    vehicle,
    branchLabel: `${branch?.name ?? `Filial #${vehicle?.branchId ?? '-'}`} • ${branch?.address ?? 'Endereco nao informado'} • ${formatPhone(branch?.phone)}`,
    categoryName: category?.name ?? `Categoria #${vehicle?.categoryId ?? '-'}`,
    categoryDescription: category?.description ?? 'Descricao indisponivel.',
    shouldShowLoginPrompt: isAuthenticated === false,
    loginRedirect: `/login?redirect=${encodeURIComponent(`/catalogo/${vehicle?.id ?? ''}`)}`,
    form,
    previewAmount,
    isSubmitting: createReservationMutation.isPending,
    errorMessage: createReservationMutation.isError ? getErrorMessage(createReservationMutation.error) : null,
    submitReservation: (values) => createReservationMutation.mutate(values),
  };
}