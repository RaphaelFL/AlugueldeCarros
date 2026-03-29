import { act, renderHook } from '@testing-library/react';
import { useAuth } from '@/hooks/use-auth';
import { useAuthStore } from '@/store/auth-store';
import type { SessionUser } from '@/types/domain';

function createUser(roles: SessionUser['roles'] = ['Customer']): SessionUser {
  return {
    id: 1,
    email: 'customer@example.com',
    firstName: 'Customer',
    lastName: 'Example',
    createdAt: '2026-03-28T00:00:00Z',
    roles,
  };
}

describe('useAuth', () => {
  beforeEach(() => {
    useAuthStore.setState({
      token: null,
      user: null,
      paymentRegistry: {},
      hydrated: false,
    });
  });

  it('expõe sessão autenticada e privilégio administrativo', () => {
    act(() => {
      useAuthStore.getState().setSession('jwt-token', createUser(['Admin']));
      useAuthStore.getState().markHydrated();
    });

    const { result } = renderHook(() => useAuth());

    expect(result.current.hydrated).toBe(true);
    expect(result.current.isAuthenticated).toBe(true);
    expect(result.current.isAdmin).toBe(true);
    expect(result.current.token).toBe('jwt-token');
  });

  it('não considera autenticado quando faltam dados do usuário', () => {
    act(() => {
      useAuthStore.setState({
        token: 'jwt-token',
        user: null,
        paymentRegistry: {},
        hydrated: true,
      });
    });

    const { result } = renderHook(() => useAuth());

    expect(result.current.isAuthenticated).toBe(false);
    expect(result.current.isAdmin).toBe(false);
  });
});