import { useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useSearchParams } from 'react-router-dom';
import { catalogService } from '@/api/services';

export interface CatalogDraftFilters {
  branchId: string;
  categoryId: string;
  from: string;
  to: string;
  priceMin: string;
  priceMax: string;
}

export interface CatalogPageViewModel {
  draft: CatalogDraftFilters;
  setDraft: React.Dispatch<React.SetStateAction<CatalogDraftFilters>>;
  branches: Awaited<ReturnType<typeof catalogService.getBranches>>;
  categories: Awaited<ReturnType<typeof catalogService.getCategories>>;
  vehicles: Awaited<ReturnType<typeof catalogService.searchVehicles>>;
  isLoading: boolean;
  handleSearch: () => void;
  resolveCategoryName: (categoryId: number, fallbackName?: string) => string;
  resolveBranchLabel: (branchId: number, fallbackName?: string, fallbackAddress?: string) => string;
}

export function useCatalogPageLogic(): CatalogPageViewModel {
  const [searchParams, setSearchParams] = useSearchParams();
  const [draft, setDraft] = useState<CatalogDraftFilters>({
    branchId: searchParams.get('branchId') || '',
    categoryId: searchParams.get('categoryId') || '',
    from: searchParams.get('from') || '',
    to: searchParams.get('to') || '',
    priceMin: searchParams.get('priceMin') || '',
    priceMax: searchParams.get('priceMax') || '',
  });

  const filters = useMemo(() => ({
    branchId: draft.branchId ? Number(draft.branchId) : undefined,
    categoryId: draft.categoryId ? Number(draft.categoryId) : undefined,
    from: draft.from || undefined,
    to: draft.to || undefined,
    priceMin: draft.priceMin ? Number(draft.priceMin) : undefined,
    priceMax: draft.priceMax ? Number(draft.priceMax) : undefined,
  }), [draft]);

  const { data: branches = [] } = useQuery({ queryKey: ['branches'], queryFn: catalogService.getBranches });
  const { data: categories = [] } = useQuery({ queryKey: ['categories'], queryFn: catalogService.getCategories });
  const vehiclesQuery = useQuery({ queryKey: ['vehicles', filters], queryFn: () => catalogService.searchVehicles(filters) });

  const categoryMap = new Map(categories.map((category) => [category.id, category]));
  const branchMap = new Map(branches.map((branch) => [branch.id, branch]));

  return {
    draft,
    setDraft,
    branches,
    categories,
    vehicles: vehiclesQuery.data ?? [],
    isLoading: vehiclesQuery.isLoading,
    handleSearch: () => {
      const params = new URLSearchParams();
      Object.entries(draft).forEach(([key, value]) => {
        if (value) params.set(key, value);
      });
      setSearchParams(params);
      void vehiclesQuery.refetch();
    },
    resolveCategoryName: (categoryId, fallbackName) => fallbackName ?? categoryMap.get(categoryId)?.name ?? 'Categoria',
    resolveBranchLabel: (branchId, fallbackName, fallbackAddress) => {
      const branch = branchMap.get(branchId);
      return `${fallbackName ?? branch?.name ?? 'Filial nao informada'} • ${fallbackAddress ?? branch?.address ?? 'Endereco indisponivel'}`;
    },
  };
}