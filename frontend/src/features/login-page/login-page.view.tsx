import { Navigate } from 'react-router-dom';
import { Button, Card, Field, InlineMessage, Input } from '@/components/ui';
import type { LoginPageViewModel } from './login-page.logic';
import './login-page.css';

export function LoginPageView({
  isAuthenticated,
  isAdmin,
  mode,
  feedback,
  loginForm,
  registerForm,
  isLoginPending,
  isRegisterPending,
  setMode,
  submitLogin,
  submitRegister,
}: Readonly<LoginPageViewModel>) {
  if (isAuthenticated) {
    return <Navigate to={isAdmin ? '/admin/dashboard' : '/app/dashboard'} replace />;
  }

  return (
    <div className="login-page-shell mx-auto grid min-h-[calc(100vh-88px)] max-w-7xl gap-8 px-6 py-10 lg:grid-cols-[0.95fr_1.05fr] lg:px-8">
      <Card className="login-page-hero bg-hero-glow p-8 md:p-10">
        <p className="login-page-kicker text-sm uppercase tracking-[0.32em] text-accent">Autenticacao oficial</p>
        <h1 className="mt-4 font-display text-4xl font-semibold tracking-tight text-ink">Acesse a jornada certa para cliente ou operacao admin.</h1>
        <p className="mt-4 text-sm leading-7 text-slate-600">
          A sessao do front reflete o comportamento real da API: JWT no cliente, refresh quando suportado, rotas protegidas e menus guiados por roles.
        </p>
        <div className="login-page-credentials mt-8 space-y-4 text-sm text-slate-600">
          <div className="surface-muted p-4">
            <p className="font-semibold text-ink">Customer de teste</p>
            <p>customer@example.com</p>
            <p>123456</p>
          </div>
          <div className="surface-muted p-4">
            <p className="font-semibold text-ink">Admin de teste</p>
            <p>admin@aluguel.com</p>
            <p>admin123</p>
          </div>
        </div>
      </Card>

      <Card className="login-page-panel p-8 md:p-10">
        <div className="login-page-mode-switch inline-flex rounded-full bg-slate-100 p-1">
          <button
            onClick={() => setMode('login')}
            className={`rounded-full px-4 py-2 text-sm font-semibold ${mode === 'login' ? 'bg-white text-ink shadow-soft' : 'text-slate-500'}`}
          >
            Login
          </button>
          <button
            onClick={() => setMode('register')}
            className={`rounded-full px-4 py-2 text-sm font-semibold ${mode === 'register' ? 'bg-white text-ink shadow-soft' : 'text-slate-500'}`}
          >
            Cadastro
          </button>
        </div>

        {feedback ? <div className="login-page-feedback mt-6"><InlineMessage tone="error">{feedback}</InlineMessage></div> : null}

        {mode === 'login' ? (
          <form className="mt-6 space-y-5" onSubmit={loginForm.handleSubmit(submitLogin)}>
            <Field label="E-mail" error={loginForm.formState.errors.email?.message}>
              <Input type="email" autoComplete="email" maxLength={120} {...loginForm.register('email')} />
            </Field>
            <Field label="Senha" error={loginForm.formState.errors.password?.message}>
              <Input type="password" autoComplete="current-password" maxLength={128} {...loginForm.register('password')} />
            </Field>
            <Button type="submit" className="w-full" disabled={isLoginPending}>
              {isLoginPending ? 'Autenticando...' : 'Entrar'}
            </Button>
          </form>
        ) : (
          <form className="mt-6 grid gap-5 md:grid-cols-2" onSubmit={registerForm.handleSubmit(submitRegister)}>
            <Field label="Nome" error={registerForm.formState.errors.firstName?.message}>
              <Input autoComplete="given-name" maxLength={60} {...registerForm.register('firstName')} />
            </Field>
            <Field label="Sobrenome" error={registerForm.formState.errors.lastName?.message}>
              <Input autoComplete="family-name" maxLength={60} {...registerForm.register('lastName')} />
            </Field>
            <div className="md:col-span-2">
              <Field label="E-mail" error={registerForm.formState.errors.email?.message}>
                <Input type="email" autoComplete="email" maxLength={120} {...registerForm.register('email')} />
              </Field>
            </div>
            <div className="md:col-span-2">
              <Field label="Senha" error={registerForm.formState.errors.password?.message} hint="A API aceita no minimo 6 caracteres no fluxo atual.">
                <Input type="password" autoComplete="new-password" maxLength={128} {...registerForm.register('password')} />
              </Field>
            </div>
            <div className="md:col-span-2">
              <Button type="submit" className="w-full" disabled={isRegisterPending}>
                {isRegisterPending ? 'Criando conta...' : 'Criar conta e iniciar sessao'}
              </Button>
            </div>
          </form>
        )}
      </Card>
    </div>
  );
}
