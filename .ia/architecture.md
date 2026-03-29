# ARCHITECTURE

Este documento descreve a arquitetura que existe hoje no projeto. Ele deve servir como referência para reconstruir a API com o mesmo comportamento, não como backlog de melhorias futuras.

## 1. Visão Geral

Arquitetura em camadas simples, agora separada em projetos .NET distintos:

1. AlugueldeCarros expõe HTTP, autenticação JWT, middleware, DTOs e bootstrap.
2. AlugueldeCarros.Application concentra regras de negócio e contratos.
3. AlugueldeCarros.Infrastructure mantém repositórios in-memory e carga JSON.
4. AlugueldeCarros.Domain contém entidades e enums.

Fluxo padrão:

Cliente HTTP -> middleware ASP.NET Core -> controller -> application service -> repository in-memory -> resposta JSON.

Objetivo deste arquivo para reconstrução:

- descrever a arquitetura que precisa ser reproduzida.
- registrar limites importantes do estado atual.
- evitar que outra IA troque a implementação real por uma arquitetura "melhorada".

Este arquivo não substitui o código-fonte, mas deve permitir reconstrução estrutural fiel quando lido junto com business-rules.md, standards.md e tech-stack.md.

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

Program.cs da API faz o seguinte:

1. Registra controllers e Swagger.
2. Lê JwtSettings de configuração.
3. Configura AddAuthentication().AddJwtBearer().
4. Configura AddAuthorization().
5. Registra os contratos de repository com implementações in-memory como singleton.
6. Registra os contratos de service com implementações da Application como scoped.
7. Registra ITokenService com JwtTokenService e JsonDataLoader como singleton.
8. Executa JsonDataLoader.LoadAllDataAsync() antes de começar a atender requisições.
9. Liga HTTPS redirection, middleware de exceção, autenticação, autorização e mapeamento de controllers.

Ordem real de registro que importa hoje:

- IReservationRepository é registrado antes de IVehicleRepository.
- IVehicleRepository depende da leitura de reservas para filtrar disponibilidade.
- repositories são singleton e services são scoped.
- JsonDataLoader executa uma carga única no startup e depois o sistema opera apenas em memória.

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

Inventario minimo de rotas expostas hoje:

- POST api/v1/auth/register
- POST api/v1/auth/login
- POST api/v1/auth/refresh
- GET api/v1/users/me
- GET api/v1/users/me/reservations
- GET api/v1/vehicles/categories
- GET api/v1/vehicles/search
- GET api/v1/vehicles/{id}
- GET api/v1/branches
- POST api/v1/reservations
- GET api/v1/reservations/{id}
- PATCH api/v1/reservations/{id}
- POST api/v1/reservations/{id}/cancel
- POST api/v1/payments/preauth
- POST api/v1/payments/capture
- POST api/v1/payments/refund
- GET api/v1/payments/{id}
- GET api/v1/pricing/rules
- POST api/v1/pricing/rules
- PATCH api/v1/pricing/rules/{id}
- GET api/v1/pricing/rules/{id}
- GET api/v1/admin/users
- POST api/v1/admin/users/{id}/roles
- POST api/v1/admin/vehicles
- PATCH api/v1/admin/vehicles/{id}

Observação importante:

- a listagem acima registra o inventario minimo de endpoints para reconstrução fiel.
- verbos HTTP, autenticação e regras de uso devem seguir business-rules.md e o código-fonte atual.

Matriz minima de acesso por rota:

- POST api/v1/auth/register: pública, AuthController.
- POST api/v1/auth/login: pública, AuthController.
- POST api/v1/auth/refresh: pública, AuthController.
- GET api/v1/users/me: autenticada, UserController.
- GET api/v1/users/me/reservations: autenticada, UserController.
- GET api/v1/vehicles/categories: pública, VehiclesController.
- GET api/v1/vehicles/search: pública, VehiclesController.
- GET api/v1/vehicles/{id}: pública, VehiclesController.
- GET api/v1/branches: pública, BranchesController.
- POST api/v1/reservations: autenticada, ReservationsController.
- GET api/v1/reservations/{id}: autenticada com ownership ou Admin, ReservationsController.
- PATCH api/v1/reservations/{id}: autenticada com ownership ou Admin, ReservationsController.
- POST api/v1/reservations/{id}/cancel: autenticada com ownership ou Admin, ReservationsController.
- POST api/v1/payments/preauth: autenticada com ownership da reserva ou Admin, PaymentsController.
- POST api/v1/payments/capture: autenticada com ownership do pagamento ou Admin, PaymentsController.
- POST api/v1/payments/refund: autenticada com ownership do pagamento ou Admin, PaymentsController.
- GET api/v1/payments/{id}: autenticada com ownership do pagamento ou Admin, PaymentsController.
- GET api/v1/pricing/rules: pública, PricingController.
- POST api/v1/pricing/rules: Admin, PricingController.
- PATCH api/v1/pricing/rules/{id}: Admin, PricingController.
- GET api/v1/pricing/rules/{id}: pública, PricingController.
- GET api/v1/admin/users: Admin, AdminUsersController.
- POST api/v1/admin/users/{id}/roles: Admin, AdminUsersController.
- POST api/v1/admin/vehicles: Admin, AdminVehiclesController.
- PATCH api/v1/admin/vehicles/{id}: Admin, AdminVehiclesController.

### 4.2 Application

Projetos e responsabilidades atuais:

- AlugueldeCarros.Domain: entidades e enums.
- AlugueldeCarros.Application: contratos de repositories, contratos de services, abstração de token e services de negócio.
- AlugueldeCarros.Infrastructure: implementações InMemory*Repository e JsonDataLoader.
- AlugueldeCarros: controllers, DTOs, configurações, middleware, JWT concreto e Program.cs.

Dependências permitidas:

- API depende de Application, Infrastructure e Domain.
- Infrastructure depende de Application e Domain.
- Application depende de Domain.
- Domain não depende das outras camadas.

### 4.3 Services

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

Observação importante: os controllers recebem contratos de service, e as implementações concretas ficam na Application.

### 4.4 Repositories

Responsabilidade:

- manter coleções List<T> em memória.
- buscar, adicionar, atualizar e remover entidades.
- gerar IDs inteiros incrementais em AddAsync na maioria das implementações.

Padrão atual:

- interfaces ficam em AlugueldeCarros.Application/Contracts/Repositories.
- implementações in-memory ficam em AlugueldeCarros.Infrastructure/Repositories.
- os dados são carregados uma vez no startup e depois vivem em memória.
- apenas users.json continua com persistência de volta para JSON após mutações.

Arquivos seed usados no estado atual:

- users.json
- roles.json
- user-roles.json
- branches.json
- vehicle-categories.json
- vehicles.json
- reservations.json
- payments.json
- pricing-rules.json

Relações que precisam continuar válidas na reconstrução:

- User se relaciona com Role por UserRole.
- Reservation referencia User, VehicleCategory e opcionalmente Vehicle.
- Payment referencia Reservation.
- Vehicle referencia Branch e VehicleCategory.
- PricingRule referencia VehicleCategory.

## 5. Modelo de Dados Operacional

Entidades principais do domínio:

- User
- Role
- UserRole
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

Campos e vínculos que outra IA deve preservar, mesmo que os nomes internos variem minimamente:

- User: Id, Email, PasswordHash e coleção ou resolução de roles.
- Vehicle: Id, CategoryId, BranchId, DailyRate e Status.
- Reservation: Id, UserId, CategoryId, VehicleId, StartDate, EndDate, Status e TotalAmount.
- Payment: Id, ReservationId, Amount, Status e CreatedAt.
- PricingRule: vínculo com categoria e campos suficientes para CRUD administrativo.

Dados operacionais mínimos hoje presentes no seed e úteis para reconstrução fiel:

- existe um customer seedado com email customer@example.com e role Customer.
- existe um admin seedado com email admin@aluguel.com e role Admin.
- os vínculos entre users e roles vêm de roles.json e user-roles.json.
- users.json é o único seed com persistência de volta para arquivo após mutações do repositório.

Contratos minimos de entrada realmente usados hoje:

- RegisterRequest: Email, Password, FirstName, LastName.
- LoginRequest: Email, Password.
- AuthResponse: Token e opcionalmente Email, conforme endpoint.
- CreateReservationRequest: CategoryId, StartDate, EndDate.
- UpdateReservationRequest: StartDate?, EndDate?, Status?.
- PreauthRequest: ReservationId, Amount.
- CaptureRequest: PaymentId.
- RefundRequest: PaymentId.
- AddUserRolesRequest: Roles como lista de string.
- CreateVehicleRequest e UpdateVehicleRequest: LicensePlate, Model, Year, CategoryId, BranchId, DailyRate, Status.

Formato de resposta que precisa ser preservado no nivel comportamental:

- endpoints de criação retornam Created ou CreatedAtAction com o recurso criado em vários módulos.
- alguns endpoints retornam entidades diretamente.
- alguns endpoints retornam objetos anônimos projetados pelo controller.

## 6. Arquitetura Oficial do Front-end

O front oficial permanece em React + TypeScript na pasta frontend, mas a arquitetura de referência para evolução e reconstrução passa a exigir separação rígida entre estrutura visual, lógica e estilo.

Essa diretriz não muda a stack. Ela define como a stack deve ser organizada.

### 6.1 Camadas obrigatórias no front

- camada visual
	- arquivo dedicado de view/template, preferencialmente `.view.tsx` ou equivalente claramente identificável
	- responsabilidade: markup, composição visual, binding de props e renderização
- camada lógica
	- arquivo dedicado `.ts`, preferencialmente `.logic.ts`, hook dedicado ou service dedicado
	- responsabilidade: handlers, integração com serviços, estado derivado, orquestração de hooks e regras locais de tela
- camada de estilo
	- arquivo dedicado `.css` ou equivalente de estilo isolado
	- responsabilidade: estilos, classes, tokens visuais e organização estética desacoplada da lógica

### 6.2 Regra de separação

No front-end, nenhum componente ou feature deve concentrar tudo em um único arquivo quando isso comprometer clareza, manutenção e escalabilidade.

Princípios obrigatórios:

- a lógica de negócio não deve ficar misturada com a estrutura visual
- o CSS deve ficar desacoplado da lógica
- a view deve ser tratada como camada de apresentação, não como concentrador de regra de domínio
- arquivos `.tsx` devem priorizar estrutura visual e composição, não decisão de domínio complexa
- regras locais de tela, integrações e handlers reutilizáveis devem sair da view e ir para `.ts`, hooks ou services

### 6.3 Organização por responsabilidade

A arquitetura do front deve favorecer organização por responsabilidade e por feature.

Exemplo conceitual aceito:

- `FeatureName.view.tsx` para estrutura visual
- `FeatureName.logic.ts` para lógica local e integração
- `FeatureName.css` para estilo

Variações são aceitas apenas se preservarem a mesma separação real entre responsabilidades.

### 6.4 Relação com o estado atual do projeto

O projeto atual ainda possui arquivos agregados como `public-pages.tsx`, `user-pages.tsx` e `admin-pages.tsx` como pontos de entrada de telas. Isso faz parte do estado atual observado e não deve ser apagado da documentação.

Ao mesmo tempo, a diretriz oficial para reconstrução e refatoração futura passa a ser:

- evitar crescimento de arquivos gigantes que misturem interface, comportamento e estilo
- decompor telas e componentes por responsabilidade clara
- isolar a estrutura visual da lógica sempre que possível
- tratar a separação entre `.tsx`, `.ts` e `.css` como requisito arquitetural do front
- falhas tratadas pelo middleware retornam JSON com a propriedade error.

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
- expõe create, get by id, patch e cancel.

### Payments

- preauth cria pagamento com status PENDING.
- capture aprova ou recusa de forma determinística com base em paymentId % 10.
- quando aprovado, a reserva vira CONFIRMED e o veículo vira RESERVED.
- refund só aceita pagamento APPROVED e troca o status para REFUNDED.

Mapa minimo modulo -> dependencias principais:

- AuthController -> AuthService -> IUserRepository, IRoleRepository, JwtTokenService, PasswordHasher.
- UserController -> UserService e ReservationService.
- VehiclesController -> VehicleService e VehicleCategoryService.
- ReservationsController -> ReservationService.
- PaymentsController -> PaymentService e ReservationService para ownership.
- PricingController -> PricingService.
- AdminUsersController -> UserService.
- AdminVehiclesController -> VehicleService.

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

Para reconstrução fiel, o front deve preservar estes limites:

- não inventar endpoints.
- não duplicar regra de negócio que já existe no backend.
- não trocar SPA por SSR.
- não modelar reserva como seleção manual do veículo final, porque a API reserva por categoria.

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

Layouts principais usados hoje:

- public-layout para área pública.
- app-shell para áreas autenticadas customer e admin.

### 10.4 Estratégia de Estado

O front usa:

- Zustand para sessão e memória local de pagamentos conhecidos.
- TanStack Query para cache, invalidação e loading states.

Estado persistido:

- token JWT.
- usuário autenticado.
- registro local reservationId -> paymentId quando o fluxo de pagamento passa pelo front.

Estado derivado que deve continuar no cliente:

- role atual do usuário para menus e guards.
- sessão carregada a partir do token e de /api/v1/users/me.
- cache de listas e detalhes via TanStack Query.

Regra operacional importante para evitar erro de autenticação no front:

- após login ou cadastro, o front deve usar imediatamente o token retornado pela API para carregar /api/v1/users/me.
- essa chamada inicial a /users/me não deve depender apenas do token já ter sido persistido no store do cliente.
- na reidratação da sessão, o bootstrap também deve usar explicitamente o token válido ao buscar /users/me.

### 10.5 Estratégia de Autenticação no Front

O front:

- armazena o JWT no cliente para suportar SPA com backend atual.
- injeta Authorization Bearer nas requisições protegidas.
- tenta renovar a sessão usando o endpoint /api/v1/auth/refresh quando o token está perto de expirar.
- encerra a sessão ao receber 401 do backend.
- usa os claims e o perfil carregado em /api/v1/users/me para controlar navegação e menus.
- após login, cadastro ou refresh, usa o token mais recente para buscar /api/v1/users/me antes de consolidar a sessão no estado persistido.

Observação importante:

- o backend atual não expõe refresh token persistido. O front só reutiliza o token atual no fluxo de refresh suportado pela API.

### 10.6 Estratégia de Consumo da API

O cliente HTTP:

- centraliza headers e tratamento de 401.
- usa baseURL configurável por variável de ambiente.
- em desenvolvimento, usa proxy do Vite para /api -> backend .NET local, evitando problema de CORS sem alterar a API.
- o alvo padrão atual desse proxy deve ser o backend HTTPS local em https://localhost:7110.

Variáveis de ambiente relevantes para reprodução:

- VITE_API_BASE_URL para apontar diretamente para a API quando necessário.
- VITE_PROXY_TARGET para definir o backend alvo do proxy local do Vite.

Valor padrão esperado hoje:

- VITE_PROXY_TARGET=https://localhost:7110.

### 10.7 Relação Entre Front e Back

O front respeita as limitações reais da API:

- reserva é criada por categoria, não por veículo específico.
- o fluxo de pagamento usa apenas preauth, capture, refund e get by id.
- como não existe endpoint para listar pagamentos por reserva ou por usuário, o front resolve o pagamento conhecido por registro local e por correspondência validada no seed quando possível.
- menus e ações administrativas só aparecem para Admin.

Mapa minimo tela -> dependencias principais:

- home e login: endpoints de auth e navegação pública.
- catálogo e detalhe: vehicles/search, vehicles/{id}, vehicles/categories e branches.
- dashboard e perfil do customer: users/me e users/me/reservations.
- detalhe e pagamento da reserva: reservations/{id}, payments/preauth, payments/capture, payments/refund e payments/{id}.
- área de usuários admin: admin/users e admin/users/{id}/roles.
- área de veículos admin: admin/vehicles e vehicles/{id} para contexto de edição.
- área de pricing admin: pricing/rules e pricing/rules/{id}, com criação e atualização administrativa.

### 10.8 Estratégia de UX

O front foi desenhado para cobrir o produto real existente, sem inventar módulos:

- landing com posicionamento institucional claro.
- catálogo com filtros reais da API.
- detalhe do veículo com CTA de reserva coerente com a regra de reserva por categoria.
- dashboard customer com foco em reservas ativas e próximas ações.
- dashboard admin com foco operacional em usuários, frota e pricing rules.

Critérios minimos para considerar a reconstrução fiel:

- mesmas áreas públicas, customer e admin.
- mesmas rotas principais da SPA.
- mesmo modelo de autenticação por JWT.
- mesma separação de guards RequireAuth e RequireAdmin.
- mesmo fluxo busca -> reserva -> pagamento.
- mesma limitação estrutural de pagamentos conhecidos sem listagem completa por reserva.
