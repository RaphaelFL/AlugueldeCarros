import { useQuery } from '@tanstack/react-query';
import { paymentService } from '@/api/services';

export function useReservationPayment(reservationId?: number) {
  return useQuery({
    queryKey: ['reservation-payment', reservationId],
    queryFn: () => paymentService.resolveForReservation(reservationId!),
    enabled: Boolean(reservationId),
  });
}
