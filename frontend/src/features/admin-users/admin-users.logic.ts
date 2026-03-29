import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { adminService } from '@/api/services';

export interface AdminUsersViewModel {
  users: Awaited<ReturnType<typeof adminService.getUsers>>;
  feedback: string | null;
  isError: boolean;
  isPending: boolean;
  isLoading: boolean;
  saveRoles: (userId: number, roles: string[]) => void;
}

export function useAdminUsersLogic(getErrorMessage: (error: unknown) => string): AdminUsersViewModel {
  const queryClient = useQueryClient();
  const { data: users = [], isLoading } = useQuery({ queryKey: ['admin-users'], queryFn: adminService.getUsers });
  const [feedback, setFeedback] = useState<string | null>(null);

  const roleMutation = useMutation({
    mutationFn: ({ userId, roles }: { userId: number; roles: string[] }) => adminService.assignRoles(userId, roles),
    onSuccess: async () => {
      setFeedback('Perfis atualizados com sucesso.');
      await queryClient.invalidateQueries({ queryKey: ['admin-users'] });
    },
    onError: (error) => setFeedback(getErrorMessage(error)),
  });

  return {
    users,
    feedback,
    isError: roleMutation.isError,
    isPending: roleMutation.isPending,
    isLoading,
    saveRoles: (userId, roles) => roleMutation.mutate({ userId, roles }),
  };
}
