import type { ButtonHTMLAttributes, HTMLAttributes, InputHTMLAttributes, ReactNode, SelectHTMLAttributes } from 'react';
import { AlertTriangle, LoaderCircle } from 'lucide-react';
import { cn } from '@/lib/utils';
import type { PaymentStatus, ReservationStatus, VehicleStatus } from '@/types/domain';

export function Button({ className, ...props }: ButtonHTMLAttributes<HTMLButtonElement>) {
  return (
    <button
      className={cn(
        'inline-flex items-center justify-center rounded-2xl px-4 py-3 text-sm font-semibold transition focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-accent/40 disabled:cursor-not-allowed disabled:opacity-60',
        'bg-ink text-white hover:bg-slate-800',
        className,
      )}
      {...props}
    />
  );
}

export function SecondaryButton({ className, ...props }: ButtonHTMLAttributes<HTMLButtonElement>) {
  return (
    <button
      className={cn(
        'inline-flex items-center justify-center rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm font-semibold text-slate-700 transition hover:border-slate-300 hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-60',
        className,
      )}
      {...props}
    />
  );
}

export function Card({ className, ...props }: HTMLAttributes<HTMLDivElement>) {
  return <div className={cn('surface p-6', className)} {...props} />;
}

export function Input({ className, ...props }: InputHTMLAttributes<HTMLInputElement>) {
  return (
    <input
      className={cn(
        'w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm text-ink shadow-sm outline-none transition placeholder:text-slate-400 focus:border-accent focus:ring-4 focus:ring-accent/10',
        className,
      )}
      {...props}
    />
  );
}

export function Select({ className, ...props }: SelectHTMLAttributes<HTMLSelectElement>) {
  return (
    <select
      className={cn(
        'w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm text-ink shadow-sm outline-none transition focus:border-accent focus:ring-4 focus:ring-accent/10',
        className,
      )}
      {...props}
    />
  );
}

export function Field({ label, error, hint, children }: { label: string; error?: string; hint?: string; children: ReactNode }) {
  return (
    <label className="flex flex-col gap-2 text-sm font-medium text-slate-700">
      <span>{label}</span>
      {children}
      {hint ? <span className="text-xs text-slate-500">{hint}</span> : null}
      {error ? <span className="text-xs text-danger">{error}</span> : null}
    </label>
  );
}

export function SectionHeader({ title, description, action }: { title: string; description: string; action?: ReactNode }) {
  return (
    <div className="flex flex-col gap-4 md:flex-row md:items-end md:justify-between">
      <div>
        <h1 className="font-display text-3xl font-semibold tracking-tight text-ink">{title}</h1>
        <p className="mt-2 max-w-2xl text-sm leading-6 text-slate-600">{description}</p>
      </div>
      {action}
    </div>
  );
}

export function Badge({ children, tone = 'neutral' }: { children: ReactNode; tone?: 'neutral' | 'success' | 'warning' | 'danger' | 'info' }) {
  const toneClasses = {
    neutral: 'bg-slate-100 text-slate-700',
    success: 'bg-emerald-50 text-emerald-700',
    warning: 'bg-amber-50 text-amber-700',
    danger: 'bg-rose-50 text-rose-700',
    info: 'bg-cyan-50 text-cyan-700',
  };

  return <span className={cn('inline-flex rounded-full px-3 py-1 text-xs font-semibold', toneClasses[tone])}>{children}</span>;
}

export function ReservationStatusBadge({ status }: { status: ReservationStatus }) {
  const tone = {
    PENDING_PAYMENT: 'warning',
    CONFIRMED: 'success',
    CANCELLED: 'danger',
    EXPIRED: 'neutral',
  } as const;
  return <Badge tone={tone[status]}>{status}</Badge>;
}

export function PaymentStatusBadge({ status }: { status: PaymentStatus }) {
  const tone = {
    PENDING: 'warning',
    APPROVED: 'success',
    DECLINED: 'danger',
    REFUNDED: 'info',
  } as const;
  return <Badge tone={tone[status]}>{status}</Badge>;
}

export function VehicleStatusBadge({ status }: { status: VehicleStatus }) {
  const tone = {
    AVAILABLE: 'success',
    RESERVED: 'warning',
    RENTED: 'info',
    MAINTENANCE: 'danger',
    BLOCKED: 'neutral',
  } as const;
  return <Badge tone={tone[status]}>{status}</Badge>;
}

export function StatCard({ label, value, caption }: { label: string; value: ReactNode; caption: string }) {
  return (
    <Card className="space-y-3">
      <p className="text-sm font-medium text-slate-500">{label}</p>
      <div className="font-display text-3xl font-semibold tracking-tight text-ink">{value}</div>
      <p className="text-sm text-slate-500">{caption}</p>
    </Card>
  );
}

export function LoadingPanel({ title = 'Carregando dados...' }: { title?: string }) {
  return (
    <Card className="flex min-h-52 items-center justify-center gap-3 text-slate-500">
      <LoaderCircle className="size-5 animate-spin" />
      <span>{title}</span>
    </Card>
  );
}

export function EmptyState({ title, description, action }: { title: string; description: string; action?: ReactNode }) {
  return (
    <Card className="flex min-h-56 flex-col items-center justify-center gap-4 text-center">
      <div className="rounded-full bg-slate-100 p-4 text-slate-500">
        <AlertTriangle className="size-6" />
      </div>
      <div>
        <h3 className="text-lg font-semibold text-ink">{title}</h3>
        <p className="mt-2 max-w-md text-sm leading-6 text-slate-500">{description}</p>
      </div>
      {action}
    </Card>
  );
}

export function InlineMessage({ tone = 'info', children }: { tone?: 'info' | 'error' | 'success'; children: ReactNode }) {
  const toneClasses = {
    info: 'border-cyan-200 bg-cyan-50 text-cyan-800',
    error: 'border-rose-200 bg-rose-50 text-rose-700',
    success: 'border-emerald-200 bg-emerald-50 text-emerald-700',
  };

  return <div className={cn('rounded-2xl border px-4 py-3 text-sm', toneClasses[tone])}>{children}</div>;
}
