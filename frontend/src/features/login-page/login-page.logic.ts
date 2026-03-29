import { useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import { z } from 'zod';
import { useForm, type UseFormReturn } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { authService } from '@/api/services';
import { useAuth } from '@/hooks/use-auth';
import { useAuthStore } from '@/store/auth-store';

export const authSchema = z.object({
  email: z.string().trim().email('Informe um e-mail valido.').max(120, 'O e-mail deve ter no maximo 120 caracteres.').transform((value) => value.toLowerCase()),
  password: z.string().min(6, 'A senha precisa ter ao menos 6 caracteres.').max(128, 'A senha deve ter no maximo 128 caracteres.'),
});

export const registerSchema = authSchema.extend({
  firstName: z.string().trim().min(2, 'Informe o nome.').max(60, 'O nome deve ter no maximo 60 caracteres.'),
  lastName: z.string().trim().min(2, 'Informe o sobrenome.').max(60, 'O sobrenome deve ter no maximo 60 caracteres.'),
});

export type LoginFormValues = z.infer<typeof authSchema>;
export type RegisterFormValues = z.infer<typeof registerSchema>;

export interface LoginPageViewModel {
  isAuthenticated: boolean;
  isAdmin: boolean;
  redirect: string | null;
  mode: 'login' | 'register';
  feedback: string | null;
  loginForm: UseFormReturn<LoginFormValues>;
  registerForm: UseFormReturn<RegisterFormValues>;
  isLoginPending: boolean;
  isRegisterPending: boolean;
  setMode: (mode: 'login' | 'register') => void;
  submitLogin: (values: LoginFormValues) => void;
  submitRegister: (values: RegisterFormValues) => void;
}

export function useLoginPageLogic(getErrorMessage: (error: unknown) => string): LoginPageViewModel {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const redirect = searchParams.get('redirect');
  const { isAuthenticated, isAdmin } = useAuth();
  const setSession = useAuthStore((state) => state.setSession);
  const [mode, setMode] = useState<'login' | 'register'>('login');
  const [feedback, setFeedback] = useState<string | null>(null);

  const loginForm = useForm<LoginFormValues>({
    resolver: zodResolver(authSchema),
    defaultValues: { email: 'customer@example.com', password: '123456' },
  });

  const registerForm = useForm<RegisterFormValues>({
    resolver: zodResolver(registerSchema),
    defaultValues: { email: '', password: '', firstName: '', lastName: '' },
  });

  const loginMutation = useMutation({
    mutationFn: async (values: LoginFormValues) => {
      const auth = await authService.login(values);
      const user = await authService.me(auth.token);
      setSession(auth.token, user);
      return user;
    },
    onSuccess: (user) => {
      navigate(redirect || (user.roles.includes('Admin') ? '/admin/dashboard' : '/app/dashboard'));
    },
    onError: (error) => setFeedback(getErrorMessage(error)),
  });

  const registerMutation = useMutation({
    mutationFn: async (values: RegisterFormValues) => {
      const auth = await authService.register(values);
      const user = await authService.me(auth.token);
      setSession(auth.token, user);
      return user;
    },
    onSuccess: () => navigate('/app/dashboard'),
    onError: (error) => setFeedback(getErrorMessage(error)),
  });

  return {
    isAuthenticated,
    isAdmin,
    redirect,
    mode,
    feedback,
    loginForm,
    registerForm,
    isLoginPending: loginMutation.isPending,
    isRegisterPending: registerMutation.isPending,
    setMode,
    submitLogin: (values) => loginMutation.mutate(values),
    submitRegister: (values) => registerMutation.mutate(values),
  };
}
