import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useForm, type UseFormReturn } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { adminService, catalogService } from '@/api/services';
import type { PricingRulePayload } from '@/types/domain';

export const pricingSchema = z.object({
  categoryId: z.coerce.number().positive('Selecione a categoria.'),
  baseDailyRate: z.coerce.number().positive('Informe a diaria base.'),
  weekendMultiplier: z.coerce.number().positive('Informe o multiplicador de fim de semana.'),
  peakSeasonMultiplier: z.coerce.number().positive('Informe o multiplicador de alta temporada.'),
});

export type AdminPricingFormValues = z.infer<typeof pricingSchema>;

export interface AdminPricingViewModel {
  rules: Awaited<ReturnType<typeof adminService.getPricingRules>>;
  categories: Array<{ id: number; name: string }>;
  editingRuleId: number | null;
  form: UseFormReturn<AdminPricingFormValues>;
  isLoading: boolean;
  isPending: boolean;
  isError: boolean;
  errorMessage: string | null;
  submitLabel: string;
  setEditingRuleId: (id: number | null) => void;
  onSubmit: (values: AdminPricingFormValues) => void;
}

export function useAdminPricingLogic(getErrorMessage: (error: unknown) => string): AdminPricingViewModel {
  const queryClient = useQueryClient();
  const { data: rules = [], isLoading } = useQuery({ queryKey: ['pricing-rules'], queryFn: adminService.getPricingRules });
  const { data: categories = [] } = useQuery({ queryKey: ['categories'], queryFn: catalogService.getCategories });
  const [editingRuleId, setEditingRuleId] = useState<number | null>(null);

  const editingRule = rules.find((rule) => rule.id === editingRuleId);
  const form = useForm<AdminPricingFormValues>({
    resolver: zodResolver(pricingSchema),
    values: editingRule
      ? {
          categoryId: editingRule.categoryId,
          baseDailyRate: editingRule.baseDailyRate,
          weekendMultiplier: editingRule.weekendMultiplier,
          peakSeasonMultiplier: editingRule.peakSeasonMultiplier,
        }
      : {
          categoryId: categories[0]?.id ?? 0,
          baseDailyRate: 0,
          weekendMultiplier: 1,
          peakSeasonMultiplier: 1,
        },
  });

  const mutation = useMutation({
    mutationFn: async (payload: PricingRulePayload) => {
      return editingRuleId ? adminService.updatePricingRule(editingRuleId, payload) : adminService.createPricingRule(payload);
    },
    onSuccess: async () => {
      setEditingRuleId(null);
      form.reset({ categoryId: categories[0]?.id ?? 0, baseDailyRate: 0, weekendMultiplier: 1, peakSeasonMultiplier: 1 });
      await queryClient.invalidateQueries({ queryKey: ['pricing-rules'] });
    },
  });

  return {
    rules,
    categories: categories.map((category) => ({ id: category.id, name: category.name })),
    editingRuleId,
    form,
    isLoading,
    isPending: mutation.isPending,
    isError: mutation.isError,
    errorMessage: mutation.isError ? getErrorMessage(mutation.error) : null,
    submitLabel: mutation.isPending ? 'Salvando...' : editingRuleId ? 'Atualizar regra' : 'Criar regra',
    setEditingRuleId,
    onSubmit: (values) => mutation.mutate(values),
  };
}
