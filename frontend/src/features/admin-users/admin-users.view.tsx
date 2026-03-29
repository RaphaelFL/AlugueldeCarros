import { Badge, Button, Card, InlineMessage, LoadingPanel, SectionHeader } from '@/components/ui';
import { formatDateTime } from '@/lib/utils';
import type { AdminUsersViewModel } from './admin-users.logic';
import './admin-users.css';

export function AdminUsersView({ users, feedback, isError, isPending, isLoading, saveRoles }: Readonly<AdminUsersViewModel>) {
  if (isLoading) return <LoadingPanel title="Carregando usuarios..." />;

  return (
    <div className="space-y-8">
      <SectionHeader title="Usuarios e perfis" description="Atribuicao de roles usando o endpoint administrativo real do backend." />
      {feedback ? <InlineMessage tone={isError ? 'error' : 'success'}>{feedback}</InlineMessage> : null}
      <div className="space-y-4">
        {users.map((user) => {
          const selectedRoles = new Set(user.roles);
          return (
            <Card key={user.id} className="space-y-4">
              <div className="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
                <div>
                  <p className="font-display text-2xl font-semibold text-ink">{user.firstName} {user.lastName}</p>
                  <p className="text-sm text-slate-500">{user.email} • criado em {formatDateTime(user.createdAt)}</p>
                </div>
                <div className="flex gap-2">
                  {user.roles.map((role) => <Badge key={role} tone={role === 'Admin' ? 'info' : 'neutral'}>{role}</Badge>)}
                </div>
              </div>
              <div className="admin-users-role-grid flex flex-wrap items-center gap-6">
                {['Customer', 'Admin'].map((role) => (
                  <label key={role} className="flex items-center gap-2 text-sm font-medium text-slate-700">
                    <input
                      type="checkbox"
                      defaultChecked={selectedRoles.has(role as never)}
                      onChange={(event) => {
                        if (event.target.checked) {
                          selectedRoles.add(role as never);
                        } else {
                          selectedRoles.delete(role as never);
                        }
                      }}
                    />
                    {role}
                  </label>
                ))}
                <Button onClick={() => saveRoles(user.id, Array.from(selectedRoles))} disabled={isPending}>
                  Salvar roles
                </Button>
              </div>
            </Card>
          );
        })}
      </div>
    </div>
  );
}
