import { LoadingPanel } from '@/components/ui';
import { CatalogPageView } from '@/features/catalog-page/catalog-page.view';
import { useCatalogPageLogic } from '@/features/catalog-page/catalog-page.logic';
import { HomePageView } from '@/features/home-page/home-page.view';
import { useHomePageLogic } from '@/features/home-page/home-page.logic';
import { useLoginPageLogic } from '@/features/login-page/login-page.logic';
import { LoginPageView } from '@/features/login-page/login-page.view';
import { useNotFoundPageLogic } from '@/features/not-found-page/not-found-page.logic';
import { NotFoundPageView } from '@/features/not-found-page/not-found-page.view';
import { useVehicleDetailsPageLogic } from '@/features/vehicle-details-page/vehicle-details-page.logic';
import { VehicleDetailsPageView } from '@/features/vehicle-details-page/vehicle-details-page.view';
import { getErrorMessage } from '@/lib/utils';

export function HomePage() {
  const viewModel = useHomePageLogic();

  return <HomePageView {...viewModel} />;
}

export function LoginPage() {
  const viewModel = useLoginPageLogic(getErrorMessage);

  return <LoginPageView {...viewModel} />;
}

export function CatalogPage() {
  const viewModel = useCatalogPageLogic();

  return <CatalogPageView {...viewModel} />;
}

export function VehicleDetailsPage() {
  const viewModel = useVehicleDetailsPageLogic(getErrorMessage);

  if (viewModel.isLoading) {
    return <div className="mx-auto max-w-7xl px-6 py-10 lg:px-8"><LoadingPanel title="Carregando detalhe do veiculo..." /></div>;
  }

  if (!viewModel.vehicle) {
    return <NotFoundPage />;
  }

  return <VehicleDetailsPageView {...viewModel} />;
}

export function NotFoundPage() {
  const viewModel = useNotFoundPageLogic();

  return <NotFoundPageView {...viewModel} />;
}
