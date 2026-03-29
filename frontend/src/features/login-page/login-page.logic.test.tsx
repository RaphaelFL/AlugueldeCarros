import { act, renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { authSchema, registerSchema, useLoginPageLogic } from '@/features/login-page/login-page.logic';
import { useAuthStore } from '@/store/auth-store';
import { authService } from '@/api/services';
import type { SessionUser } from '@/types/domain';

const navigateMock = jest.fn();
let currentSearchParams = new URLSearchParams();

jest.mock('@/api/services', () => ({
  authService: {
    login: jest.fn(),
    me: jest.fn(),
    register: jest.fn(),
  },
}));

jest.mock('react-router-dom', () => {
  const actual = jest.requireActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => navigateMock,
    useSearchParams: () => [currentSearchParams, jest.fn()],
  };
});

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

function createWrapper() {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });

  return function Wrapper({ children }: Readonly<{ children: React.ReactNode }>) {
    return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>;
  };
}

describe('useLoginPageLogic', () => {
  const mockedAuthService = authService as jest.Mocked<typeof authService>;

  beforeEach(() => {
    currentSearchParams = new URLSearchParams();
    useAuthStore.setState({
      token: null,
      user: null,
      paymentRegistry: {},
      hydrated: false,
    });
  });

  it('normaliza e valida os dados do schema de autenticação', () => {
    expect(authSchema.parse({ email: '  USER@TEST.COM  ', password: '123456' })).toEqual({
      email: 'user@test.com',
      password: '123456',
    });

    expect(registerSchema.parse({
      email: 'new@example.com',
      password: '123456',
      firstName: 'Ana',
      lastName: 'Silva',
    }).firstName).toBe('Ana');
  });

  it('faz login, salva a sessão e navega para o redirect informado', async () => {
    currentSearchParams = new URLSearchParams('redirect=/app/reservas');
    mockedAuthService.login.mockResolvedValue({ token: 'jwt-token', email: 'customer@example.com' });
    mockedAuthService.me.mockResolvedValue(createUser());

    const { result } = renderHook(() => useLoginPageLogic(() => 'erro'), {
      wrapper: createWrapper(),
    });

    act(() => {
      result.current.submitLogin({ email: 'customer@example.com', password: '123456' });
    });

    await waitFor(() => expect(mockedAuthService.login).toHaveBeenCalledTimes(1));
    await waitFor(() => expect(useAuthStore.getState().token).toBe('jwt-token'));
    await waitFor(() => expect(navigateMock).toHaveBeenCalledWith('/app/reservas'));
  });

  it('faz cadastro, salva a sessão e navega para o dashboard padrão', async () => {
    mockedAuthService.register.mockResolvedValue({ token: 'new-token', email: 'new@example.com' });
    mockedAuthService.me.mockResolvedValue(createUser());

    const { result } = renderHook(() => useLoginPageLogic(() => 'erro'), {
      wrapper: createWrapper(),
    });

    act(() => {
      result.current.submitRegister({
        email: 'new@example.com',
        password: '123456',
        firstName: 'Ana',
        lastName: 'Silva',
      });
    });

    await waitFor(() => expect(mockedAuthService.register).toHaveBeenCalledTimes(1));
    await waitFor(() => expect(useAuthStore.getState().token).toBe('new-token'));
    await waitFor(() => expect(navigateMock).toHaveBeenCalledWith('/app/dashboard'));
  });
});