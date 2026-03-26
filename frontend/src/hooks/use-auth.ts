import { useMemo } from 'react';
import { useAuthStore } from '@/store/auth-store';

export function useAuth() {
  const token = useAuthStore((state) => state.token);
  const user = useAuthStore((state) => state.user);
  const hydrated = useAuthStore((state) => state.hydrated);

  return useMemo(
    () => ({
      token,
      user,
      hydrated,
      isAuthenticated: Boolean(token && user),
      isAdmin: Boolean(user?.roles.includes('Admin')),
    }),
    [token, user, hydrated],
  );
}
