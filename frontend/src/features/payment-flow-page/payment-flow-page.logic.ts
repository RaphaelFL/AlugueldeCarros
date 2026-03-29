import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useForm, type UseFormReturn } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useParams } from 'react-router-dom';
import { paymentService, reservationService } from '@/api/services';
import { useReservationPayment } from '@/hooks/use-reservation-payment';

export const preauthSchema = z.object({
  amount: z.coerce.number().positive('Informe um valor positivo.'),
});

export type PreauthFormValues = z.infer<typeof preauthSchema>;

export interface PaymentFlowPageViewModel {
  reservation: Awaited<ReturnType<typeof reservationService.getById>> | null;
  paymentLookup: ReturnType<typeof useReservationPayment>;
  isLoading: boolean;
  feedback: string | null;
  form: UseFormReturn<PreauthFormValues>;
  preauthPending: boolean;
  capturePending: boolean;
  refundPending: boolean;
  submitPreauth: (values: PreauthFormValues) => void;
  capturePayment: (paymentId: number) => void;
  refundPayment: (paymentId: number) => void;
}

export function usePaymentFlowPageLogic(getErrorMessage: (error: unknown) => string): PaymentFlowPageViewModel {
  const queryClient = useQueryClient();
  const { reservationId } = useParams();
  const reservationQuery = useQuery({
    queryKey: ['reservation', reservationId],
    queryFn: () => reservationService.getById(Number(reservationId)),
    enabled: Boolean(reservationId),
  });
  const paymentLookup = useReservationPayment(Number(reservationId));
  const [feedback, setFeedback] = useState<string | null>(null);

  const form = useForm<PreauthFormValues>({
    resolver: zodResolver(preauthSchema),
    values: reservationQuery.data ? { amount: reservationQuery.data.totalAmount } : undefined,
  });

  const preauthMutation = useMutation({
    mutationFn: (values: PreauthFormValues) => paymentService.preauthorize({ reservationId: Number(reservationId), amount: values.amount }),
    onSuccess: async () => {
      setFeedback('Pre-autorizacao criada com sucesso.');
      await paymentLookup.refetch();
    },
    onError: (error) => setFeedback(getErrorMessage(error)),
  });

  const captureMutation = useMutation({
    mutationFn: (paymentId: number) => paymentService.capture(paymentId),
    onSuccess: async () => {
      setFeedback('Captura processada. O status da reserva foi atualizado quando aplicavel.');
      await queryClient.invalidateQueries({ queryKey: ['reservation', reservationId] });
      await queryClient.invalidateQueries({ queryKey: ['my-reservations'] });
      await paymentLookup.refetch();
    },
    onError: (error) => setFeedback(getErrorMessage(error)),
  });

  const refundMutation = useMutation({
    mutationFn: (paymentId: number) => paymentService.refund(paymentId),
    onSuccess: async () => {
      setFeedback('Reembolso solicitado com sucesso.');
      await paymentLookup.refetch();
    },
    onError: (error) => setFeedback(getErrorMessage(error)),
  });

  return {
    reservation: reservationQuery.data ?? null,
    paymentLookup,
    isLoading: reservationQuery.isLoading,
    feedback,
    form,
    preauthPending: preauthMutation.isPending,
    capturePending: captureMutation.isPending,
    refundPending: refundMutation.isPending,
    submitPreauth: (values) => preauthMutation.mutate(values),
    capturePayment: (paymentId) => captureMutation.mutate(paymentId),
    refundPayment: (paymentId) => refundMutation.mutate(paymentId),
  };
}