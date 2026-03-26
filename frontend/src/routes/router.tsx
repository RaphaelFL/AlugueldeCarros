import { lazy, Suspense, type ReactNode } from 'react';
import { QueryClient } from '@tanstack/react-query';
import { createBrowserRouter, Navigate, Outlet, RouterProvider, useLocation } from 'react-router-dom';
import { useAuth } from '@/hooks/use-auth';
import { AppShell } from '@/layouts/app-shell';
import { PublicLayout } from '@/layouts/public-layout';

const HomePage = lazy(() => import('@/pages/public-pages').then((module) => ({ default: module.HomePage })));
const LoginPage = lazy(() => import('@/pages/public-pages').then((module) => ({ default: module.LoginPage })));
const CatalogPage = lazy(() => import('@/pages/public-pages').then((module) => ({ default: module.CatalogPage })));
const VehicleDetailsPage = lazy(() => import('@/pages/public-pages').then((module) => ({ default: module.VehicleDetailsPage })));
const NotFoundPage = lazy(() => import('@/pages/public-pages').then((module) => ({ default: module.NotFoundPage })));

const UserDashboardPage = lazy(() => import('@/pages/user-pages').then((module) => ({ default: module.UserDashboardPage })));
const ProfilePage = lazy(() => import('@/pages/user-pages').then((module) => ({ default: module.ProfilePage })));
const ReservationsPage = lazy(() => import('@/pages/user-pages').then((module) => ({ default: module.ReservationsPage })));
const ReservationDetailsPage = lazy(() => import('@/pages/user-pages').then((module) => ({ default: module.ReservationDetailsPage })));
const PaymentFlowPage = lazy(() => import('@/pages/user-pages').then((module) => ({ default: module.PaymentFlowPage })));

const AdminDashboardPage = lazy(() => import('@/pages/admin-pages').then((module) => ({ default: module.AdminDashboardPage })));
const AdminUsersPage = lazy(() => import('@/pages/admin-pages').then((module) => ({ default: module.AdminUsersPage })));
const AdminVehiclesPage = lazy(() => import('@/pages/admin-pages').then((module) => ({ default: module.AdminVehiclesPage })));
const AdminVehicleEditorPage = lazy(() => import('@/pages/admin-pages').then((module) => ({ default: module.AdminVehicleEditorPage })));
const AdminPricingPage = lazy(() => import('@/pages/admin-pages').then((module) => ({ default: module.AdminPricingPage })));

function withSuspense(element: ReactNode) {
  return <Suspense fallback={<div className="surface flex min-h-[45vh] items-center justify-center text-slate-500">Carregando modulo...</div>}>{element}</Suspense>;
}

function RequireAuth() {
  const { isAuthenticated, hydrated } = useAuth();
  const location = useLocation();

  if (!hydrated) {
    return <div className="surface flex min-h-[50vh] items-center justify-center text-slate-500">Carregando sessao...</div>;
  }

  if (!isAuthenticated) {
    return <Navigate to={`/login?redirect=${encodeURIComponent(location.pathname + location.search)}`} replace />;
  }

  return <Outlet />;
}

function RequireAdmin() {
  const { isAdmin } = useAuth();
  if (!isAdmin) {
    return <Navigate to="/app/dashboard" replace />;
  }
  return <Outlet />;
}

export const router = createBrowserRouter([
  {
    element: <PublicLayout />,
    children: [
      { path: '/', element: withSuspense(<HomePage />) },
      { path: '/login', element: withSuspense(<LoginPage />) },
      { path: '/catalogo', element: withSuspense(<CatalogPage />) },
      { path: '/catalogo/:vehicleId', element: withSuspense(<VehicleDetailsPage />) },
    ],
  },
  {
    element: <RequireAuth />,
    children: [
      {
        element: <AppShell />,
        children: [
          { path: '/app/dashboard', element: withSuspense(<UserDashboardPage />) },
          { path: '/app/profile', element: withSuspense(<ProfilePage />) },
          { path: '/app/reservas', element: withSuspense(<ReservationsPage />) },
          { path: '/app/reservas/:reservationId', element: withSuspense(<ReservationDetailsPage />) },
          { path: '/app/reservas/:reservationId/pagamento', element: withSuspense(<PaymentFlowPage />) },
          {
            element: <RequireAdmin />,
            children: [
              { path: '/admin/dashboard', element: withSuspense(<AdminDashboardPage />) },
              { path: '/admin/users', element: withSuspense(<AdminUsersPage />) },
              { path: '/admin/vehicles', element: withSuspense(<AdminVehiclesPage />) },
              { path: '/admin/vehicles/new', element: withSuspense(<AdminVehicleEditorPage />) },
              { path: '/admin/vehicles/:vehicleId', element: withSuspense(<AdminVehicleEditorPage />) },
              { path: '/admin/pricing', element: withSuspense(<AdminPricingPage />) },
            ],
          },
        ],
      },
    ],
  },
  {
    path: '*',
    element: withSuspense(<NotFoundPage />),
  },
]);

export function AppRouter() {
  return <RouterProvider router={router} />;
}

export const appQueryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      staleTime: 1000 * 30,
      refetchOnWindowFocus: false,
    },
  },
});
