# ARCHITECTURE

Este documento descreve a arquitetura que existe hoje no projeto. Ele deve servir como referência para reconstruir a API com o mesmo comportamento, não como backlog de melhorias futuras.

## 1. Visão Geral

Arquitetura em camadas simples:

1. Controllers recebem HTTP, extraem identidade do JWT e retornam status code.
2. Services concentram regras de negócio e orquestram repositórios.
3. Repositories mantêm os dados em memória.
4. JsonDataLoader carrega os arquivos de Resources/MockData no startup.

Fluxo padrão:

Cliente HTTP -> middleware ASP.NET Core -> controller -> service -> repository in-memory -> resposta JSON.

## 2. Stack Arquitetural Real

- ASP.NET Core Web API em .NET 8.
- JWT Bearer para autenticação.
- Autorização por role com Customer e Admin.
- Swagger habilitado em desenvolvimento.
- ExceptionHandlingMiddleware como middleware global de erro.
- Persistência apenas em memória, abastecida por JSON.

Nao existe hoje:

- Entity Framework.
- DbContext.
- Banco relacional.
- mensageria.
- cache distribuido.

## 3. Bootstrap da Aplicação

Program.cs faz o seguinte:

1. Registra controllers e Swagger.
2. Lê JwtSettings de configuração.
3. Configura AddAuthentication().AddJwtBearer().
4. Configura AddAuthorization().
5. Registra os repositórios in-memory como singleton.
6. Registra os services como scoped.
7. Registra JwtTokenService e JsonDataLoader como singleton.
8. Executa JsonDataLoader.LoadAllDataAsync() antes de começar a atender requisições.
9. Liga HTTPS redirection, middleware de exceção, autenticação, autorização e mapeamento de controllers.

## 4. Organização por Camada

### 4.1 Controllers

Responsabilidade:

- definir rotas em api/v1.
- aplicar Authorize e Authorize(Roles = "Admin").
- ler ClaimTypes.NameIdentifier do usuário autenticado.
- fazer checagem de ownership quando necessário.
- devolver IActionResult.

Nao devem:

- implementar regra de negócio complexa.
- acessar diretamente arquivos JSON.
- recalcular preço, disponibilidade ou refund.

Controllers existentes:

- AuthController
- UserController
- VehiclesController
- BranchesController
- ReservationsController
- PaymentsController
- PricingController
- AdminUsersController
- AdminVehiclesController

### 4.2 Services

Responsabilidade:

- validar regras de negócio.
- escolher veículo disponível.
- calcular total da reserva.
- controlar transição de status de reserva, pagamento e veículo.
- criar exceções que serão traduzidas pelo middleware.

Services existentes:

- AuthService
- UserService
- VehicleService
- VehicleCategoryService
- BranchService
- ReservationService
- PaymentService
- PricingService

Observação importante: os controllers recebem classes concretas de service, não interfaces.

### 4.3 Repositories

Responsabilidade:

- manter coleções List<T> em memória.
- buscar, adicionar, atualizar e remover entidades.
- gerar IDs inteiros incrementais em AddAsync na maioria das implementações.

Padrão atual:

- interface e implementação ficam no mesmo arquivo em vários casos.
- os dados são carregados uma vez no startup e depois vivem em memória.
- não há persistência de volta para JSON após mutações.

## 5. Modelo de Dados Operacional

Entidades principais do domínio:

- User
- Role
- UserRole
- CustomerProfile
- Branch
- VehicleCategory
- Vehicle
- Reservation
- Payment
- PricingRule

Enums relevantes:

- ReservationStatus: PENDING_PAYMENT, CONFIRMED, CANCELLED, EXPIRED
- PaymentStatus: PENDING, APPROVED, DECLINED, REFUNDED
- VehicleStatus: AVAILABLE, RESERVED, RENTED, MAINTENANCE, BLOCKED

Os IDs usados pelo código atual são inteiros, não GUIDs.

## 6. Autenticação e Autorização

JWT:

- o token é emitido por JwtTokenService.
- usa HmacSha256.
- inclui ClaimTypes.NameIdentifier, ClaimTypes.Email, ClaimTypes.Name e ClaimTypes.Role.
- issuer e audience vêm de JwtSettings.

RBAC:

- rotas administrativas usam role Admin.
- rotas autenticadas normais aceitam Customer e Admin.
- ownership de reservas e pagamentos é validado manualmente em controllers, comparando o userId do token com o userId do recurso.

Papéis reais usados hoje:

- Customer
- Admin

## 7. Comportamentos Importantes por Módulo

### Auth

- registro cria usuário com role Customer por padrão.
- senha nova é hasheada com BCrypt.
- login aceita hash BCrypt e também fallback para comparação em texto puro, por compatibilidade com alguns dados atuais.
- refresh lê o token recebido, extrai o userId e gera um novo token.

### Vehicles

- busca pública filtra por categoryId opcional.
- só considera veículos com status AVAILABLE.
- se houver período, remove veículos com reserva sobreposta em status CONFIRMED ou PENDING_PAYMENT.

### Reservations

- criação exige usuário autenticado.
- valida fim maior que início.
- limita a 5 reservas ativas por usuário.
- seleciona o primeiro veículo disponível retornado pela busca.
- cria reserva com status PENDING_PAYMENT.
- calcula TotalAmount usando DailyRate do veículo, não PricingRule.

### Payments

- preauth cria pagamento com status PENDING.
- capture aprova ou recusa de forma determinística com base em paymentId % 10.
- quando aprovado, a reserva vira CONFIRMED e o veículo vira RESERVED.
- refund só aceita pagamento APPROVED e troca o status para REFUNDED.

## 8. Tratamento de Erros

- ExceptionHandlingMiddleware centraliza a tradução das exceções para HTTP.
- controllers também retornam Unauthorized, Forbid e NotFound em checagens simples.
- services usam ValidationException, KeyNotFoundException, InvalidOperationException e UnauthorizedAccessException conforme o caso.

## 9. Limites do Estado Atual

Para reconstruir a API corretamente, preserve estes fatos:

- o projeto é um mock funcional, não um sistema com persistência definitiva.
- existe PricingRule no domínio e em endpoints, mas o cálculo atual da reserva não depende dela.
- não há mecanismo transacional.
- não há background jobs.
- não há refresh token persistido.
- não há separação formal entre DTO de saída e entidade em todos os endpoints; alguns controllers retornam a própria entidade.

## 10. Arquitetura do Front-end

O front-end oficial do projeto agora vive em frontend como uma SPA React com TypeScript e Vite.

Objetivo arquitetural do front:

- consumir a API existente sem recriar domínio.
- refletir JWT, RBAC e ownership do backend.
- separar experiência pública, autenticada e administrativa.
- manter estrutura escalável sem adicionar complexidade desnecessária.

### 10.1 Organização do Front

Estrutura principal do front:

- frontend/src/app: providers globais.
- frontend/src/api: cliente HTTP e serviços de integração.
- frontend/src/components: primitives e componentes reutilizáveis.
- frontend/src/hooks: hooks de sessão e resolução de pagamento.
- frontend/src/layouts: cascas de navegação pública e autenticada.
- frontend/src/pages: telas públicas, customer e admin.
- frontend/src/routes: roteamento e guards.
- frontend/src/store: estado persistido de sessão.
- frontend/src/types: contratos TypeScript alinhados à API.
- frontend/src/lib: utilitários de formatação e leitura do JWT.

### 10.2 Separação de Experiências

Áreas do produto:

- pública: home, login, catálogo e detalhe do veículo.
- autenticada customer: dashboard, perfil, reservas, detalhe da reserva e pagamento.
- autenticada admin: dashboard admin, usuários, veículos e pricing rules.

Essa separação é feita por rota, layout e visibilidade de menu.

### 10.3 Estratégia de Roteamento

Rotas públicas:

- /
- /login
- /catalogo
- /catalogo/:vehicleId

Rotas autenticadas customer:

- /app/dashboard
- /app/profile
- /app/reservas
- /app/reservas/:reservationId
- /app/reservas/:reservationId/pagamento

Rotas autenticadas admin:

- /admin/dashboard
- /admin/users
- /admin/vehicles
- /admin/vehicles/new
- /admin/vehicles/:vehicleId
- /admin/pricing

Guards do front:

- RequireAuth para sessão válida.
- RequireAdmin para páginas administrativas.

### 10.4 Estratégia de Estado

O front usa:

- Zustand para sessão e memória local de pagamentos conhecidos.
- TanStack Query para cache, invalidação e loading states.

Estado persistido:

- token JWT.
- usuário autenticado.
- registro local reservationId -> paymentId quando o fluxo de pagamento passa pelo front.

### 10.5 Estratégia de Autenticação no Front

O front:

- armazena o JWT no cliente para suportar SPA com backend atual.
- injeta Authorization Bearer nas requisições protegidas.
- tenta renovar a sessão usando o endpoint /api/v1/auth/refresh quando o token está perto de expirar.
- encerra a sessão ao receber 401 do backend.
- usa os claims e o perfil carregado em /api/v1/users/me para controlar navegação e menus.

Observação importante:

- o backend atual não expõe refresh token persistido. O front só reutiliza o token atual no fluxo de refresh suportado pela API.

### 10.6 Estratégia de Consumo da API

O cliente HTTP:

- centraliza headers e tratamento de 401.
- usa baseURL configurável por variável de ambiente.
- em desenvolvimento, usa proxy do Vite para /api -> backend .NET local, evitando problema de CORS sem alterar a API.

### 10.7 Relação Entre Front e Back

O front respeita as limitações reais da API:

- reserva é criada por categoria, não por veículo específico.
- o fluxo de pagamento usa apenas preauth, capture, refund e get by id.
- como não existe endpoint para listar pagamentos por reserva ou por usuário, o front resolve o pagamento conhecido por registro local e por correspondência validada no seed quando possível.
- menus e ações administrativas só aparecem para Admin.

### 10.8 Estratégia de UX

O front foi desenhado para parecer produto real, mas sem inventar módulos:

- landing com posicionamento institucional claro.
- catálogo com filtros reais da API.
- detalhe do veículo com CTA de reserva coerente com a regra de reserva por categoria.
- dashboard customer com foco em reservas ativas e próximas ações.
- dashboard admin com foco operacional em usuários, frota e pricing rules.
