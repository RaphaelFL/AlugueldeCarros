# BUSINESS RULES

Este documento registra apenas as regras efetivamente implementadas hoje. Ele não deve inventar fluxos futuros nem nomes de status que o código não usa.

## 1. Contexto do Produto

A API atende um cenário de aluguel de carros com:

- autenticação por JWT.
- catálogo de veículos e categorias.
- reservas vinculadas a usuário autenticado.
- pagamentos mockados.
- operações administrativas para usuários e frota.

Persistência atual:

- dados vindos de arquivos JSON.
- repositórios in-memory na Infrastructure, expostos por contratos da Application.

Escopo deste arquivo:

- registrar apenas comportamento implementado hoje.
- deixar explícitos os invariantes que outra IA não deve alterar ao reconstruir o sistema.
- separar regra real de produto de conveniência visual do front.

Invariantes globais do sistema:

- autenticação protegida usa JWT Bearer.
- os únicos papéis operacionais são Customer e Admin.
- ownership de reservas e pagamentos é aplicado no fluxo HTTP.
- reserva é criada por categoria e recebe um veículo disponível internamente.
- o sistema opera sobre dados mockados carregados em memória.

## 2. Papéis e Acesso

Papéis reais do sistema:

- Customer
- Admin

Regras:

1. Endpoints com prefixo api/v1/admin exigem role Admin.
2. Endpoints de usuário exigem autenticação.
3. Usuário comum só pode acessar a própria reserva e seus próprios pagamentos.
4. Admin pode acessar qualquer reserva e qualquer pagamento.

Invariante operacional:

- o sistema não usa um papel intermediário como Guest.

## 3. Regras de Usuário e Autenticação

### Registro

- email não pode existir previamente.
- senha nova é armazenada com hash BCrypt.
- usuário novo recebe role Customer por padrão.
- o retorno do registro já é um JWT válido.

### Login

- busca usuário por email.
- se a senha não conferir, retorna erro de credencial inválida.
- a validação aceita hash BCrypt.
- se a verificação BCrypt falhar por formato, existe fallback para comparar a senha em texto puro com PasswordHash atual.

### Refresh

- recebe um token JWT.
- extrai o claim NameIdentifier.
- se o usuário existir, gera um novo token com as roles atuais.
- não existe refresh token persistido.

## 4. Regras de Veículos e Disponibilidade

Busca de veículos:

1. Pode filtrar por categoryId.
2. Sempre considera apenas veículos com status AVAILABLE.
3. Se startDate e endDate forem informados, remove veículos com sobreposição de reserva em status CONFIRMED ou PENDING_PAYMENT.

Estados de veículo usados hoje:

- AVAILABLE
- RESERVED
- RENTED
- MAINTENANCE
- BLOCKED

Na lógica atual de busca, somente AVAILABLE entra no resultado.

## 5. Regras de Reserva

### Criação

- exige usuário autenticado.
- endDate deve ser maior que startDate.
- o usuário pode ter no máximo 5 reservas ativas.
- reserva ativa significa status CONFIRMED ou PENDING_PAYMENT.
- se não houver veículo disponível para a categoria e período, a criação falha.
- o serviço escolhe o primeiro veículo disponível encontrado.

Resultado da criação:

- status inicial: PENDING_PAYMENT.
- VehicleId é preenchido na reserva.
- TotalAmount = número de dias da reserva x DailyRate do veículo.
- o cálculo usa Math.Max(1, (endDate - startDate).Days).
- o serviço não escolhe um veículo específico enviado pelo cliente; ele seleciona internamente o primeiro disponível.

Observação importante:

- embora exista PricingRule no projeto, o valor da reserva hoje é calculado a partir de Vehicle.DailyRate.

### Consulta e atualização

- o dono da reserva pode consultar e atualizar a própria reserva.
- Admin pode consultar e atualizar qualquer reserva.
- se a reserva não existir, retorna not found.

### Cancelamento

- o dono da reserva pode cancelar a própria reserva.
- Admin pode cancelar qualquer reserva.
- não é permitido cancelar quando faltam 2 horas ou menos para StartDate.
- ao cancelar, o status vira CANCELLED.
- o cancelamento cria um pagamento adicional com status REFUNDED.
- o valor desse novo pagamento é calculado sobre Reservation.TotalAmount.

### Status de reserva

Os status válidos hoje são:

- PENDING_PAYMENT
- CONFIRMED
- CANCELLED
- EXPIRED

Transições implementadas:

1. criação -> PENDING_PAYMENT.
2. captura aprovada -> CONFIRMED.
3. cancelamento -> CANCELLED.

O status EXPIRED existe no domínio, mas não há fluxo automático de expiração implementado hoje.

## 6. Regras de Pagamento

### Pré-autorização

- exige acesso à reserva correspondente.
- cria um Payment com ReservationId, Amount informado e status PENDING.

### Captura

- exige acesso ao pagamento correspondente.
- só permite captura de pagamento com status PENDING.
- o mock decide aprovação de forma determinística:
  - se paymentId % 10 < 9, o pagamento vira APPROVED.
  - caso contrário, vira DECLINED.

Efeitos de captura aprovada:

- a reserva associada vira CONFIRMED.
- se a reserva tiver VehicleId, o veículo associado vira RESERVED.

### Refund

- exige acesso ao pagamento correspondente.
- só permite refund de pagamento com status APPROVED.
- o refund direto troca o status do próprio pagamento para REFUNDED.

### Refund gerado por cancelamento

Quando a reserva é cancelada, o sistema cria um novo Payment com:

- ReservationId da reserva cancelada.
- Amount calculado como Reservation.TotalAmount x percentual de refund.
- Status REFUNDED.

Percentual usado no cancelamento:

- mais de 7 dias: 100%
- mais de 3 dias: 80%
- mais de 1 dia: 50%
- 1 dia ou menos: 0%

Invariantes de reserva e pagamento que devem ser preservados:

- criação de reserva sempre termina em PENDING_PAYMENT.
- captura aprovada sempre muda o pagamento para APPROVED.
- captura aprovada também confirma a reserva.
- captura aprovada também marca o veículo como RESERVED, se houver VehicleId.
- refund direto não cria novo payment; ele altera o payment aprovado existente.
- refund por cancelamento cria um novo payment REFUNDED.

Matriz minima por recurso:

- User: customer lê o próprio perfil; admin lista usuários e atribui roles.
- Reservation: customer lê, atualiza e cancela a própria reserva; admin faz isso para qualquer reserva existente.
- Payment: customer opera apenas pagamentos ligados a reservas próprias; admin opera qualquer pagamento existente.
- Vehicle: público consulta catálogo; admin cria e atualiza frota.
- PricingRule: público consulta; admin cria e atualiza.

## 7. Regras de Ownership

Ownership é validado no controller, não no repositório.

Regras atuais:

- ReservationsController compara o UserId da reserva com o claim NameIdentifier.
- PaymentsController carrega o pagamento, resolve a reserva e compara o UserId da reserva com o claim NameIdentifier.
- UserController sempre usa o usuário autenticado para retornar me e me/reservations.

Consequência para reconstrução:

- ownership pertence ao fluxo HTTP e à camada de controller.
- essa regra não deve ser empurrada para seed, repository ou front como fonte de verdade.

## 8. Invariantes Arquiteturais Atuais

- mover código entre projetos não pode alterar rota, payload, regra de autorização ou semântica dos status.
- Domain não contém regra de infraestrutura.
- Application concentra regras de negócio e depende apenas de contratos e domínio.
- Infrastructure implementa persistência mockada e carga inicial sem decidir regra de negócio.
- API continua responsável por autenticação HTTP, middleware, controllers e composição de dependências.

## 8. Regras que Não Devem Ser Inventadas

Ao reconstruir a API, não introduza como se já existissem:

- role Guest.
- IDs Guid.
- captura no check-in com estado CAPTURED.
- gateway real de pagamento.
- persistência em banco.
- expiração automática de reserva.
- cálculo oficial de reserva baseado em PricingRule.

## 9. Reflexo das Regras no Front-end

O front deve materializar as regras já existentes, não reinterpretá-las.

Tudo nesta seção deve ser lido como efeito de regras existentes da API, não como regra nova de produto.

Se houver conflito entre UX e regra operacional, prevalece a regra operacional definida nas seções anteriores.

Diretriz obrigatória de arquitetura do front:

- regras de negócio devem ficar fora da camada puramente visual
- a UI deve refletir o domínio sem concentrar decisão de negócio em componentes de renderização
- componentes visuais devem consumir serviços, hooks e regras organizadas, em vez de virar a fonte principal de decisão de domínio

Isso não cria regra nova de produto. Apenas define onde a regra existente deve viver no front-end.

### 9.1 O que Customer pode ver e fazer

Customer pode:

- autenticar-se e manter sessão JWT.
- ver o próprio perfil em /users/me.
- ver as próprias reservas em /users/me/reservations.
- consultar detalhe de uma reserva se ela for dele.
- atualizar a própria reserva usando PATCH na rota existente de reservations.
- cancelar a própria reserva quando a janela de 2 horas ainda não tiver sido violada.
- usar o fluxo de pagamento para preauth, capture e refund apenas quando o backend permitir acesso ao recurso.
- navegar pelo catálogo público e iniciar uma reserva por categoria.

Customer não pode:

- acessar telas administrativas.
- ver reservas de outros usuários.
- ver pagamentos que não pertençam a uma reserva sua.

### 9.2 O que Admin pode ver e fazer

Admin pode:

- acessar dashboards administrativos.
- listar usuários.
- atribuir roles.
- criar e editar veículos.
- criar e editar pricing rules.
- acessar rotas protegidas de customer também.

Admin não ganha no front permissões que a API não possui. Exemplo:

- não existe tela para listar todas as reservas porque a API atual não expõe esse endpoint.

### 9.3 Fluxo Busca -> Reserva -> Pagamento no Front

Fluxo oficial de UX:

1. usuário busca veículos no catálogo.
2. usuário abre o detalhe de uma unidade disponível.
3. front explica que a reserva continua sendo por categoria.
4. front cria a reserva enviando apenas categoryId, startDate e endDate.
5. front redireciona para a tela de pagamento da reserva.
6. usuário executa pre-autorização.
7. se houver pagamento pendente conhecido, pode capturar.
8. se houver pagamento aprovado conhecido, pode solicitar refund.

Limite importante desse fluxo:

- o front não pode depender de um endpoint inexistente para listar pagamentos por reserva.
- quando precisar exibir pagamento associado, deve usar apenas o que foi retornado pela API ou o registro local criado no próprio fluxo.

### 9.4 Como o cancelamento deve aparecer

No front, cancelamento deve:

- estar visível apenas quando a reserva estiver em estado cancelável.
- informar claramente a regra da janela mínima de 2 horas.
- exibir feedback de sucesso ou erro vindo da API.
- refletir o novo status CANCELLED após invalidação de cache.

### 9.5 Como os status devem ser exibidos

Reserva:

- PENDING_PAYMENT: destaque de ação pendente.
- CONFIRMED: confirmação positiva.
- CANCELLED: estado encerrado por cancelamento.
- EXPIRED: estado encerrado sem ação posterior automática no front.

Pagamento:

- PENDING: ação de captura disponível quando aplicável.
- APPROVED: pagamento aprovado, podendo expor refund se permitido.
- DECLINED: falha visível com possibilidade de nova preauth.
- REFUNDED: operação encerrada com reembolso.

### 9.6 Limitação que a UX deve respeitar

Como a API atual não fornece listagem de pagamentos por reserva, o front não deve fingir que existe histórico completo de pagamentos. Ele pode:

- mostrar o pagamento resolvido por operação executada nesta sessão.
- validar correspondência segura com os dados seed quando houver paymentId igual ao reservationId e a API confirmar o vínculo.

Ele não deve:

- inventar endpoints de histórico.
- exibir um pagamento como vinculado sem validação.
