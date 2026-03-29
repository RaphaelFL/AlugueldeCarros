import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useForm, type UseFormReturn } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useParams } from 'react-router-dom';
import { reservationService } from '@/api/services';
import { useReservationPayment } from '@/hooks/use-reservation-payment';
import { toDateInputValue } from '@/lib/utils';

export const updateReservationSchema = z
  .object({
    startDate: z.string().min(1, 'Selecione a nova retirada.'),
    endDate: z.string().min(1, 'Selecione a nova devolucao.'),
  })
  .refine((value) => new Date(value.endDate) > new Date(value.startDate), {
    message: 'A devolucao deve ser posterior a retirada.',
    path: ['endDate'],
  });

export type UpdateReservationFormValues = z.infer<typeof updateReservationSchema>;

export interface ReservationDetailsPageViewModel {
  reservation: Awaited<ReturnType<typeof reservationService.getById>> | null;
  isLoading: boolean;
  canCancel: boolean;
  form: UseFormReturn<UpdateReservationFormValues>;
  paymentLookup: ReturnType<typeof useReservationPayment>;
  isUpdating: boolean;
  isCancelling: boolean;
  updateError: string | null;
  cancelError: string | null;
  submitUpdate: (values: UpdateReservationFormValues) => void;
  submitCancel: () => void;
}

export function useReservationDetailsPageLogic(getErrorMessage: (error: unknown) => string): ReservationDetailsPageViewModel {
  const queryClient = useQueryClient();
  const { reservationId } = useParams();
  const reservationQuery = useQuery({
    queryKey: ['reservation', reservationId],
    queryFn: () => reservationService.getById(Number(reservationId)),
    enabled: Boolean(reservationId),
  });
  const paymentLookup = useReservationPayment(Number(reservationId));

  const form = useForm<UpdateReservationFormValues>({
    resolver: zodResolver(updateReservationSchema),
    values: reservationQuery.data
      ? {
          startDate: toDateInputValue(reservationQuery.data.startDate),
          endDate: toDateInputValue(reservationQuery.data.endDate),
        }
      : undefined,
  });

  const updateMutation = useMutation({
    mutationFn: (values: UpdateReservationFormValues) => reservationService.update(Number(reservationId), values),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['reservation', reservationId] });
      await queryClient.invalidateQueries({ queryKey: ['my-reservations'] });
    },
  });

  const cancelMutation = useMutation({
    mutationFn: () => reservationService.cancel(Number(reservationId)),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['reservation', reservationId] });
      await queryClient.invalidateQueries({ queryKey: ['my-reservations'] });
      await paymentLookup.refetch();
    },
  });

  const reservation = reservationQuery.data ?? null;
  const canCancel = reservation
    ? new Date(reservation.startDate).getTime() > Date.now() + 2 * 60 * 60 * 1000 && ['PENDING_PAYMENT', 'CONFIRMED'].includes(reservation.status)
    : false;

  return {
    reservation,
    isLoading: reservationQuery.isLoading,
    canCancel,
    form,
    paymentLookup,
    isUpdating: updateMutation.isPending,
    isCancelling: cancelMutation.isPending,
    updateError: updateMutation.isError ? getErrorMessage(updateMutation.error) : null,
    cancelError: cancelMutation.isError ? getErrorMessage(cancelMutation.error) : null,
    submitUpdate: (values) => updateMutation.mutate(values),
    submitCancel: () => cancelMutation.mutate(),
  };
}