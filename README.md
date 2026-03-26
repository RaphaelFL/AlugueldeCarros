# Aluguel de Carros

Plataforma de aluguel de veículos com backend ASP.NET Core e front-end React oficial na pasta frontend.

Estado atual do projeto:

- API REST em .NET 8.
- SPA React em TypeScript com Vite.
- autenticação JWT Bearer.
- RBAC com Customer e Admin.
- dados mockados em JSON com operação principal em memória.

## Visão Geral

O produto cobre hoje:

- autenticação e renovação de sessão.
- catálogo público de veículos.
- criação de reserva por categoria.
- fluxo de pagamento com preauth, capture e refund.
- área autenticada do cliente.
- área administrativa para usuários, veículos e pricing rules.

Limites importantes:

- não existe banco relacional.
- não existe listagem completa de pagamentos por reserva ou por usuário.
- não existe módulo de check-in, check-out, promoções ou relatórios complexos.

## Estrutura do Repositório

```text
.
├── .ai/                       # contexto operacional do projeto
├── AlugueldeCarros/           # backend ASP.NET Core Web API
├── AlugueldeCarros.Tests/     # testes do backend
├── frontend/                  # SPA React com Vite
└── README.md
```

## Como Rodar

### Pré-requisitos

- .NET 8 SDK
- Node.js 20+
- npm 10+

### Backend

```bash
cd AlugueldeCarros
dotnet restore
dotnet build
dotnet run
```

Backend local:

- http://localhost:5097
- https://localhost:7110
- Swagger: https://localhost:7110/swagger

Observação de execução no Visual Studio:

- o perfil principal para abrir Swagger é o https.
- os perfis http e IIS Express não abrem navegador automaticamente.

### Front-end

```bash
cd frontend
npm install
npm run dev
```

Front local:

- http://localhost:5173

### Build

```bash
cd AlugueldeCarros
dotnet build

cd ../frontend
npm run build
```

## API

### Autenticação

- esquema: JWT Bearer
- usar o botão Authorize do Swagger com o formato `Bearer TOKEN`
- roles reais: Customer e Admin

Credenciais de teste:

- customer@example.com / 123456
- admin@aluguel.com / admin123

### Fluxo principal da API

1. autenticar em `POST /api/v1/auth/login` ou registrar em `POST /api/v1/auth/register`
2. consultar catálogo em `GET /api/v1/vehicles/search`
3. criar reserva por categoria em `POST /api/v1/reservations`
4. executar preauth em `POST /api/v1/payments/preauth`
5. capturar em `POST /api/v1/payments/capture`

Regras importantes:

- reserva é criada por categoria, não por veículo escolhido pelo cliente.
- reservas começam em `PENDING_PAYMENT`.
- captura aprovada pode confirmar a reserva e marcar o veículo como `RESERVED`.
- ownership de reservas e pagamentos é validado no fluxo HTTP.

### Endpoints reais

#### Auth

- POST /api/v1/auth/register
- POST /api/v1/auth/login
- POST /api/v1/auth/refresh

#### Usuário

- GET /api/v1/users/me
- GET /api/v1/users/me/reservations
- GET /api/v1/admin/users
- POST /api/v1/admin/users/{id}/roles

#### Catálogo e frota

- GET /api/v1/branches
- GET /api/v1/vehicles/categories
- GET /api/v1/vehicles/search
- GET /api/v1/vehicles/{id}
- POST /api/v1/admin/vehicles
- PATCH /api/v1/admin/vehicles/{id}

#### Pricing

- GET /api/v1/pricing/rules
- GET /api/v1/pricing/rules/{id}
- POST /api/v1/pricing/rules
- PATCH /api/v1/pricing/rules/{id}

#### Reservas

- POST /api/v1/reservations
- GET /api/v1/reservations/{id}
- PATCH /api/v1/reservations/{id}
- POST /api/v1/reservations/{id}/cancel

#### Pagamentos

- POST /api/v1/payments/preauth
- POST /api/v1/payments/capture
- POST /api/v1/payments/refund
- GET /api/v1/payments/{id}

## Front-end Oficial

Áreas disponíveis:

Públicas:

- /
- /login
- /catalogo
- /catalogo/:vehicleId

Customer:

- /app/dashboard
- /app/profile
- /app/reservas
- /app/reservas/:reservationId
- /app/reservas/:reservationId/pagamento

Admin:

- /admin/dashboard
- /admin/users
- /admin/vehicles
- /admin/vehicles/new
- /admin/vehicles/:vehicleId
- /admin/pricing

Estratégia de integração:

- Axios centralizado em `frontend/src/api/http.ts`
- TanStack Query para queries e mutations
- proxy do Vite para `/api`, apontando por padrão para `https://localhost:7110`

Variáveis de ambiente de referência em [frontend/.env.example](frontend/.env.example):

- `VITE_API_BASE_URL`
- `VITE_PROXY_TARGET`

## Stack

### Backend

- .NET 8
- ASP.NET Core Web API
- Microsoft.AspNetCore.Authentication.JwtBearer 8.0.25
- Swashbuckle.AspNetCore 6.6.2
- System.IdentityModel.Tokens.Jwt 8.17.0
- BCrypt.Net-Next 4.1.0

### Front-end

- React 19
- TypeScript 5
- Vite 6
- React Router 7
- TanStack Query 5
- Zustand 5
- React Hook Form 7
- Zod 3
- Tailwind CSS 3
- Axios 1

## Dados Mockados

Arquivos em `AlugueldeCarros/Resources/MockData`:

- users.json
- roles.json
- user-roles.json
- branches.json
- vehicle-categories.json
- vehicles.json
- pricing-rules.json
- reservations.json
- payments.json

Observações:

- a maior parte das mudanças permanece em memória durante a execução.
- users.json é persistido pelo repositório de usuários.

## Testes

Projeto de testes:

- `AlugueldeCarros.Tests/AlugueldeCarros.Tests.csproj`

Executar todos os testes:

```bash
dotnet test AlugueldeCarros.Tests/AlugueldeCarros.Tests.csproj
```

Stack de testes do backend:

- xUnit
- FluentAssertions
- Moq
- Microsoft.AspNetCore.Mvc.Testing

## Troubleshooting

| Problema | Solução |
|----------|---------|
| Erro 401 no Swagger | Faça login primeiro e use `Bearer TOKEN` em Authorize |
| 403 Forbidden | Verifique se o usuário tem a role necessária ou ownership do recurso |
| Build falha | Verifique se o .NET 8 SDK está instalado e se não há o executável bloqueado por uma instância em execução |
| Dados não carregam | Confirme a existência de `Resources/MockData/*.json` |
| Token expirado | Faça login novamente ou use `POST /api/v1/auth/refresh` |
| Front recebe 500 ao chamar `/api/*` | Verifique se o backend está rodando em `https://localhost:7110` ou ajuste `VITE_PROXY_TARGET` |
| Front não alcança a API | Verifique `VITE_PROXY_TARGET` ou `VITE_API_BASE_URL` |

## Documentação de Contexto

Para reconstrução fiel do sistema, consulte também:

- [.ai/architecture.md](.ai/architecture.md)
- [.ai/business-rules.md](.ai/business-rules.md)
- [.ai/standards.md](.ai/standards.md)
- [.ai/tech-stack.md](.ai/tech-stack.md)
