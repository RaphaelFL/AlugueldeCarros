import { PaymentFlowPageView } from '@/features/payment-flow-page/payment-flow-page.view';
import { usePaymentFlowPageLogic } from '@/features/payment-flow-page/payment-flow-page.logic';
import { ProfilePageView } from '@/features/profile-page/profile-page.view';
import { useProfilePageLogic } from '@/features/profile-page/profile-page.logic';
import { ReservationDetailsPageView } from '@/features/reservation-details-page/reservation-details-page.view';
import { useReservationDetailsPageLogic } from '@/features/reservation-details-page/reservation-details-page.logic';
import { ReservationsPageView } from '@/features/reservations-page/reservations-page.view';
import { useReservationsPageLogic } from '@/features/reservations-page/reservations-page.logic';
import { UserDashboardView } from '@/features/user-dashboard/user-dashboard.view';
import { useUserDashboardLogic } from '@/features/user-dashboard/user-dashboard.logic';
import { getErrorMessage } from '@/lib/utils';

export function UserDashboardPage() {
  const viewModel = useUserDashboardLogic();

  return <UserDashboardView {...viewModel} />;
}

export function ProfilePage() {
  const viewModel = useProfilePageLogic();

  return <ProfilePageView {...viewModel} />;
}

export function ReservationsPage() {
  const viewModel = useReservationsPageLogic();

  return <ReservationsPageView {...viewModel} />;
}

export function ReservationDetailsPage() {
  const viewModel = useReservationDetailsPageLogic(getErrorMessage);

  return <ReservationDetailsPageView {...viewModel} />;
}

export function PaymentFlowPage() {
  const viewModel = usePaymentFlowPageLogic(getErrorMessage);

  return <PaymentFlowPageView {...viewModel} />;
}
