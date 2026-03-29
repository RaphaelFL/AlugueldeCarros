import { Link } from 'react-router-dom';
import { Button, EmptyState } from '@/components/ui';
import type { NotFoundPageViewModel } from './not-found-page.logic';
import './not-found-page.css';

export function NotFoundPageView({ homePath }: Readonly<NotFoundPageViewModel>) {
  return (
    <div className="not-found-page-shell mx-auto flex min-h-screen max-w-3xl items-center justify-center px-6 py-10">
      <EmptyState
        title="Pagina nao encontrada"
        description="A rota solicitada nao existe nesta SPA. Use o menu principal para voltar a uma area valida do produto."
        action={<Link to={homePath}><Button>Voltar ao inicio</Button></Link>}
      />
    </div>
  );
}