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

describe('useAuthStore', () => {
  beforeEach(() => {
    useAuthStore.setState({
      token: null,
      user: null,
      paymentRegistry: {},
      hydrated: false,
    });
  });

  it('setSession e signOut controlam a sessão atual', () => {
    const user = createUser();

    useAuthStore.getState().setSession('token-123', user);

    expect(useAuthStore.getState().token).toBe('token-123');
    expect(useAuthStore.getState().user).toEqual(user);

    useAuthStore.getState().signOut();

    expect(useAuthStore.getState().token).toBeNull();
    expect(useAuthStore.getState().user).toBeNull();
    expect(useAuthStore.getState().paymentRegistry).toEqual({});
  });

  it('rememberPayment e getPaymentId mantêm o registro por reserva', () => {
    useAuthStore.getState().rememberPayment(10, 99);

    expect(useAuthStore.getState().getPaymentId(10)).toBe(99);
    expect(useAuthStore.getState().getPaymentId(11)).toBeNull();
  });

  it('updateUser e markHydrated atualizam o estado derivado', () => {
    const original = createUser();
    const updated = createUser(['Customer', 'Admin']);

    useAuthStore.getState().setSession('token-123', original);
    useAuthStore.getState().updateUser(updated);
    useAuthStore.getState().markHydrated();

    expect(useAuthStore.getState().user?.roles).toEqual(['Customer', 'Admin']);
    expect(useAuthStore.getState().hydrated).toBe(true);
  });
});