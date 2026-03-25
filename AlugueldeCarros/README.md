# API de Aluguel de Carros

## Visão Geral do Sistema
Esta é uma API REST para um sistema de aluguel de carros, desenvolvida em C# .NET 8 com ASP.NET Core Web API. O sistema permite cadastro de usuários, busca de veículos, criação de reservas, gestão de pagamentos e administração de frota, com autenticação JWT e autorização baseada em roles (RBAC).

### Objetivo do Projeto
Criar uma API funcional para simular operações de aluguel de carros, focando em fluxos reais como busca, reserva e pagamento, sem uso de banco de dados (dados mockados em JSON).

### Principais Funcionalidades
- **Cliente**: Cadastro, login, busca de veículos, criação/cancelamento de reservas, consulta de pagamentos.
- **Admin**: Gestão de usuários, veículos, categorias, regras de preço e frota.

### Perfis de Acesso
- **Customer**: Acesso limitado a operações pessoais.
- **Admin**: Acesso total para gestão.

### Escopo do Sistema
Inclui autenticação, busca, reserva, pagamento, administração. Exclui check-in/out, avaliações, contratos, relatórios complexos.

### Itens Fora do Escopo
Cotação, inspeções, campanhas, auditoria avançada.

## Arquitetura da Solução
Arquitetura em camadas: Controllers (API), Services (lógica), Repositories (dados em memória), Domain (entidades/enums). Usa JWT para segurança, Swagger para documentação, JSON mockado para dados (sem banco).

### Tecnologias
- C# .NET 8
- ASP.NET Core Web API
- JWT Authentication
- Swagger/OpenAPI
- System.Text.Json
- Dados mockados em JSON (sem Entity Framework ou banco real)

## Estrutura de Pastas
```
AlugueldeCarros/
  Controllers/          # Endpoints da API
  DTOs/                 # Objetos de transferência de dados
  Services/             # Lógica de negócio
  Repositories/         # Acesso a dados (em memória)
  Domain/Entities/      # Modelos de domínio
  Domain/Enums/         # Enums do sistema
  Configurations/       # Configurações (JWT)
  Security/             # Serviços de segurança (JWT, hash)
  Middleware/           # Middleware customizado
  Exceptions/           # Exceções customizadas
  Mappers/              # Mapeamento entre entidades e DTOs
  Validations/          # Validações de entrada
  Mock/                 # Classes de mock
  Loaders/              # Carregadores de dados JSON
  Resources/MockData/   # Arquivos JSON com dados falsos
  Program.cs            # Configuração da aplicação
  appsettings.json      # Configurações gerais
```

## Modelagem do Sistema
### Entidades Principais
- **User**: Usuário do sistema.
- **Role**: Papéis (Customer, Admin).
- **Vehicle**: Veículos disponíveis.
- **Reservation**: Reservas feitas.
- **Payment**: Pagamentos associados.

### Relacionamentos
User -> Role (muitos-para-muitos via UserRole), Reservation -> Vehicle/Category, etc.

### Enums
- ReservationStatus: PENDING_PAYMENT, CONFIRMED, CANCELLED, EXPIRED
- VehicleStatus: AVAILABLE, RESERVED, RENTED, MAINTENANCE, BLOCKED

## Segurança
### Autenticação
Usa JWT. Tokens gerados no login, validados em requests.

### Autorização (RBAC)
Roles: Customer (operações pessoais), Admin (gestão total). Endpoints protegidos com [Authorize(Roles = "Admin")].

## API
### Endpoints Principais
- POST /api/v1/auth/register - Cadastro
- POST /api/v1/auth/login - Login
- GET /api/v1/vehicles/search - Busca de veículos
- POST /api/v1/reservations - Criar reserva
- GET /api/v1/users/me/reservations - Minhas reservas

### Exemplos de Request/Response
Cadastro: POST /api/v1/auth/register { "email": "user@example.com", "password": "123456" } -> { "token": "jwt..." }

## Execução do Projeto
### Pré-requisitos
- .NET 8 SDK instalado.

### Passos
1. `dotnet restore`
2. `dotnet build`
3. `dotnet run`
4. Acesse https://localhost:5001/swagger para testar.

### Testes
Use Swagger para testar endpoints. Exemplo: Login com dados mockados.

## Dados Mockados
Dados falsos em JSON (Resources/MockData/). Carregados na inicialização. Alterações em runtime ficam em memória.

## Fluxos Principais
1. Cadastro/Login -> Busca -> Reserva -> Pagamento.

## Qualidade e Manutenção
Código limpo, SOLID, pronto para evolução (adicionar banco futuramente).

## Troubleshooting
- Erro de build: Verifique .NET SDK.
- Erro JWT: Confirme chave secreta.
- Dados não carregam: Verifique caminhos JSON.

## Exemplos Práticos
- `dotnet run` -> API roda em https://localhost:5001
- Swagger: Documentação interativa.

## Finalização
URL API: https://localhost:5001  
URL Swagger: https://localhost:5001/swagger  
Credenciais iniciais: Ver dados mockados (ex: admin@aluguel.com / admin123).