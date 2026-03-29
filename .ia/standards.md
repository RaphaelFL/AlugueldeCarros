# STANDARDS

Este documento descreve os padrões observados no código atual. Ele deve orientar reconstruções fiéis ao projeto, não impor uma arquitetura diferente.

## 1. Estrutura de Pastas

Estrutura principal existente:

- AlugueldeCarros/Configurations
- AlugueldeCarros/Controllers
- AlugueldeCarros/DTOs
- AlugueldeCarros/Exceptions
- AlugueldeCarros/Middleware
- AlugueldeCarros/Security
- AlugueldeCarros.Domain/Entities
- AlugueldeCarros.Domain/Enums
- AlugueldeCarros.Application/Contracts
- AlugueldeCarros.Application/Services
- AlugueldeCarros.Application/Security
- AlugueldeCarros.Infrastructure/Repositories
- AlugueldeCarros.Infrastructure/Loaders
- AlugueldeCarros/Resources/MockData

Pontos importantes:

- JsonDataLoader está em AlugueldeCarros.Infrastructure/Loaders.
- contratos de repository ficam na Application e implementações in-memory na Infrastructure.
- existe pasta Mappers, mas o projeto não depende pesadamente de mapeamento centralizado.
- existe pasta Integrations, mas ela não define hoje uma integração externa relevante para o fluxo principal.

Como interpretar este arquivo:

- "padrão observado" significa comportamento recorrente no código atual.
- "preservar" significa que a reconstrução deve manter isso salvo conflito direto com o código-fonte real.
- exceções já existentes no projeto podem permanecer; este documento não exige uniformização forçada.

## 2. Convenções de Nome

- classes, métodos, propriedades e enums em PascalCase.
- campos privados em _camelCase.
- interfaces com prefixo I.
- DTOs organizados por domínio dentro de DTOs.
- controllers terminam com Controller.
- services terminam com Service.
- repositórios terminam com Repository.

## 3. Padrão de Controllers

Padrão observado:

- herdam de ControllerBase.
- usam ApiController e Route fixo com api/v1/....
- retornam IActionResult.
- fazem checagens simples de autenticação e ownership no próprio controller.
- delegam a lógica central para services.

Estilo atual de retorno:

- Ok para leitura.
- CreatedAtAction para criação.
- NoContent para update sem payload.
- NotFound, Unauthorized e Forbid quando necessário.

Padrão real de erro compartilhado pelo middleware:

- respostas de exceção retornam JSON com o shape { error: "mensagem" }.
- o front deve priorizar response.data.error quando existir.
- controllers ainda podem retornar Unauthorized, Forbid e NotFound diretamente quando fazem checagens simples.

Contratos e serialização que devem ser assumidos na reconstrução:

- IDs são int em entidades, DTOs e claims operacionais.
- datas entram e saem como DateTime do ASP.NET Core padrão, sem convenção customizada documentada acima da serialização padrão.
- enums circulam diretamente em domínio e contratos sem camada extra obrigatória de tradução.
- payloads de request usam nomes de propriedades em PascalCase do backend C# quando serializados pelo padrão atual do ASP.NET Core.

## 4. Padrão de Services

Padrão observado:

- classes concretas, sem interface em vários casos.
- métodos assíncronos terminando com Async.
- validações de negócio ficam aqui.
- exceções são lançadas aqui e tratadas pelo middleware ou controller.

O que preservar:

- regras de negócio fora dos controllers.
- cálculos e mudanças de status dentro dos services.
- dependência por interfaces de repository.
- controllers dependentes de interfaces de service.

## 5. Padrão de Repositories

Padrão observado:

- interface de repository em Application/Contracts/Repositories.
- implementação in-memory em Infrastructure/Repositories.
- armazenamento em List<T>.
- AddAsync normalmente gera Id por contagem atual + 1.
- UpdateAsync normalmente remove o item existente e adiciona o atualizado de volta.

Implicações:

- a ordem interna da lista pode mudar após update.
- não há controle transacional.
- não há concorrência sofisticada.

Ponto de fidelidade importante:

- não substituir esse padrão por ORM, unit of work ou abstrações extras durante a reconstrução.

## 6. Segurança

Padrão observado:

- JwtTokenService centraliza emissão do token.
- PasswordHasher centraliza hash e verificação BCrypt.
- role admin é escrita como Admin.
- claim de identidade usado pelos controllers é ClaimTypes.NameIdentifier.

Ao reconstruir, preserve exatamente:

- roles Customer e Admin.
- uso de Authorize(Roles = "Admin").
- leitura do userId a partir do token.
- fallback de senha em texto puro apenas na autenticação, por compatibilidade com dados atuais.

## 7. Dados e Contratos

- entidades ficam em Domain/Entities.
- enums ficam em Domain/Enums.
- requests ficam em DTOs por módulo.
- nem todo endpoint usa DTO de resposta; alguns retornam entidades anônimos ou a própria entidade.

Para reconstrução fiel:

- manter DTOs de entrada por módulo.
- não forçar DTO de saída onde o projeto atual não usa.
- manter IDs como int em contratos e entidades.

Padrões obrigatórios versus padrões observados:

- obrigatório: roles Customer e Admin, JWT, ownership em controller, repositories in-memory, services concretos nos controllers.
- obrigatório: evitar EF Core, camadas extras e contratos que o projeto atual não exige.
- observado: coexistência de entidades retornadas diretamente e objetos anônimos em alguns controllers.
- observado no estado atual: contratos e implementações de repository estão em projetos separados por camada.

Contratos mínimos de resposta que devem ser preservados porque não estão todos materializados em DTO dedicado:

- POST api/v1/auth/register e POST api/v1/auth/login retornam pelo menos Token e Email.
- GET api/v1/users/me retorna Id, Email, FirstName, LastName, CreatedAt e Roles.
- GET api/v1/admin/users retorna coleção com Id, Email, FirstName, LastName, CreatedAt e Roles.
- erros tratados pelo middleware retornam JSON com a propriedade error; payload inválido de model binding retorna error e details.

Se for recriar, mantenha coerência com o estado atual em vez de forçar padronização extra.

## 8. Estilo de Implementação

O código atual favorece:

- simplicidade.
- baixo número de abstrações.
- dependência de contratos de service nos controllers.
- lógica suficiente para o mock funcional, sem infraestrutura extra.

Sinais de que a reconstrução está desalinhada:

- criação de novas camadas além das quatro já adotadas.
- migração prematura para persistência real, mediator ou use cases adicionais.
- introdução de eventos, filas, cache distribuído ou persistência externa.

Evite introduzir na reconstrução, a menos que o projeto mude de direção:

- camadas adicionais artificiais.
- patterns não usados hoje, como mediator, unit of work ou specification.
- abstrações de persistência mais complexas que os repositórios atuais.

## 9. Regra de Fidelidade

Quando houver conflito entre este documento e uma ideia de "melhor prática", preserve o comportamento existente do projeto.

Prioridade de referência:

1. código-fonte atual.
2. regras documentadas em business-rules.md.
3. decisões arquiteturais em architecture.md.
4. stack descrita em tech-stack.md.

## 10. Convenções do Front-end

O front oficial usa React com TypeScript na pasta frontend.

### 10.1 Organização de Arquivos

Padrão adotado:

- app para providers globais.
- api para cliente HTTP e serviços.
- components para UI reutilizável.
- hooks para lógica reusável de tela.
- layouts para cascas de navegação.
- pages para telas por domínio.
- routes para configuração de rotas e guards.
- store para sessão persistida.
- types para contratos da aplicação.
- lib para utilitários puros.

### 10.2 Convenções de Nome

Padrão adotado no front:

- componentes React em PascalCase.
- hooks com prefixo use.
- arquivos utilitários e stores em kebab-case.
- páginas agrupadas por domínio funcional.

Exemplos:

- use-auth.ts
- auth-store.ts
- public-pages.tsx
- admin-pages.tsx

Convenção oficial para novos componentes, features e refatorações do front:

- `ComponentName.view.tsx` para estrutura visual
- `ComponentName.logic.ts` para lógica da feature/componente
- `ComponentName.css` para estilo dedicado
- `useFeatureName.ts` para hooks reutilizáveis
- `feature-name.service.ts` para integrações e serviços do front

Se outro padrão equivalente for usado, ele só é aceito quando mantiver separação real entre view, lógica e estilo.

### 10.3 Padrão de Componentes

Componentes devem:

- ser orientados a composição.
- receber props explícitas.
- evitar lógica de API dentro de primitives de UI.
- concentrar aparência consistente em componentes reutilizáveis como Button, Card, Badge e Field.
- manter o markup principal em arquivo de view/template dedicado quando a complexidade justificar.
- evitar concentrar renderização, estado complexo, integração e estilo no mesmo arquivo.

Componentes não devem:

- concentrar regra de negócio de domínio na camada de view
- acumular handlers extensos, chamadas de serviço e markup grande no mesmo `.tsx`
- crescer indefinidamente como arquivos monolíticos de interface

### 10.4 Padrão de Páginas

Páginas devem:

- consumir serviços através de TanStack Query e mutations.
- exibir loading, erro, sucesso e empty state.
- manter formulários e ações próximos do contexto da tarefa.
- não duplicar regras de domínio já descritas no backend.

Para novas telas e refatorações estruturais, páginas devem ser separadas por responsabilidade:

- view em `.tsx`
- lógica em `.ts`, hooks ou services
- estilo em `.css`

Padrão esperado para reprodução:

- os agrupadores atuais `public-pages.tsx`, `user-pages.tsx` e `admin-pages.tsx` pertencem ao estado atual observado, mas não devem servir como justificativa para novos arquivos gigantes.
- a arquitetura oficial futura deve quebrar telas e componentes em unidades menores por feature e responsabilidade.
- rotas principais devem continuar lazy-loaded.
- login, cadastro e reidratação da sessão devem resolver o usuário autenticado usando o token mais recente, sem depender apenas do store já hidratado.

### 10.5 Padrão de Tratamento de Erro e Loading

No front:

- erros da API devem priorizar response.data.error.
- loading states precisam ser visíveis e não bloquear a navegação inteira sem necessidade.
- estados vazios devem explicar a ausência de dados e apontar próxima ação.
- um 401 em /api/v1/users/me logo após login normalmente indica fluxo incorreto de bootstrap da sessão, não ausência de permissão de negócio.

### 10.6 Padrão de Formulários

Formulários oficiais do front usam:

- react-hook-form para controle.
- zod para validação.
- mensagens de erro curtas e orientadas à ação.

Regras obrigatórias de separação para formulários:

- schema e validação ficam em lógica TypeScript dedicada quando o formulário crescer além do trivial
- markup do formulário fica na view
- estilo do formulário fica em arquivo de estilo dedicado ou camada de estilo explicitamente isolada

### 10.7 Padrão de Hooks, Services e Estilos

Hooks devem:

- encapsular lógica reutilizável de tela ou feature
- expor estado derivado e ações sem acoplar markup
- permanecer em arquivos `.ts`

Services do front devem:

- encapsular acesso HTTP, serialização e integração com a API
- não renderizar UI
- não carregar estrutura visual

Estilos dedicados devem:

- ficar em arquivo `.css` próprio da feature, componente ou view quando houver estilo específico
- evitar mistura de decisão de negócio com definição visual
- servir como camada isolada de apresentação, mesmo quando a base usar Tailwind como utilitário

### 10.8 Regra Anti-Arquivo-Gigante

Arquivos de front-end não devem crescer a ponto de misturar:

- interface
- comportamento
- integração
- validação extensa
- estilo

Quando isso acontecer, a refatoração esperada é separar por responsabilidade. O objetivo é manter componentes legíveis, reutilizáveis e escaláveis.

Formulários críticos:

- login e cadastro.
- criação de reserva.
- remarcação de reserva.
- create/update de veículo.
- create/update de pricing rule.

### 10.7 Padrão de Rotas Protegidas

Rotas devem ser protegidas em dois níveis:

- autenticação com RequireAuth.
- autorização administrativa com RequireAdmin.

Além disso:

- menus e CTAs devem respeitar role.
- front não deve mostrar atalhos administrativos para Customer.

### 10.8 Padrão de Design System

O visual do front deve seguir:

- linguagem visual consistente entre áreas pública, customer e admin.
- tipografia baseada nas fontes já escolhidas no projeto.
- cards, badges, headers e feedbacks reutilizáveis.
- consistência entre área pública, customer e admin.

Este bloco orienta consistência visual, não autoriza mudar o produto para outro estilo de arquitetura front-end.
