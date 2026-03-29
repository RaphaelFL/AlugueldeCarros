import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { NotFoundPageView } from '@/features/not-found-page/not-found-page.view';
import { useNotFoundPageLogic } from '@/features/not-found-page/not-found-page.logic';

describe('not-found page', () => {
  it('mantém a rota inicial padronizada no view model', () => {
    expect(useNotFoundPageLogic()).toEqual({ homePath: '/' });
  });

  it('renderiza mensagem e ação de retorno', () => {
    render(
      <MemoryRouter>
        <NotFoundPageView homePath="/" />
      </MemoryRouter>,
    );

    expect(screen.getByText('Pagina nao encontrada')).toBeInTheDocument();
    expect(screen.getByRole('link', { name: /voltar ao inicio/i })).toHaveAttribute('href', '/');
  });
});