import { AdminDashboardView } from '@/features/admin-dashboard/admin-dashboard.view';
import { useAdminDashboardLogic } from '@/features/admin-dashboard/admin-dashboard.logic';
import { AdminPricingView } from '@/features/admin-pricing/admin-pricing.view';
import { useAdminPricingLogic } from '@/features/admin-pricing/admin-pricing.logic';
import { AdminUsersView } from '@/features/admin-users/admin-users.view';
import { useAdminUsersLogic } from '@/features/admin-users/admin-users.logic';
import { useAdminVehicleEditorLogic } from '@/features/admin-vehicle-editor/admin-vehicle-editor.logic';
import { AdminVehicleEditorView } from '@/features/admin-vehicle-editor/admin-vehicle-editor.view';
import { AdminVehiclesView } from '@/features/admin-vehicles/admin-vehicles.view';
import { useAdminVehiclesLogic } from '@/features/admin-vehicles/admin-vehicles.logic';
import { getErrorMessage } from '@/lib/utils';

export function AdminDashboardPage() {
  const viewModel = useAdminDashboardLogic();

  return <AdminDashboardView {...viewModel} />;
}

export function AdminUsersPage() {
  const viewModel = useAdminUsersLogic(getErrorMessage);

  return <AdminUsersView {...viewModel} />;
}

export function AdminVehiclesPage() {
  const viewModel = useAdminVehiclesLogic();

  return <AdminVehiclesView {...viewModel} />;
}

export function AdminVehicleEditorPage() {
  const viewModel = useAdminVehicleEditorLogic(getErrorMessage);

  return <AdminVehicleEditorView {...viewModel} />;
}

export function AdminPricingPage() {
  const viewModel = useAdminPricingLogic(getErrorMessage);

  return <AdminPricingView {...viewModel} />;
}
