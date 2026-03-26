import { useEffect } from 'react';
import { QueryClientProvider } from '@tanstack/react-query';
import { RouterProvider, createBrowserRouter } from 'react-router-dom';
import { authService } from '@/api/services';
import { isTokenExpired } from '@/lib/jwt';
import { useAuthStore } from '@/store/auth-store';
import { appQueryClient } from '@/routes/router';

export function AppProviders({ router }: { router: ReturnType<typeof createBrowserRouter> }) {
  const token = useAuthStore((state) => state.token);
  const hydrated = useAuthStore((state) => state.hydrated);
  const setSession = useAuthStore((state) => state.setSession);
  const signOut = useAuthStore((state) => state.signOut);

  useEffect(() => {
    if (!hydrated || !token) return;

    const bootstrap = async () => {
      try {
        const usableToken = isTokenExpired(token, 600) ? (await authService.refresh(token)).token : token;
        const user = await authService.me(usableToken);
        setSession(usableToken, user);
      } catch {
        signOut();
      }
    };

    void bootstrap();
  }, [hydrated, token, setSession, signOut]);

  return (
    <QueryClientProvider client={appQueryClient}>
      <RouterProvider router={router} />
    </QueryClientProvider>
  );
}
