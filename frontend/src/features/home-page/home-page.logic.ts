import { useQuery } from '@tanstack/react-query';
import { CalendarClock, CheckCircle2, Compass, ShieldCheck, Sparkles, WalletCards, type LucideIcon } from 'lucide-react';
import { catalogService } from '@/api/services';

export interface HomeHighlight {
  title: string;
  copy: string;
  icon: LucideIcon;
}

export interface HomePageViewModel {
  branchesCount: number;
  categoriesCount: number;
  primaryBranchName: string;
  highlights: HomeHighlight[];
}

export function useHomePageLogic(): HomePageViewModel {
  const { data: branches = [] } = useQuery({ queryKey: ['branches'], queryFn: catalogService.getBranches });
  const { data: categories = [] } = useQuery({ queryKey: ['categories'], queryFn: catalogService.getCategories });

  return {
    branchesCount: branches.length,
    categoriesCount: categories.length,
    primaryBranchName: branches[0]?.name ?? 'uma filial principal',
    highlights: [
      {
        title: 'Busca com contexto operacional',
        copy: 'O cliente encontra categorias e veiculos com filtros reais de filial, periodo e faixa de diaria, sem promessas fora do backend atual.',
        icon: Compass,
      },
      {
        title: 'Fluxo de reserva com pagamento conectado',
        copy: 'A jornada segue a regra do sistema: disponibilidade, criacao da reserva, pre-autorizacao e captura.',
        icon: WalletCards,
      },
      {
        title: 'Operacao administrativa integrada',
        copy: 'Admins operam usuarios, frota e pricing rules na mesma SPA, respeitando JWT e role-based access.',
        icon: ShieldCheck,
      },
    ],
  };
}

export const homePageCapabilities = [
  {
    title: 'Base atual de atendimento',
    copy: (primaryBranchName: string) => `A operacao carrega hoje ${primaryBranchName}, pronta para buscas publicas e atendimento autenticado.`,
    icon: CheckCircle2,
  },
  {
    title: 'Estados claros de jornada',
    copy: () => 'Reservas e pagamentos exibem status reais do backend para orientar proxima acao sem ambiguidade.',
    icon: CalendarClock,
  },
  {
    title: 'RBAC aplicado no produto',
    copy: () => 'Menus, rotas e acoes mudam conforme o papel da sessao, separando Customer e Admin de forma objetiva.',
    icon: Sparkles,
  },
] as const;