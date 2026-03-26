# BUSINESS RULES — Aluguel de Carros API

**Este documento é a âncora de domínio do projeto.**
**Todo código, endpoint e decisão técnica deve estar alinhado às regras aqui definidas.**

---

## 1. VISÃO DO PRODUTO

Sistema de aluguel de veículos baseado em **reservas por categoria**, com suporte a:
- Autenticação e autorização via JWT
- Gestão de frota por branches
- Busca e reserva de veículos com análise de disponibilidade
- Processamento de pagamentos (pré-autorização e captura)
- Cancelamento e remarcação sob regras específicas

**Fase Atual**: Mockup funcional com dados em JSON (sem persistência em banco real).

---

## 2. GLOSSÁRIO DO DOMÍNIO

| Termo | Definição |
|-------|-----------|
| **Branch** | Locação física onde veículos podem ser retirados/devolvidos |
| **Vehicle Category** | Classificação de veículos (ex: Econômico, SUV, Executivo) |
| **Vehicle** | Veículo específico que pertence a uma categoria e a um branch |
| **Reservation** | Promessa de alocação de um veículo de uma categoria para datas específicas |
| **Pricing Rule** | Regra que define o valor/dia para cada categoria por branch e período |
| **Payment** | Transação de pagamento (pré-autorização, captura, refund) |
| **User Role** | Perfil de usuário que define permissões e acesso a endpoints |
| **Customer Profile** | Dados expandidos do cliente (licença, documentos, histórico) |
| **Availability** | Estado de um veículo em uma data/hora específica para oferecimento na busca |

---

## 3. PERFIS E PERMISSÕES (RBAC)

### 3.1 Papéis Cadastrados

```
ADMIN      → Gestor do sistema, frota e operações
USER       → Cliente de aluguel padrão
GUEST      → Sem permissão, apenas consulta pública (futuro)
```

### 3.2 Mapeamento de Acesso

#### **ADMIN** → Endpoint Pattern: `/api/v1/admin/**`

- ✅ Listar e gerenciar usuários
- ✅ Adicionar/remover papéis de usuários
- ✅ Criar e editar veículos da frota
- ✅ Criar e editar categorias
- ✅ Criar e editar regras de preço
- ✅ Listar todas as reservas do sistema
- ✅ Listar todos os pagamentos
- ✅ Visualizar histórico completo

#### **USER** → Endpoint Pattern: `/api/v1/**` (exceto `/admin`)

- ✅ Registrar-se e autenticar
- ✅ Visualizar próprio perfil (`/users/me`)
- ✅ Buscar veículos por filtros
- ✅ Criar reserva para si mesmo
- ✅ Visualizar próprias reservas (`/users/me/reservations`)
- ✅ Cancelar própria reserva
- ✅ Remarcar própria reserva conforme regras
- ✅ Listar branches e categorias
- ✅ Visualizar status de pagamento

#### **GUEST** (Reservado para futuro)

- Visualizar catálogo de veículos e preços
- Buscar disponibilidade

### 3.3 Regras de Acesso ao Próprio Dado

```
Regra: Um USER só pode visualizar/modificar reservas, pagamentos e dados
que pertencem a si mesmo, a menos que seja ADMIN.

Validação obrigatória em todo endpoint `/users/me/**` e `/reservations/{id}`:
- Verificar se o userId do token == userId do recurso
- Se não for ADMIN, rejeitar com 403 Forbidden
```

### 3.4 Regras para Endpoints ADMIN

```
Regra: Todos os endpoints `/admin/**` exigem:
1. Token JWT válido
2. Role = "ADMIN" no token
3. Se falhar qualquer validação, retornar 403 Forbidden

Implementação obrigatória:
- Use [Authorize(Roles = "ADMIN")] nos controllers
- Não há exceção de segurança
```

---

## 4. REGRAS DE DISPONIBILIDADE

### 4.1 Definição de Disponibilidade

Uma vehicle de uma category está **DISPONÍVEL** para uma reserva se:

```
1. Existe pelo menos um veículo específico da categoria no branch
2. O veículo tem status = AVAILABLE ou RESERVED
3. NÃO existe sobreposição de datas com reservas CONFIRMED ou PENDING_PAYMENT
4. O veículo NÃO está em status RENTED, MAINTENANCE ou BLOCKED
```

### 4.2 Lógica de Busca

```
GET /api/v1/vehicles/search?
  branchId={id}
  &from={data_inicio}
  &to={data_fim}
  &categoryId={id}
  &priceMin={valor}
  &priceMax={valor}

Retorna:
- Lista de veículos disponíveis na categoria/branch/período
- Preço calculado baseado em PricingRule
- Quantidade de dias
- Cada veículo tem status e disponibilidade
```

### 4.3 Verificação de Overbooking

```
Algoritmo para cada vehicle na category:
  count = 0
  para cada reservation:
    if reservation.status IN (CONFIRMED, PENDING_PAYMENT)
       AND reservation.checkIn <= searchTo
       AND reservation.checkOut >= searchFrom
    then count++
  
  if count > 0:
    vehicle é considerado indisponível neste período
    NÃO aparece nos resultados
```

### 4.4 Ordem de Prioridade

Quando múltiplos veículos da mesma categoria estão disponíveis:
- Ordenar por status: AVAILABLE primeiro, depois RESERVED
- Within same status: ordem não é garantida (pode ser aleatória ou por ID)

---

## 5. REGRAS DE RESERVA

### 5.1 Criação de Reserva

```
POST /api/v1/reservations
Body:
{
  "categoryId": UUID,
  "branchId": UUID,
  "checkIn": "2026-03-25T14:00:00Z",
  "checkOut": "2026-03-27T14:00:00Z",
  "passengers": 1
}

Validações OBRIGATÓRIAS:
✓ checkIn < checkOut
✓ checkIn >= agora (não reservar retroativamente)
✓ checkOut <= agora + 365 dias (limite de antecedência)
✓ Veículo da categoria existe e está disponível
✓ User está autenticado
✓ User é o criador da reserva (ou é ADMIN)

Resultado:
- Status inicial: PENDING_PAYMENT
- Gera preço com base em PricingRule
- Cria Payment record com status PRE_AUTH_PENDING
- Reserva aguarda pré-autorização antes de confirmar
```

### 5.2 Transições Válidas de Status

```
PENDING_PAYMENT
  ↓ (pré-autorização aprovada)
  ↓
CONFIRMED
  ↓ (cancelamento ou check-out)
  ↓ CANCELLED

(Se pré-autorização falhar ou expirar)
PENDING_PAYMENT → EXPIRED
```

### 5.3 Regras de Remoção de Vehicle Status

```
Quando uma reserva CONFIRMED é criada/alterada:
- O veículo specificamente alocado muda de status para RESERVED
  (intenção: este veículo está comprometido para esta reserva)

Quando a reserva é cancelada:
- O veículo volta para AVAILABLE
  (se nenhuma outra reserva o está usando)

Lógica: Para cada vehicle alocado a uma reserva:
  if reservation.status = CONFIRMED:
    vehicle.status = RESERVED
  elif reservation.status IN (CANCELLED, EXPIRED):
    if não existem outras CONFIRMED reservas para este vehicle:
      vehicle.status = AVAILABLE
```

### 5.4 Limite de Reservas Ativas

```
Um USER pode ter no máximo 5 reservas CONFIRMED ou PENDING_PAYMENT
ativas simultaneamente.

Valiar ao criar nova reserva:
  count = reservations onde status IN (CONFIRMED, PENDING_PAYMENT)
  if count >= 5:
    retornar 400 Bad Request: "Limite de reservas ativas atingido"
```

---

## 6. REGRAS DE CANCELAMENTO

### 6.1 Quem Pode Cancelar

```
- O USER que criou a reserva
- Um ADMIN
- Ninguém mais
```

### 6.2 Estados Canceláveis

```
Só é permitido cancelar se:
- Reserva está em status CONFIRMED ou PENDING_PAYMENT
- checkIn está no futuro (> agora + 2 horas)

Se tentar cancelar com < 2 horas:
  Retornar 422: "Cancelamento não permitido com menos de 2 horas antes do check-in"
```

### 6.3 Consequências do Cancelamento

```
1. Status: CONFIRMED → CANCELLED
2. Refund: Automático (regra em 6.5)
3. Vehicle: Volta para AVAILABLE (se não houver outras reservas)
4. Payment record: Criado com tipo REFUND (status: APPROVED)
```

---

## 7. REGRAS DE PAGAMENTO

### 7.1 Fluxo de Pagamento

```
1. Criar Reserva (status: PENDING_PAYMENT)
   ↓
2. Pré-autorização (PRE_AUTH)
   ├─ Se APROVADA: reserva → CONFIRMED, Vehicle → RESERVED
   └─ Se RECUSADA: reserva → PENDING_PAYMENT (permanecer tentando)
   ↓
3. (Futuro) Captura no check-in (CAPTURE)
   ├─ Se APROVADA: efetiva o pagamento
   └─ Se RECUSADA: reserva pode ser cancelada
   ↓
4. Refund (opcional, após cancelamento ou ajuste)
```

### 7.2 Cálculo de Valor

```
Fórmula:
  dias = (checkOut - checkIn).TotalDays (arredondar para cima)
  pricePerDay = PricingRule.amount (para a categoria/branch/período)
  totalAmount = dias × pricePerDay

Exemplo:
  checkIn: 2026-03-25 14:00
  checkOut: 2026-03-27 14:00
  dias = 2
  pricePerDay = R$ 150
  totalAmount = R$ 300

Aplicar tax/fees como:
  if totalAmount > 500:
    serviceFee = totalAmount × 0.05  (5%)
  else:
    serviceFee = totalAmount × 0.03  (3%)
  
  finalAmount = totalAmount + serviceFee
```

### 7.3 Status de Pagamento

Válidos:

```
PRE_AUTH_PENDING   → Aguardando pré-autorização
PRE_AUTH_APPROVED  → Pré-autorização ok, pode confirmar reserva
PRE_AUTH_DECLINED  → Pré-autorização recusada
CAPTURED           → Pagamento capturado (check-in realizado)
REFUNDED           → Reembolso processado
```

### 7.4 Simulação de Resposta

```
Em MOCK (sem gateway real):
- 90% das pré-autorizações retornam APPROVED
- 10% retornam DECLINED (simular recusa)
- Refunds sempre retornam APPROVED no mock

Implementação: Usar seed aleatório ou ID da reserva para decidir
```

### 7.5 Tentativas de Pré-autorização

```
Um USER pode tentar pré-autorizar a mesma reserva múltiplas vezes
enquanto estiver em PENDING_PAYMENT.

Cada tentativa:
1. Chama POST /api/v1/payments/preauth ?reservationId={id}
2. Cria novo Payment record (nova linha)
3. Atualiza reservation.paymentId para apontar ao novo payment
4. Se APPROVED: reserva → CONFIRMED

Máximo de tentativas: 3 por reserva
Além disso, retornar 429: "Muitas tentativas de pagamento"
```

---

## 8. REGRAS DE REFUND

### 8.1 Elegibilidade para Refund

```
Um REFUND é automáticamente processado quando:
- Usuário cancela própria reserva
- Pré-autorização não foi capturada (ainda em PENDING_PAYMENT)
- checkIn > agora + 2 horas

Refund rejeitado se:
- Reserva já foi CAPTURADA (check-in realizado)
- Cancelamento em < 2 horas
```

### 8.2 Percentage de Refund

```
if checkIn > agora + 7 dias:
  refundPercentage = 100%
elif checkIn > agora + 3 dias:
  refundPercentage = 80%
elif checkIn > agora + 1 dia:
  refundPercentage = 50%
else:
  refundPercentage = 0%  (sem refund)

refundAmount = paymentAmount × (refundPercentage / 100)
```

### 8.3 Processamento

```
1. POST /api/v1/payments/refund ?paymentId={id}
2. Cria novo Payment record com tipo REFUND
3. Valida elegibilidade
4. Se elegível: status = APPROVED, amount = refundAmount calculado
5. Se não elegível: status = DECLINED, motivo em description
```

---

## 9. STATUS OFICIAIS

### 9.1 Reservation Status

```
PENDING_PAYMENT   → Reserva criada, aguardando pré-autorização
CONFIRMED         → Pré-autorização aprovada, reserva confirmada
CANCELLED         → Cancelada pelo usuário ou admin
EXPIRED           → Expirou por timeout de pré-autorização (30 dias)
```

### 9.2 Vehicle Status

```
AVAILABLE         → Veículo livre para reserva
RESERVED          → Veículo alocado a uma reserva CONFIRMED
RENTED            → Veículo foi retirado no check-in (fora do escopo agora)
MAINTENANCE       → Veículo em manutenção
BLOCKED           → Veículo bloqueado (administrativo)
```

### 9.3 Payment Status

```
PRE_AUTH_PENDING  → Gateways de pagamento processando
PRE_AUTH_APPROVED → Pré-auth bem-sucedida
PRE_AUTH_DECLINED → Pré-auth recusada
CAPTURED          → Captura realizada no check-in
REFUNDED          → Refund processado
```

---

## 10. ENTIDADES PRINCIPAIS — ESTRUTURA DE DADOS

### 10.1 User

```
{
  "id": UUID,
  "email": "usuario@email.com",
  "passwordHash": "hash_bcrypt",
  "fullName": "Nome Completo",
  "cpf": "12345678900",
  "phone": "+55 11 99999-9999",
  "createdAt": "2026-01-01T10:00:00Z",
  "status": "ACTIVE | BLOCKED"
}
```

### 10.2 UserRole (Many-to-Many)

```
{
  "id": UUID,
  "userId": UUID,
  "roleId": UUID,
  "assignedAt": "2026-01-01T10:00:00Z"
}
```

### 10.3 Role

```
{
  "id": UUID,
  "name": "ADMIN | USER",
  "description": "...",
  "permissions": [...]  (reservado para futuro)
}
```

### 10.4 CustomerProfile

```
{
  "id": UUID,
  "userId": UUID,
  "licenseNumber": "12345678900",
  "licenseExpiry": "2030-01-01",
  "country": "BR",
  "address": "Rua X, 123",
  "city": "São Paulo",
  "state": "SP",
  "zipCode": "01310100",
  "documents": [...]  (referências)
}
```

### 10.5 Branch

```
{
  "id": UUID,
  "name": "Branch Centro",
  "city": "São Paulo",
  "address": "Av. Paulista, 1000",
  "phone": "+55 11 3000-0000",
  "operatingHours": "08:00-20:00",
  "status": "ACTIVE | INACTIVE"
}
```

### 10.6 VehicleCategory

```
{
  "id": UUID,
  "name": "Econômico | SUV | Executivo",
  "description": "...",
  "seatingCapacity": 5,
  "transmission": "MANUAL | AUTOMATIC",
  "fuelType": "GASOLINE | DIESEL | HYBRID | ELECTRIC",
  "status": "ACTIVE | INACTIVE"
}
```

### 10.7 Vehicle

```
{
  "id": UUID,
  "categoryId": UUID,
  "branchId": UUID,
  "licensePlate": "ABC1234",
  "brand": "Toyota",
  "model": "Corolla",
  "year": 2024,
  "color": "Prata",
  "mileage": 5000,
  "status": "AVAILABLE | RESERVED | RENTED | MAINTENANCE | BLOCKED",
  "lastServiceDate": "2026-01-15T10:00:00Z",
  "createdAt": "2026-01-01T10:00:00Z"
}
```

### 10.8 Reservation

```
{
  "id": UUID,
  "userId": UUID,
  "categoryId": UUID,
  "branchId": UUID,
  "vehicleId": UUID | null,  (alocado depois se necessário)
  "checkIn": "2026-03-25T14:00:00Z",
  "checkOut": "2026-03-27T14:00:00Z",
  "status": "PENDING_PAYMENT | CONFIRMED | CANCELLED | EXPIRED",
  "totalPrice": 355.00,
  "createdAt": "2026-03-20T10:00:00Z",
  "cancelledAt": null,
  "paymentId": UUID
}
```

### 10.9 PricingRule

```
{
  "id": UUID,
  "categoryId": UUID,
  "branchId": UUID,
  "amount": 150.00,  (por dia)
  "validFrom": "2026-01-01",
  "validTo": "2026-12-31",
  "status": "ACTIVE | INACTIVE",
  "createdAt": "2026-01-01T10:00:00Z"
}
```

### 10.10 Payment

```
{
  "id": UUID,
  "reservationId": UUID,
  "userId": UUID,
  "type": "PRE_AUTH | CAPTURE | REFUND",
  "amount": 355.00,
  "status": "PRE_AUTH_PENDING | PRE_AUTH_APPROVED | PRE_AUTH_DECLINED | CAPTURED | REFUNDED",
  "transactionId": "txn_12345",  (mock)
  "createdAt": "2026-03-20T10:00:00Z",
  "processedAt": null
}
```

---

## 11. LIMITES CLAROS DE ESCOPO

### 11.1 O Que É FORA DO ESCOPO (Proibido)

```
❌ Cotação/Simulação        (sem previsão)
❌ Check-in/Check-out       (operação futura)
❌ Avaliações/Reviews       (fora do MVP)
❌ Inspeções de veículos    (futuro)
❌ Contratos operacionais   (legal, não tech)
❌ Operações de loja        (lojas físicas)
❌ Campanhas/Promoções      (marketing)
❌ Cupons/Vouchers          (future commerce)
❌ Relatórios complexos     (analytics future)
❌ Auditoria avançada       (compliance future)
❌ Push notifications       (integração future)
❌ Social login             (auth future)
❌ Integrações bancárias    (payment future)
```

### 11.2 Bloqueadores de Implementação

Se um prompt futuro tentar implementar qualquer item do escopo FORA, rejeitar com mensagem clara:
```
"[BLOQUEADO] Esta funcionalidade está fora do escopo definido em .ai/business-rules.md#11.1"
```

---

## 12. DADOS MOCKADOS — COERÊNCIA COM O DOMÍNIO

### 12.1 Princípios

```
1. Dados JSON devem refletir cenários reais de aluguel
2. Categorias devem ter preços diferenciados
3. Branches devem ter localidades diferentes
4. Veículos devem ter status variados (alguns AVAILABLE, alguns RESERVED, etc)
5. Reservas devem ter datas futuras coerentes
6. Pagamentos devem ter status realistas (alguns APPROVED, alguns DECLINED)
```

### 12.2 Exemplo de Coerência

```
branches.json
- 3 branches: São Paulo, Rio, Brasília

vehicle-categories.json
- Econômico (R$ 80/dia)
- SUV (R$ 200/dia)
- Executivo (R$ 300/dia)

vehicles.json
- 15 veículos distribuídos entre branches/categorias
- Status: 10 AVAILABLE, 3 RESERVED, 1 MAINTENANCE, 1 BLOCKED

pricing-rules.json
- 9 regras: 3 categorias × 3 branches
- Preços coerentes com categoria

reservations.json
- 5 reservas com datas futuras
- Status mix: 2 CONFIRMED, 2 PENDING_PAYMENT, 1 CANCELLED

payments.json
- Assoc. com reservas
- Status mix: 2 APPROVED, 1 DECLINED, 1 PENDING
```

---

## 13. REGRAS PARA AGENTES DE IA

### 13.1 Proibições Absolutas

```
🚫 NÃO inventar rotas fora dos endpoints definidos em .cursorrules#ENDPOINTS_OFICIAIS
🚫 NÃO sugerir banco de dados real nesta fase (mock obrigatório)
🚫 NÃO criar Entity Framework DbContext para persistência real
🚫 NÃO adicionar funcionalidades fora do escopo (seção 11.1)
🚫 NÃO sobrescrever arquivos corretos sem necessidade
🚫 NÃO alterar estrutura de solução existente
🚫 NÃO criar Controllers/Services desnecessários
```

### 13.2 Obrigações

```
✅ SEMPRE ler .ai/ antes de propor changeset
✅ SEMPRE validar contra business-rules.md
✅ SEMPRE manter compilação limpa
✅ SEMPRE fazer deploy incremental (pequenos diffs)
✅ SEMPRE preservar código correto existente
✅ SEMPRE atualizar .http endpoints se alterar rotas
✅ SEMPRE validar regras de RBAC
✅ SEMPRE usar JSON mockado
```

---

## 15. COBERTURA DE TESTES

### 15.1 Cenários Críticos por Endpoint

**POST /api/v1/auth/register**
```
✅ Usuário novo com dados válidos → Criado com sucesso
✅ Email duplicado → Erro 409 Conflict
✅ Password < 8 chars → Erro 400 Bad Request
✅ Email vazio → Erro 400 Bad Request
```

**POST /api/v1/auth/login**
```
✅ Credenciais válidas → Token JWT retornado
✅ Email não existe → Erro 401 Unauthorized
✅ Password incorreto → Erro 401 Unauthorized
✅ Email/Password vazios → Erro 400 Bad Request
```

**GET /api/v1/vehicles (Search)**
```
✅ Sem filtros → Lista todos disponíveis
✅ Filtro categoria válida → Lista filtrada
✅ Filtro branch válida → Lista por branch
✅ Datas inválidas (EndDate < StartDate) → Erro 400
✅ Sem veículos disponíveis → Lista vazia (200 OK, não 404)
```

**POST /api/v1/reservations**
```
✅ Dados válidos, veículo disponível → Reserva criada (PENDING_PAYMENT)
✅ Usuário já tem 5 reservas ativas → Erro 409 Conflict
✅ Veículo não disponível na data → Erro 409 Conflict
✅ EndDate < StartDate → Erro 400 Bad Request
✅ Sem autenticação → Erro 401 Unauthorized
```

**PATCH /api/v1/reservations/{id}/cancel**
```
✅ Cancelamento < 2 horas (~início) → Refund 100% (REFUNDED)
✅ Cancelamento > 2 horas → Sem refund (CANCELLED)
✅ Reserva não existe → Erro 404 Not Found
✅ Reserva já cancelada → Erro 409 Conflict
✅ Sem autenticação → Erro 401 Unauthorized
```

**POST /api/v1/payments/preauth**
```
✅ Dados válidos, valor > 0 → Preauth criado (APPROVED ou DECLINED)
✅ Heurística (90/10): 90% APPROVED, 10% DECLINED
✅ Valor = 0 → Erro 400 Bad Request
✅ Reserva não existe → Erro 404 Not Found
```

**DELETE /api/v1/admin/users/{id}** (ADMIN only)
```
✅ ADMIN token válido → Usuário deletado (200/204)
✅ USER token (não-admin) → Erro 403 Forbidden
✅ Token expirado → Erro 401 Unauthorized
✅ Usuário não existe → Erro 404 Not Found
```

**POST /api/v1/admin/vehicles**
```
✅ ADMIN, dados válidos → Veículo criado
✅ USER token → Erro 403 Forbidden
✅ Categoria não existe → Erro 400/404
✅ Branch não existe → Erro 400/404
```

### 15.2 Mapeamento de Cenários

| Endpoint | Happy Path | Validação | Autenticação | Autorização | Edge Cases |
|----------|-----------|-----------|--------------|-------------|------------|
| POST /auth/register | 1 | 3 | - | - | 1 |
| POST /auth/login | 1 | 3 | - | - | 1 |
| GET /vehicles | 1 | 3 | ✓* | - | 2 |
| POST /reservations | 1 | 2 | ✓ | ✓ | 1 |
| PATCH /reservations/{id}/cancel | 2 | 2 | ✓ | ✓ | 0 |
| POST /payments/preauth | 1 | 2 | ✓ | ✓ | 1 |
| DELETE /admin/users/{id} | 1 | 1 | ✓ | ✓ | 1 |
| POST /admin/vehicles | 1 | 2 | ✓ | ✓ | 0 |

*GET /vehicles é público (sem token)

### 15.3 Casos de Uso Críticos

**ReservationService.CreateAsync()**
```
→ Validar 5-reserva limit (em MemoryRepository)
→ Validar disponibilidade de veículo
→ Validação de datas
→ Criar pagamento pré-autorizado (mock 90/10)
→ Retornar reserva com status PENDING_PAYMENT
```

**PaymentService.PreauthorizeAsync()**
```
→ Mock determinístico: 90% APPROVED, 10% DECLINED
→ Usar seed baseado em Guid da reserva
→ Atualizar status de payment
→ Retornar resultado previsível em testes
```

**ExceptionHandlingMiddleware**
```
→ Capturar exceptions não-tratadas
→ Retornar formato padrão (error code + message)
→ Logar erro com contexto
→ Retornar 500 para erros inesperados
```

### 15.4 Quantidade de Testes Esperada

```
Unit/Services:       80+ testes (10+ por service)
Unit/Security:       15+ testes (JWT + RBAC)
Integration:         40+ testes (5+ por controller)
Fixtures:            N/A (não testados)

TOTAL:              ~135+ testes (phase 1 complete)
```

---

## 14. AUTENTICAÇÃO E TOKENS JWT

### 14.1 Payload do Token

```
{
  "sub": "{{userId}}",
  "email": "usuario@email.com",
  "roles": ["USER", "ADMIN"],  (array)
  "exp": 3600,  (segundos)
  "iat": agora,
  "iss": "AlugueldeCarros"
}
```

### 14.2 Endpoints Públicos

```
POST /api/v1/auth/register   (sem token)
POST /api/v1/auth/login      (sem token)
GET  /api/v1/vehicles/categories   (opcional token)
GET  /api/v1/branches              (opcional token)
```

### 14.3 Endpoints Protegidos

```
Todo endpoint exceto lista acima exige token válido no header:
Authorization: Bearer {token}
```

---

## 15. CHANGELOG — HISTÓRICO DE REGRAS

| Data | Evento | Descrição |
|------|--------|-----------|
| 2026-03-25 | Criação | Contexto inicial com todas as regras de negócio |

