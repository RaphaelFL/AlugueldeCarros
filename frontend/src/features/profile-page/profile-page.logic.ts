import { useAuth } from '@/hooks/use-auth';

export interface ProfilePageViewModel {
  user: ReturnType<typeof useAuth>['user'];
  isLoading: boolean;
}

export function useProfilePageLogic(): ProfilePageViewModel {
  const { user } = useAuth();

  return {
    user,
    isLoading: !user,
  };
}