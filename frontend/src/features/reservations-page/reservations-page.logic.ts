import { useQuery } from '@tanstack/react-query';
import { reservationService } from '@/api/services';

export interface ReservationsPageViewModel {
  reservations: Awaited<ReturnType<typeof reservationService.getMine>>;
  isLoading: boolean;
}

export function useReservationsPageLogic(): ReservationsPageViewModel {
  const { data: reservations = [], isLoading } = useQuery({ queryKey: ['my-reservations'], queryFn: reservationService.getMine });

  return {
    reservations,
    isLoading,
  };
}