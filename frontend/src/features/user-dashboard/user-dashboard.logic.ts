import { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { reservationService } from '@/api/services';
import { useAuth } from '@/hooks/use-auth';

export interface UserDashboardViewModel {
  userFirstName: string;
  reservations: Awaited<ReturnType<typeof reservationService.getMine>>;
  summary: {
    pending: number;
    confirmed: number;
    upcomingValue: number;
  };
  isLoading: boolean;
}

export function useUserDashboardLogic(): UserDashboardViewModel {
  const { user } = useAuth();
  const { data: reservations = [], isLoading } = useQuery({ queryKey: ['my-reservations'], queryFn: reservationService.getMine });

  const summary = useMemo(() => ({
    pending: reservations.filter((reservation) => reservation.status === 'PENDING_PAYMENT').length,
    confirmed: reservations.filter((reservation) => reservation.status === 'CONFIRMED').length,
    upcomingValue: reservations.reduce((total, reservation) => total + reservation.totalAmount, 0),
  }), [reservations]);

  return {
    userFirstName: user?.firstName ?? 'cliente',
    reservations,
    summary,
    isLoading,
  };
}