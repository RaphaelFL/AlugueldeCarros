# STANDARDS

Este documento descreve os padrões observados no código atual. Ele deve orientar reconstruções fiéis ao projeto, não impor uma arquitetura diferente.

## 1. Estrutura de Pastas

Estrutura principal existente:

- Configurations
- Controllers
- Domain/Entities
- Domain/Enums
- DTOs
- Exceptions
- Integrations
- Loaders
- Mappers
- Middleware
- Repositories
- Resources/MockData
- Security
- Services
- Validations

Pontos importantes:

- JsonDataLoader está em Loaders.
- repositórios e implementações in-memory dividem o mesmo arquivo em vários casos.
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
- services concretos injetados diretamente nos controllers.

## 5. Padrão de Repositories

Padrão observado:

- interface e implementação in-memory no mesmo arquivo em diversos módulos.
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
- observado: interface e implementação de repository no mesmo arquivo em vários módulos.

Se for recriar, mantenha coerência com o estado atual em vez de forçar padronização extra.

## 8. Estilo de Implementação

O código atual favorece:

- simplicidade.
- baixo número de abstrações.
- dependência direta de services concretos nos controllers.
- lógica suficiente para o mock funcional, sem infraestrutura extra.

Sinais de que a reconstrução está desalinhada:

- criação de interfaces de service sem necessidade real.
- criação de camadas Application, Domain Services ou Use Cases que hoje não existem.
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

### 10.3 Padrão de Componentes

Componentes devem:

- ser orientados a composição.
- receber props explícitas.
- evitar lógica de API dentro de primitives de UI.
- concentrar aparência consistente em componentes reutilizáveis como Button, Card, Badge e Field.

### 10.4 Padrão de Páginas

Páginas devem:

- consumir serviços através de TanStack Query e mutations.
- exibir loading, erro, sucesso e empty state.
- manter formulários e ações próximos do contexto da tarefa.
- não duplicar regras de domínio já descritas no backend.

Padrão esperado para reprodução:

- cada área principal deve continuar agrupada em public-pages.tsx, user-pages.tsx e admin-pages.tsx.
- rotas principais devem continuar lazy-loaded.

### 10.5 Padrão de Tratamento de Erro e Loading

No front:

- erros da API devem priorizar response.data.error.
- loading states precisam ser visíveis e não bloquear a navegação inteira sem necessidade.
- estados vazios devem explicar a ausência de dados e apontar próxima ação.

### 10.6 Padrão de Formulários

Formulários oficiais do front usam:

- react-hook-form para controle.
- zod para validação.
- mensagens de erro curtas e orientadas à ação.

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
