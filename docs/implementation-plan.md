# Plano de implementação do LocalScore

## Como usar este documento

Este é o ponto de referência para o escopo, a ordem de implementação, as decisões aprovadas e as pendências do LocalScore.

- Cada etapa deve ser refinada antes de ser implementada.
- Cada etapa deve, preferencialmente, ser tratada em uma conversa separada.
- Ao aprovar ou alterar uma regra, atualize a etapa correspondente neste arquivo.
- Não avance para a etapa seguinte sem solicitação explícita do usuário.
- Decisões ainda não tomadas devem permanecer marcadas como pendentes; não devem ser presumidas durante a implementação.

## Estado geral

| Etapa | Tema | Estado |
|---|---|---|
| 1 | Fundação do projeto | Concluída |
| 2 | Contas e autenticação | Concluída |
| 3 | Campeonatos e equipes | Concluída |
| 4 | Jogadores e inscrições | Planejada; requer refinamento |
| 5 | Rodadas e partidas | Planejada; requer refinamento |
| 6 | Eventos manuais da partida | Planejada; requer refinamento |
| 7 | Área pública e estatísticas básicas | Planejada; requer refinamento |
| 8 | Qualidade, documentação e entrega | Planejada; requer refinamento |

## Objetivo do produto

O LocalScore será uma aplicação web responsiva para organizar campeonatos amadores de futebol. Usuários cadastrados poderão criar e administrar seus próprios campeonatos. Visitantes poderão acompanhar informações públicas como equipes, jogadores, rodadas, partidas, placares, classificação e artilharia.

O projeto também é destinado a estudo e portfólio. A arquitetura deve ser clara e testável, sem padrões ou infraestrutura que não tragam benefício concreto ao estágio atual.

## Escopo consolidado do MVP

### Incluído

- cadastro e login de contas comuns;
- cada usuário gerencia somente os campeonatos que criou;
- criação e manutenção de campeonatos;
- cadastro de equipes e inscrição no campeonato;
- cadastro de jogadores e vínculo com uma equipe no campeonato;
- criação manual de rodadas;
- criação manual de partidas;
- alteração manual dos status de campeonatos e partidas;
- placar informado manualmente;
- registro manual de gols, cartões amarelos, cartões vermelhos e substituições;
- consulta pública de campeonatos, equipes, jogadores, rodadas, partidas e placares;
- classificação básica por pontos;
- artilharia baseada nos gols registrados.

### Fora do MVP

- escalações;
- geração automática de rodadas e confrontos;
- fases, grupos e mata-mata;
- critérios esportivos de desempate;
- roles e permissões compartilhadas por campeonato;
- convites e organizações;
- confirmação obrigatória de e-mail;
- recuperação de senha e edição de perfil;
- assistências e gol contra como regras específicas;
- suspensões automáticas e limites de substituição;
- atualização em tempo real;
- notificações;
- aplicativo móvel;
- uploads de imagens;
- cadastro separado de locais;
- Docker, até nova autorização;
- Testcontainers, enquanto Docker estiver adiado.

## Arquitetura aprovada

### Repositório

Frontend e backend permanecem no mesmo repositório:

```text
localscore/
├── frontend/
├── backend/
│   ├── LocalScore.sln
│   └── src/
│       ├── LocalScore.Api/
│       ├── LocalScore.Application/
│       ├── LocalScore.Domain/
│       └── LocalScore.Infrastructure/
├── docs/
│   └── implementation-plan.md
├── AGENTS.md
└── README.md
```

### Backend

- `LocalScore.Domain`: entidades e regras de negócio; não depende das demais camadas.
- `LocalScore.Application`: casos de uso, contratos internos, validações e abstrações necessárias pela aplicação.
- `LocalScore.Infrastructure`: EF Core, PostgreSQL, persistência e implementações externas.
- `LocalScore.Api`: controllers, contratos HTTP, autenticação do pipeline, middlewares e composição da injeção de dependência.

Direção das referências:

```text
Api → Application
Api → Infrastructure
Infrastructure → Application
Infrastructure → Domain
Application → Domain
Domain → nenhuma camada do projeto
```

O `Program.cs` é a raiz de composição. Registros futuros deverão ser agrupados por extensões como `AddApplication()` e `AddInfrastructure()`, evitando listar todos os serviços diretamente no arquivo.

### Frontend

- Angular com componentes standalone;
- roteamento com carregamento por funcionalidade quando necessário;
- SCSS;
- Signals para estado local e compartilhado simples;
- RxJS para operações assíncronas e composição HTTP;
- sem NgRx enquanto não existir necessidade demonstrada;
- organização por features conforme as telas de negócio forem criadas.

## Decisões técnicas transversais

- Angular 20 e .NET 9 foram mantidos por já estarem instalados.
- Planejar atualização antes de um deploy de produção posterior a novembro de 2026.
- API baseada em controllers.
- DTOs de entrada e saída; entidades de domínio não são expostas diretamente.
- Falhas esperadas usam `Result` ou `Result<T>` na camada Application.
- Sucessos HTTP retornam o DTO diretamente, sem envelope universal `ApiResponse<T>`.
- Erros HTTP usam `ProblemDetails` com `code` e `traceId`.
- Paginação futura usa `PagedResponse<T>` porque possui metadados próprios.
- Exceptions inesperadas são tratadas globalmente e não representam fluxo normal de negócio.
- Datas e horas que representam instantes são armazenadas em UTC.
- Não introduzir repository genérico, CQRS, MediatR, microsserviços ou mensageria sem necessidade concreta e nova decisão.
- Segredos nunca são versionados nem incluídos em logs.

## Decisões de persistência

- PostgreSQL como banco relacional.
- Entity Framework Core com provider Npgsql.
- Um único `LocalScoreDbContext` no MVP.
- Contexto, mapeamentos e migrations em `LocalScore.Infrastructure/Persistence`.
- Entidades de negócio em `LocalScore.Domain`.
- Fluent API com uma configuração por entidade.
- Schema padrão `public`.
- Tabelas e colunas em `snake_case`.
- UUID para entidades principais.
- Inteiro para códigos de tabelas de referência.
- Relacionamentos com histórico preferem `Restrict`/`NoAction`, evitando cascatas amplas.
- Migrations são versionadas e aplicadas manualmente.
- Não usar `EnsureCreated` nem `Database.Migrate()` no startup.
- Desenvolvimento local usa o database PostgreSQL `localscore` já instalado na máquina.
- A conexão local usará o usuário administrativo existente, por decisão do usuário; isso deve ser revisto antes da produção.
- Connection string e chave JWT ficam em ASP.NET Core User Secrets no desenvolvimento.
- Docker não será adicionado até nova autorização.
- Testes automatizados não podem limpar o database local compartilhado.

## Convenção para tabelas de referência

Status e tipos terão tabelas separadas, por exemplo:

- `championship_statuses`;
- `match_statuses`;
- `match_event_types`.

Regras:

- `code` é numérico, estável e pode ser a chave primária;
- `description` é usada para apresentação;
- enums C# usam exatamente os mesmos valores numéricos;
- códigos removidos não são reutilizados com outro significado;
- dados são inseridos por migration e controlados pela aplicação;
- não haverá CRUD administrativo dessas referências no MVP;
- não será criada uma tabela genérica única para status de conceitos diferentes.

---

# Etapa 1 — Fundação do projeto

**Estado:** concluída.

## Objetivo

Criar uma base executável para frontend e backend, sem antecipar autenticação ou domínio.

## Funcionalidades entregues

- monorepositório com `frontend` e `backend`;
- solução Visual Studio;
- projetos Api, Application, Domain e Infrastructure;
- aplicação Angular standalone com roteamento e SCSS;
- OpenAPI em desenvolvimento;
- health check da API em `/health`;
- logging inicial em console;
- Git local na branch `main`;
- configurações de formatação e arquivos ignorados.

## Frontend

- Angular 20;
- página inicial de fundação;
- teste inicial do componente raiz.

## Backend

- ASP.NET Core Web API em .NET 9;
- controllers habilitados;
- OpenAPI;
- health check;
- referências entre projetos respeitando a direção arquitetural.

## Banco de dados

Nenhuma implementação. PostgreSQL, EF Core, contexto e migrations foram deliberadamente adiados.

## Dependências

Nenhuma.

## Critérios atendidos

- backend compilou sem erros ou avisos;
- frontend gerou build de produção;
- testes iniciais do Angular passaram;
- `/health` respondeu `200 Healthy`;
- artefatos de build e dependências estão ignorados pelo Git.

## Observações

- Docker chegou a ser considerado durante a fundação, mas foi removido por decisão do usuário.
- Não há remote Git configurado obrigatoriamente e nenhum commit deve ser presumido.

---

# Etapa 2 — Contas e autenticação

**Estado:** concluída.

## Objetivo

Permitir cadastro, login, restauração de sessão, renovação e logout de contas comuns. Preparar a identidade usada futuramente para limitar alterações ao proprietário de cada campeonato.

## Escopo funcional

- cadastro público;
- login por e-mail e senha;
- login automático após cadastro;
- access token JWT;
- refresh token em cookie seguro;
- múltiplas sessões por usuário;
- renovação automática no Angular;
- logout da sessão atual;
- consulta dos dados do usuário atual;
- proteção inicial contra tentativas automatizadas.

Não inclui edição de perfil, alteração de senha, recuperação de senha, exclusão de conta, confirmação obrigatória de e-mail ou tela de dispositivos.

## Decisões de conta

- contas comuns, sem role `Admin` no MVP;
- cada usuário gerenciará apenas os campeonatos que criar;
- campos funcionais: `id`, `name`, `email`, `created_at`, `updated_at` e indicador técnico de atividade;
- e-mail é usado também como username interno;
- sem username público separado, telefone, foto ou data de nascimento;
- nome obrigatório entre 2 e 100 caracteres, com espaços externos removidos;
- nomes aceitam acentos, espaços, hífen e apóstrofo;
- e-mail obrigatório com até 254 caracteres;
- e-mail normalizado e único sem diferenciação de maiúsculas;
- confirmação da senha existe somente no formulário Angular;
- confirmação de e-mail fica preparada, mas não é exigida;
- campos técnicos padrão de telefone e 2FA do Identity permanecem no banco, embora não sejam expostos ou utilizados no MVP.

## Identity e senha

- ASP.NET Core Identity Core sem roles;
- infraestrutura do Identity usada para hash, verificação, normalização, bloqueio e futuros tokens de conta;
- telas e endpoints serão próprios do LocalScore;
- senha entre 8 e 128 caracteres;
- exige ao menos uma letra minúscula e um número;
- não exige maiúscula ou caractere especial;
- bloqueio após 5 tentativas consecutivas inválidas;
- bloqueio por 15 minutos;
- login válido zera o contador;
- login inválido usa mensagem genérica para não confirmar a existência da conta.

## JWT

- access token JWT armazenado no `localStorage`;
- chave simétrica HMAC SHA-256 com ao menos 256 bits;
- chave em User Secrets no desenvolvimento;
- validade de 15 minutos;
- validação de assinatura, emissor, destinatário e expiração;
- claims: `sub`, `email`, `name`, `jti`, `sid`, `iat`, `nbf`, `exp`, `iss` e `aud`;
- sem roles, permissões, campeonatos ou dados sensíveis no token;
- access token revogado indiretamente permanece válido por no máximo 15 minutos, sem consulta ao banco em cada requisição.

## Refresh token e sessão

- refresh token aleatório e opaco;
- valor original existe apenas no cookie e durante a requisição necessária;
- banco armazena apenas hash SHA-256 binário (`bytea`, 32 bytes);
- validade e duração máxima da sessão: 7 dias a partir do login;
- renovação não estende o limite máximo da sessão;
- sem opção “lembrar de mim”;
- rotação a cada uso;
- token anterior é revogado e aponta para o substituto;
- reutilização fora do fluxo esperado revoga a família da sessão;
- múltiplos logins criam famílias independentes;
- logout revoga apenas a família atual;
- `token_family_id` UUID agrupa tokens originados pelo mesmo login;
- sem tabela `user_sessions` no MVP;
- tokens são mantidos por 7 dias após `expires_at`;
- login e refresh removem oportunisticamente tokens antigos do usuário;
- não haverá rotina global em segundo plano nesta etapa.

Estrutura aprovada de `refresh_tokens`:

| Coluna | Finalidade |
|---|---|
| `id` | UUID do registro |
| `user_id` | Usuário proprietário |
| `token_family_id` | Sessão lógica |
| `token_hash` | SHA-256 binário e único |
| `created_at` | Criação |
| `expires_at` | Limite da sessão |
| `revoked_at` | Revogação, quando houver |
| `revocation_reason` | Motivo da revogação |
| `replaced_by_token_id` | Token que substituiu o atual |

Índices: único em `token_hash`, e índices em `user_id`, `token_family_id` e `expires_at`. Não persistir IP, localização, user agent ou nome de dispositivo.

## Cookie de refresh

```text
Name: LocalScore.RefreshToken
HttpOnly: true
Secure: true
SameSite: Strict
Path: /api/v1/auth
Max-Age: 7 dias
Domain: não definido
```

- Angular e API serão tratados como mesma origem;
- desenvolvimento usa proxy Angular para `/api`;
- produção deve preservar a mesma origem por hospedagem conjunta ou proxy reverso;
- HTTPS é obrigatório;
- logout expira explicitamente o cookie.

## Endpoints e contratos

```text
POST /api/v1/auth/register
POST /api/v1/auth/login
POST /api/v1/auth/refresh
POST /api/v1/auth/logout
GET  /api/v1/auth/me
```

- `register`: cria conta, inicia sessão, grava cookie e retorna `201 Created`;
- `login`: valida credenciais, inicia sessão, grava cookie e retorna `200 OK`;
- `refresh`: lê somente o cookie, rotaciona o token e retorna `200 OK`;
- `logout`: revoga a família atual, remove cookie e retorna `204 No Content`; é idempotente;
- `me`: exige bearer token e retorna `200 OK`.

Cadastro, login e refresh retornam diretamente:

```json
{
  "accessToken": "jwt",
  "accessTokenExpiresAt": "date-time",
  "sessionExpiresAt": "date-time",
  "user": {
    "id": "uuid",
    "name": "Nome",
    "email": "email@example.com"
  }
}
```

`me` retorna apenas `id`, `name` e `email`. Refresh token, campos técnicos e detalhes internos nunca aparecem no JSON.

## Respostas, erros e exceptions

- `Result`/`Result<T>` representa sucessos e falhas esperadas na Application;
- tipos previstos: Validation, Unauthorized, Forbidden, NotFound e Conflict;
- a API converte esses tipos para status HTTP e `ProblemDetails`;
- validação usa Data Annotations inicialmente, sem FluentValidation;
- `ProblemDetails` contém `code`, `traceId` e, quando aplicável, erros por campo;
- um `IExceptionHandler` próprio é conectado ao middleware `UseExceptionHandler`;
- exceptions inesperadas retornam sempre uma mensagem genérica, inclusive em Development;
- stack trace, SQL, caminhos e configurações permanecem somente nos logs;
- cancelamento do cliente não retorna `500` nem é registrado como erro crítico;
- exceptions não são usadas como fluxo de negócio.

## Rate limiting

- cadastro: 5 requisições por minuto por IP;
- login: 10 por minuto por IP;
- refresh: 30 por minuto por IP;
- implementação nativa e em memória;
- excesso retorna `429 Too Many Requests`, `Retry-After` e `ProblemDetails` com código `rate_limit_exceeded`;
- limitação distribuída fica fora do MVP.

## Persistência da etapa

- um único `LocalScoreDbContext` baseado no contexto de usuários do Identity sem roles;
- schema `public`;
- tabelas personalizadas em `snake_case`:
  - `users`;
  - `user_claims`;
  - `user_logins`;
  - `user_tokens`;
  - `refresh_tokens`;
- tabelas de roles não são criadas;
- primeira migration contém somente autenticação;
- migrations e configurações ficam em `LocalScore.Infrastructure/Persistence`;
- API é o startup project das ferramentas EF;
- não criar `IDesignTimeDbContextFactory` sem necessidade comprovada;
- migration aplicada manualmente no database local `localscore`;
- User Secrets fornece `ConnectionStrings:DefaultConnection` e `Jwt:SigningKey`;
- parâmetros JWT não sensíveis permanecem em `appsettings`.

## Frontend da etapa

Rotas:

```text
/           página pública inicial
/login      login
/register   cadastro
/app        área autenticada inicial
```

Regras:

- login e cadastro redirecionam para `/app`;
- acesso anônimo a `/app` redireciona para `/login`;
- usuário autenticado em `/login` ou `/register` é redirecionado para `/app`;
- logout redireciona para `/login`;
- `/app` mostra inicialmente nome, e-mail, estado autenticado e ação de logout;
- somente `LocalScore.AccessToken` é armazenado no `localStorage`;
- dados do usuário ficam em Signal e são restaurados por `/auth/me`;
- claim `exp` é lido com APIs nativas, sem biblioteca externa;
- decodificar no frontend não significa validar o token;
- token malformado ou sem `exp` é descartado;
- na inicialização, access token válido é confirmado por `/me`; token expirado tenta refresh;
- interceptor adiciona bearer token e trata `401`;
- somente um refresh ocorre por vez dentro da aplicação;
- requisições aguardam o mesmo refresh e são repetidas no máximo uma vez;
- falha de refresh limpa estado e redireciona para login;
- abas coordenam a renovação; uma aba atualiza o access token compartilhado e as demais aguardam;
- falha de coordenação pode encerrar a sessão para preservar a rotação estrita;
- sem temporizador permanente de renovação.

## Estratégia de testes da etapa

Automatizar sem banco destrutivo:

- `Result` e mapeamento de erros;
- handler global;
- geração e claims do JWT;
- hash e geração de refresh token;
- regras de rotação que possam ser isoladas;
- parser de expiração no Angular;
- estado de autenticação, guardas e interceptor;
- validações dos formulários.

Validar manualmente no PostgreSQL local:

- aplicação da migration;
- cadastro e login automático;
- e-mail duplicado;
- credenciais inválidas;
- bloqueio após cinco tentativas;
- `/me` autenticado;
- refresh e rotação;
- reutilização de token;
- múltiplas sessões;
- logout;
- rate limiting.

Ficam adiados testes de integração automatizados que limpem o banco, Testcontainers e E2E completo. O database local compartilhado nunca deve ser recriado ou limpo por testes automatizados.

## Incrementos da etapa 2

### 2.1 — Fundamentos de aplicação e erros

**Estado:** concluído.

Implementar `Result`, `Result<T>`, erros de aplicação, conversão para `ProblemDetails`, handler global, `traceId` e testes. Não adicionar banco ou Identity.

**Concluído quando:** falhas esperadas e inesperadas têm contratos distintos e testados; backend compila sem erros.

### 2.2 — PostgreSQL, EF Core e Identity

**Estado:** concluído.

Adicionar pacotes, User Secrets, `ApplicationUser`, `LocalScoreDbContext`, mapeamentos, política de senha/bloqueio e registros de DI. Não criar endpoints ou JWT.

**Concluído quando:** contexto é descoberto pelo EF, configurações sensíveis não são versionadas e backend compila.

### 2.3 — Refresh tokens e primeira migration

**Estado:** concluído; migration aplicada e validada no PostgreSQL local.

Implementar a entidade, hash, família, rotação persistente, índices e primeira migration. Aplicar manualmente e inspecionar o PostgreSQL local.

**Concluído quando:** somente tabelas de autenticação existem e a migration foi validada.

### 2.4 — JWT e serviços de autenticação

**Estado:** concluído com testes unitários sem banco.

Implementar emissão, claims, sessão, rotação, detecção de reutilização, múltiplas famílias e limpeza oportunística, com testes unitários.

**Concluído quando:** regras de token e sessão estão testadas sem expor segredos.

### 2.5 — Endpoints da API

**Estado:** concluído; fluxos validados manualmente no PostgreSQL local.

Implementar os cinco endpoints, cookie, validações, rate limiting e autorização bearer.

**Concluído quando:** contratos e fluxos funcionam manualmente no banco local.

### 2.6 — Infraestrutura de autenticação no Angular

**Estado:** concluído.

Implementar proxy, cliente HTTP, estado, armazenamento, parser, interceptor, refresh, coordenação entre abas e guardas.

**Concluído quando:** sessão é restaurada e renovada, e rotas protegidas funcionam corretamente.

### 2.7 — Telas

**Estado:** concluído.

Implementar login, cadastro e `/app` com formulários reativos, feedback, loading, erros por campo e responsividade.

**Concluído quando:** fluxo completo pode ser utilizado em celular e desktop.

### 2.8 — Validação e documentação

**Estado:** concluído.

Executar builds, testes, roteiro manual, revisão de segurança e atualização documental.

**Concluído quando:** todos os critérios aprovados passam e pendências ficam registradas.

## Registro da implementação — 12 de setembro de 2026

Implementado:

- `Result`, `Result<T>`, tipos de erro e mapeamento para `ProblemDetails`;
- handler global baseado em `IExceptionHandler`, sem exposição de detalhes internos;
- Identity Core sem roles, `LocalScoreDbContext`, PostgreSQL/Npgsql e mapeamentos em `snake_case`;
- refresh tokens com hash SHA-256, rotação, famílias, detecção de reutilização, múltiplas sessões, logout da família atual e limpeza oportunística;
- migration `InitialAuthentication`, contendo somente `users`, `user_claims`, `user_logins`, `user_tokens` e `refresh_tokens`;
- emissão e validação de JWT, cookie HttpOnly e os cinco endpoints aprovados;
- rate limiting em memória para cadastro, login e refresh;
- estado Angular com Signals, parser de expiração, interceptor, guards, renovação coordenada e proxy de desenvolvimento;
- páginas responsivas pública, de cadastro, login e área autenticada;
- testes unitários backend sem banco e testes Angular de parser, formulários e interceptor;
- documentação da stack, execução, endpoints e verificações no `README.md`; instruções temporárias de configuração foram removidas após a validação local.

Validações concluídas:

- solution backend compilada sem erros ou avisos;
- 11 testes backend aprovados;
- build de produção Angular aprovado;
- 9 testes Angular aprovados;
- API inicializada com configuração temporária sem acesso ao banco;
- `/health` retornou `200` e `/auth/me` anônimo retornou `401` em `ProblemDetails`;
- request de cadastro vazio retornou `400` com erros por campo;
- a sexta tentativa de cadastro no mesmo minuto retornou `429` com `rate_limit_exceeded`.

## Validação final — 19 de setembro de 2026

- User Secrets de desenvolvimento configurados sem versionar a connection string ou a chave JWT;
- migration `20260912175218_InitialAuthentication` confirmada no histórico do PostgreSQL;
- tabelas de autenticação criadas no database `localscore`, conforme inspeção do usuário;
- cadastro com login automático e cookie seguro validado;
- e-mail duplicado retornou `409 Conflict`;
- credenciais inválidas retornaram `401 Unauthorized` sem revelar a existência da conta;
- cinco falhas consecutivas bloquearam a conta, inclusive para uma senha posteriormente correta;
- `/me` autenticado retornou o usuário esperado;
- refresh rotacionou o token e invalidou o token anterior;
- reutilização do token anterior revogou a família correspondente;
- uma segunda família permaneceu válida após a revogação da primeira;
- logout revogou a sessão atual, removeu sua capacidade de refresh e permaneceu idempotente;
- access token já emitido continuou válido até sua expiração, conforme a decisão arquitetural;
- atributos `HttpOnly`, `Secure`, `SameSite=Strict` e `Path=/api/v1/auth` foram confirmados no cookie;
- rate limiting havia sido validado anteriormente com retorno `429` e código `rate_limit_exceeded`;
- 11 testes backend e 9 testes Angular passaram novamente;
- solution backend e build de produção Angular concluíram com sucesso;
- telas públicas, login, cadastro e área autenticada foram revisadas durante a validação da etapa;
- os testes finais criaram duas contas claramente identificadas como dados de teste e não limparam nem recriaram o database compartilhado.

Com essas verificações, todos os critérios aprovados para a etapa 2 foram atendidos.

## Dependências

- etapa 1 concluída;
- PostgreSQL local acessível;
- credenciais configuradas pelo usuário em User Secrets antes de aplicar a migration;
- nenhuma dependência de Docker.

## Critério final da etapa 2

Um usuário consegue se cadastrar, entrar automaticamente, recarregar a aplicação, acessar `/app`, renovar a sessão, usar múltiplas sessões e sair. Falhas retornam contratos consistentes, segredos não vazam, a migration foi validada manualmente e nenhuma tabela de domínio foi criada.

---

# Etapa 3 — Campeonatos e equipes

**Estado:** concluída.

## Objetivo

Permitir que um usuário autenticado crie e administre seus próprios campeonatos, mantenha as equipes sob sua responsabilidade e controle quais delas participam de cada campeonato. Disponibilizar a primeira consulta pública de um campeonato quando ele atender às regras de publicação aprovadas.

A etapa deve estabelecer uma base consistente para jogadores, rodadas, partidas e estatísticas, sem antecipar essas funcionalidades.

## Escopo aprovado

### Incluído

- criação, consulta, edição e tratamento de exclusão de campeonatos;
- vínculo obrigatório entre campeonato e proprietário;
- ciclo de vida e publicação do campeonato;
- criação, consulta, edição e tratamento de exclusão de equipes;
- inscrição, listagem e remoção controlada de equipes participantes;
- painel administrativo com os campeonatos do usuário;
- detalhe administrativo do campeonato e gerenciamento de participantes;
- página pública inicial com dados básicos e equipes participantes;
- autorização por proprietário em todas as operações administrativas;
- migration própria e testes das regras aprovadas.

### Fora da etapa

- jogadores e elencos, reservados para a etapa 4;
- rodadas, partidas, placares e agenda, reservados para a etapa 5;
- eventos, classificação e artilharia;
- compartilhamento de administração, convites, organizações ou roles por campeonato;
- escudos, capas e uploads;
- fases, grupos, mata-mata, geração de confrontos ou regulamentos configuráveis;
- importação, exportação e clonagem;
- área pública completa de descoberta, que permanece na etapa 7.

## Decisões já aprovadas

- somente usuários autenticados criam e administram campeonatos e equipes;
- cada usuário administra somente os campeonatos que criou;
- cada equipe pertence a um usuário e pode ser reutilizada em vários campeonatos desse mesmo proprietário;
- equipes não são compartilhadas entre usuários;
- todo campeonato possui `owner_user_id` imutável;
- campeonato possui nome, slug, status, descrição opcional e datas inicial e final opcionais;
- datas inicial e final representam dias planejados, sem horário ou fuso, e a data final não pode anteceder a inicial;
- campeonato não possui campo de localidade geral nesta etapa;
- equipe possui nome, nome curto, sigla, cidade, cor principal e cor secundária;
- o ciclo do campeonato possui somente os status `Draft`, `Published` e `Finished`;
- os códigos são `1 = Draft`, `2 = Published` e `3 = Finished` e não podem ser reutilizados com outro significado;
- `Published` representa todo o período em que o campeonato está público e ativo;
- não haverá status `InProgress`, `Cancelled` ou `Archived` nesta etapa;
- as transições permitidas são `Draft → Published`, `Published → Draft`, `Published → Finished` e `Finished → Published`;
- `Published → Draft` será bloqueada quando existirem rodadas ou partidas;
- não existem transições diretas entre `Draft` e `Finished`;
- retornos de status exigem confirmação na interface;
- publicar exige nome e slug válidos e, quando informadas, datas coerentes;
- publicação não exige quantidade mínima de equipes;
- a visibilidade pública é determinada pelo status, sem campo `is_public` separado;
- `Draft` é privado; `Published` e `Finished` são públicos;
- o slug é globalmente único, gerado do nome em ASCII minúsculo com hífens e pode ser ajustado manualmente em `Draft` antes da primeira publicação;
- colisões de slug gerado recebem sufixo numérico crescente, como `copa-do-bairro-2`;
- uma constraint única no banco é a garantia final contra colisões concorrentes;
- depois da primeira publicação, o slug é imutável mesmo que o nome do campeonato seja alterado;
- nomes de campeonato podem se repetir;
- nomes de equipe são únicos por proprietário, ignorando maiúsculas, minúsculas e espaços externos;
- remover uma equipe do campeonato preenche `championship_teams.removed_at` sem apagar o vínculo;
- reinscrever a mesma equipe reativa o vínculo existente, preservando seu identificador;
- equipes removidas não aparecem na lista pública atual, permanecem visíveis na administração e continuam disponíveis em registros históricos que as referenciem;
- campeonato só pode ser excluído fisicamente quando estiver em `Draft` e não possuir qualquer vínculo de participação ou outra dependência;
- equipe só pode ser excluída fisicamente quando nunca tiver sido inscrita em campeonato;
- não haverá soft delete nem arquivamento nesta etapa;
- campeonato não possui limite máximo de equipes no MVP;
- participantes podem ser adicionados ou removidos em `Draft` e `Published`;
- em `Finished`, participantes não podem ser alterados sem reabrir o campeonato;
- a etapa 5 definirá o bloqueio necessário para remover equipes envolvidas em partidas pendentes;
- participantes são ordenados por nome, ignorando maiúsculas e acentos; `registered_at` não determina a ordem padrão;
- recurso administrativo inexistente ou pertencente a outro usuário retorna igualmente `404 Not Found`;
- edições usam a política de última gravação válida; não haverá token de concorrência otimista nesta etapa;
- `updated_at` é informativo e não funciona como versão de concorrência;
- listas administrativas não são paginadas nesta etapa e possuem ordenação determinística;
- campeonatos são ordenados por `updated_at` decrescente, depois por nome e UUID;
- catálogo de equipes é ordenado alfabeticamente pelo nome, ignorando maiúsculas e acentos, com UUID como desempate;
- o detalhe público inicial agrega os dados do campeonato e a lista de participantes ativos em uma única resposta;
- somente o nome completo da equipe é obrigatório;
- nome curto, sigla, cidade, cor principal e cor secundária são opcionais;
- na ausência de nome curto, a interface usa o nome completo; na ausência de sigla ou cores, usa apresentação neutra;
- nome do campeonato possui de 3 a 120 caracteres, descrição opcional até 1.000 e slug de 3 a 100;
- espaços externos são removidos antes da validação e descrição vazia é tratada como ausência;
- nome da equipe possui de 2 a 100 caracteres; nome curto de 2 a 30; sigla de 2 a 10; cidade de 2 a 100;
- espaços externos são removidos dos campos de equipe e valores opcionais vazios são tratados como ausência;
- sigla aceita somente letras `A-Z` e números e é normalizada para maiúsculas;
- cores usam `#RRGGBB` em maiúsculas e a cor secundária exige uma cor principal;
- `teams.normalized_name` armazena o nome sem espaços externos e em maiúsculas invariantes;
- índice único em `(owner_user_id, normalized_name)` garante a unicidade do nome da equipe;
- mudanças de status usam comandos `publish`, `return-to-draft`, `finish` e `reopen`;
- o endpoint comum de edição não aceita alteração direta de `status_code`;
- edições de campeonato e equipe usam `PUT` com todos os campos editáveis;
- contratos de edição não aceitam proprietário, status ou campos técnicos;
- em `Draft`, todos os campos editáveis do campeonato podem ser alterados;
- em `Published`, nome, descrição e datas podem ser alterados, mas o slug permanece bloqueado;
- em `Finished`, o campeonato é somente leitura até ser reaberto;
- equipes podem ser editadas a qualquer momento, e suas alterações refletem em todos os campeonatos que as utilizam;
- o MVP não mantém versões históricas dos dados cadastrais da equipe;
- equipes são mantidas em catálogo próprio em `/app/teams`;
- o gerenciamento de participantes seleciona equipes existentes e redireciona ao cadastro quando necessário, retornando ao campeonato após salvar;
- listas administrativas usam tabela no desktop e cartões empilhados no celular, com as mesmas informações e ações essenciais;
- a rota pública do Angular é `/championships/:slug`, fora de `/app` e sem autenticação;
- o detalhe administrativo permanece em `/app/championships/:id`, protegido e identificado por UUID;
- o DTO público do campeonato contém nome, slug, status com código e descrição, descrição e datas;
- equipes ativas no DTO público contêm ID, nome, nome curto, sigla, cidade e cores;
- o contrato público não expõe proprietário, nome normalizado, datas técnicas ou participações removidas;
- o detalhe administrativo possui cabeçalho com nome, status e ações válidas;
- `/app/championships/:id` exibe a visão geral e `/app/championships/:id/teams` gerencia participantes ativos e removidos;
- a página pública exibe cabeçalho com nome e status, descrição e período quando informados e lista responsiva de equipes ativas;
- página pública sem participantes exibe estado vazio; slug inexistente ou campeonato em `Draft` retorna página `404`;
- a página pública não exibe ações administrativas;
- inscrição usa `PUT /championships/{championshipId}/teams/{teamId}` de forma idempotente;
- remoção usa `DELETE` no mesmo recurso, preenchendo `removed_at` de forma idempotente;
- primeira inscrição retorna `201 Created`; reinscrição ou vínculo já ativo retorna `200 OK`; remoção retorna `204 No Content`;
- `championship_teams` possui `id` UUID próprio para referências das etapas seguintes;
- reinscrição atualiza `registered_at` para o novo momento e limpa `removed_at`;
- status usa tabela de referência própria, código inteiro estável e enum C# com os mesmos valores;
- códigos e descrições dos status são controlados por migration, sem CRUD administrativo;
- campeonato em rascunho não é público;
- entidades de domínio não são expostas diretamente pela API;
- DTOs administrativos e públicos podem ser diferentes;
- entidades principais usam UUID;
- instantes técnicos são armazenados em UTC;
- imagens permanecem fora do MVP;
- relacionamentos com futuro histórico não usam cascatas amplas.

## Modelo funcional aprovado

### Campeonato

| Campo | Finalidade | Situação |
|---|---|---|
| `id` | Identificador UUID | definido |
| `owner_user_id` | Proprietário e limite de autorização | definido |
| `status_code` | Estado atual, com códigos `1`, `2` e `3` | definido |
| `name` | Nome exibido, de 3 a 120 caracteres | definido |
| `slug` | Identificador da URL pública, de 3 a 100 caracteres | definido |
| `published_at` | Primeira publicação em UTC e bloqueio permanente do slug | definido |
| `description` | Apresentação opcional de até 1.000 caracteres | definido |
| `start_date` | Data planejada e opcional de início | definido |
| `end_date` | Data planejada e opcional de término | definido |
| `created_at` | Criação em UTC | definido |
| `updated_at` | Última alteração em UTC | definido |

Datas de planejamento usam somente data e não representam um instante. Se ambas forem informadas, a data final não pode anteceder a inicial. Não haverá localidade geral nesta etapa.

### Equipe

| Campo | Finalidade | Situação |
|---|---|---|
| `id` | Identificador UUID | definido |
| `owner_user_id` | Proprietário e limite de autorização | definido |
| `name` | Nome completo, de 2 a 100 caracteres | definido |
| `short_name` | Nome curto opcional, de 2 a 30 caracteres | definido |
| `acronym` | Sigla opcional, de 2 a 10 caracteres, com `A-Z` e números | definido |
| `city` | Cidade opcional, de 2 a 100 caracteres | definido |
| `primary_color` | Cor principal opcional em `#RRGGBB` | definido |
| `secondary_color` | Cor secundária opcional em `#RRGGBB`, dependente da principal | definido |
| `created_at` | Criação em UTC | definido |
| `updated_at` | Última alteração em UTC | definido |

Somente o nome completo é obrigatório. Espaços externos são removidos e campos opcionais vazios são tratados como ausência. A sigla aceita somente letras `A-Z` e números e é normalizada para maiúsculas. Cores usam `#RRGGBB` em maiúsculas e a secundária exige uma principal. Na ausência de nome curto, a interface usa o nome completo; sem sigla ou cores, usa apresentação neutra. Escudo continua fora do MVP.

### Participação no campeonato

`championship_teams` representa uma inscrição, não apenas um detalhe técnico de muitos-para-muitos. As etapas seguintes poderão referenciar a equipe dentro de um campeonato por esse vínculo.

| Campo | Finalidade | Situação |
|---|---|---|
| `id` | UUID da participação | definido |
| `championship_id` | Campeonato | definido |
| `team_id` | Equipe | definido |
| `registered_at` | Momento UTC da ativação mais recente | definido |
| `removed_at` | Momento UTC da remoção lógica | definido |

`registered_at` representa a ativação mais recente. Na reinscrição ele é atualizado e `removed_at` é limpo; a data da primeira inscrição não é preservada no MVP. Duas participações ativas da mesma equipe no mesmo campeonato são impedidas.

## Ciclo de vida aprovado

O status controla operações administrativas e visibilidade pública, mas não substitui regras independentes de integridade. O fluxo aprovado usa `Draft`, `Published` e `Finished`, com códigos e transições descritos nesta etapa. `Draft` é privado; os demais são públicos. Não há cancelamento nem arquivamento. Campeonato finalizado precisa ser reaberto antes de novas alterações administrativas. Restrições relacionadas a partidas serão complementadas na etapa 5.

## Autorização e exposição

### Administração

- o usuário vem da identidade autenticada, nunca de `owner_user_id` enviado pelo cliente;
- coleções retornam somente recursos administráveis pelo usuário atual;
- alterar campeonato exige ser seu proprietário;
- inscrever uma equipe exige que o usuário seja proprietário do campeonato e da equipe;
- o backend valida autorização mesmo que o Angular oculte a ação;
- um ID inexistente ou pertencente a outro usuário retorna igualmente `404 Not Found`.

### Consulta pública

- não exige autenticação;
- localiza campeonato por slug;
- retorna somente status públicos;
- não expõe `owner_user_id`, e-mail ou outros dados da conta;
- contém nome, slug, status com código e descrição, descrição e datas do campeonato;
- contém ID, nome, nome curto, sigla, cidade e cores das equipes ativas;
- não contém nomes normalizados, datas técnicas ou participações removidas;
- rascunho e slug inexistente são indistinguíveis para visitantes;
- descoberta e listagem pública ampla continuam na etapa 7, salvo decisão posterior.

## Contratos HTTP aprovados

```text
GET    /api/v1/championships
POST   /api/v1/championships
GET    /api/v1/championships/{championshipId}
PUT    /api/v1/championships/{championshipId}
DELETE /api/v1/championships/{championshipId}
```

Mudanças de ciclo de vida usam comandos explícitos:

```text
POST /api/v1/championships/{championshipId}/publish
POST /api/v1/championships/{championshipId}/return-to-draft
POST /api/v1/championships/{championshipId}/finish
POST /api/v1/championships/{championshipId}/reopen
```

O endpoint comum de edição não aceita alteração direta de `status_code` nem pode ignorar as transições de domínio.

Equipes possuem uma coleção administrativa própria e reutilizável:

```text
GET    /api/v1/teams
POST   /api/v1/teams
GET    /api/v1/teams/{teamId}
PUT    /api/v1/teams/{teamId}
DELETE /api/v1/teams/{teamId}
GET    /api/v1/championships/{championshipId}/teams
PUT    /api/v1/championships/{championshipId}/teams/{teamId}
DELETE /api/v1/championships/{championshipId}/teams/{teamId}
```

O `PUT` cria ou reativa a participação e não duplica um vínculo já ativo. O `DELETE` preenche `removed_at` e também é idempotente.

A primeira inscrição retorna `201 Created` com o DTO da participação. Reinscrição ou vínculo já ativo retorna `200 OK` com o estado atual. A remoção retorna `204 No Content`, inclusive quando o vínculo já estiver removido.

Consulta pública inicial:

```text
GET /api/v1/public/championships/{slug}
```

Esse endpoint agrega os dados básicos do campeonato e seus participantes ativos. Rodadas, partidas e estatísticas terão recursos separados nas etapas correspondentes.

## Respostas e validação

- criação retorna `201 Created` com DTO e localização;
- sucessos retornam DTOs diretamente;
- exclusão ou remoção sem corpo retorna `204 No Content`;
- validação retorna `400` com `ProblemDetails` e erros por campo;
- conflito de slug, nome ou inscrição retorna `409` quando adequado;
- recurso inexistente retorna `404`;
- recurso administrativo inexistente ou de outro proprietário retorna `404`; `403` fica reservado a contextos em que a existência do recurso já seja legitimamente conhecida;
- falhas esperadas usam `Result`/`Result<T>`;
- exceptions inesperadas continuam no tratamento global existente.

Os limites, formatos, coerência das datas, unicidade, quantidade de participantes e operações por status estão definidos nesta etapa e devem ser aplicados tanto nos casos de uso quanto nos contratos de entrada aplicáveis.

## Persistência aprovada

Tabelas esperadas:

- `championship_statuses`;
- `championships`;
- `teams`;
- `championship_teams`.

Relacionamentos de proprietário, status e participação usam `Restrict`/`NoAction`. Índices esperados:

- `championships.owner_user_id` e `championships.status_code`;
- índice único global em `championships.slug`;
- `teams.owner_user_id`;
- índice único em `(teams.owner_user_id, teams.normalized_name)`;
- ambas as FKs de `championship_teams`;
- índice único em `(championship_teams.championship_id, championship_teams.team_id)`, pois reinscrição reutiliza o mesmo registro.

A aplicação mantém `teams.normalized_name` removendo espaços externos e aplicando maiúsculas invariantes, enquanto `name` preserva a apresentação escolhida pelo usuário. Um índice único em `(owner_user_id, normalized_name)` garante a regra no banco sem depender de `citext`.

A migration da etapa cria apenas tabelas, índices e dados de referência aprovados. Ela deve ser revisada e aplicada explicitamente no PostgreSQL local. Não haverá migration automática nem limpeza do database compartilhado por testes.

## Frontend aprovado

Rotas administrativas:

```text
/app/championships
/app/championships/new
/app/championships/:id
/app/championships/:id/edit
/app/championships/:id/teams
/app/teams
/app/teams/new
/app/teams/:id/edit
/championships/:slug
```

Haverá `/app/teams` para manter o catálogo reutilizável do usuário. A inscrição continuará dentro do campeonato: o usuário seleciona uma equipe existente ou segue ao cadastro e retorna ao campeonato após salvar. Não haverá formulário duplicado nem modal de criação nesta etapa.

Telas e comportamentos esperados:

- painel com estados de carregamento, vazio, erro e sucesso;
- formulário de criação e edição;
- detalhe com resumo, status e ações válidas;
- cabeçalho compartilhado com nome, status e ações, visão geral na rota principal e equipes em rota própria;
- seletor de equipe existente, com redirecionamento ao cadastro quando necessário e retorno ao campeonato após salvar;
- confirmações para ações com consequência relevante;
- página pública por slug com cabeçalho, descrição e período opcionais, estado vazio e lista responsiva de equipes ativas;
- página `404` para slug inexistente ou campeonato em `Draft`;
- validação próxima aos campos, mantendo o backend como autoridade;
- rotas administrativas protegidas pela autenticação existente.

Listas administrativas usam tabela no desktop e cartões empilhados no celular, preservando informações e ações essenciais.

## Incrementos implementados

A etapa pode ser entregue em incrementos pequenos, somente depois das decisões essenciais:

1. domínio, enums, regras e contratos aprovados;
2. mapeamentos, contexto e migration revisada;
3. casos de uso e endpoints administrativos de campeonato;
4. equipes, participações e testes de integridade;
5. painel, formulários e gerenciamento no Angular;
6. publicação, endpoint por slug e página pública inicial;
7. builds, testes e validação manual com dois usuários.

Os sete incrementos foram entregues dentro da etapa 3, sem antecipar funcionalidades das etapas seguintes.

## Estratégia de validação

### Backend

- testes das transições e regras de publicação aprovadas;
- proprietário, recurso inexistente e usuário diferente;
- duplicidade de slug, equipe e participação;
- remoção, reinscrição e exclusão;
- projeção pública sem vazamento de dados;
- conversão das falhas esperadas para `ProblemDetails`;
- build e testes da solução.

### Frontend

- formulários e mensagens relevantes;
- serviços HTTP e tratamento de erro;
- ações disponíveis por status;
- estados vazio, carregando, erro e sucesso;
- build de produção e testes em ChromeHeadless.

### Fluxo manual

- usuário A cria e mantém seu campeonato;
- usuário B não vê nem altera os recursos administrativos de A;
- proprietário cria ou seleciona equipe e a inscreve;
- duplicidades e transições inválidas são recusadas;
- rascunho não é acessível anonimamente;
- publicado é acessível pelo slug sem dados privados;
- remoção e exclusão obedecem às regras aprovadas;
- migration é aplicada sem recriar o database.

## Critérios de conclusão aprovados

- usuário administra somente os próprios campeonatos;
- ciclo de vida aprovado é imposto pelo domínio e pela API;
- publicação e URL pública obedecem às regras aprovadas;
- equipes são mantidas no escopo escolhido;
- participação ativa duplicada é impedida;
- remoção não apaga indevidamente dados necessários às próximas etapas;
- visitante acessa apenas campeonatos públicos;
- contratos não expõem entidades nem dados privados;
- migration é revisada e validada no PostgreSQL local;
- backend e frontend compilam e os testes relacionados passam;
- decisões aprovadas ficam registradas nesta etapa.

## Decisões consolidadas do refinamento

### 1. Reutilização da equipe — decidido

Cada equipe pertence a um usuário e pode ser reutilizada em vários campeonatos desse mesmo proprietário. Não existe catálogo global compartilhado. Excluir o cadastro da equipe e remover sua participação em um campeonato são operações distintas.

### 2. Campos do campeonato — decidido

O campeonato possui nome de 3 a 120 caracteres, slug de 3 a 100, status, descrição opcional de até 1.000 caracteres e datas inicial e final opcionais. Espaços externos são removidos antes da validação e descrição vazia é tratada como ausência. As datas usam somente data, a data final não pode anteceder a inicial e não haverá localidade geral nesta etapa.

### 3. Campos da equipe — decidido

A equipe possui nome obrigatório de 2 a 100 caracteres, nome curto opcional de 2 a 30, sigla opcional de 2 a 10, cidade opcional de 2 a 100, cor principal opcional e cor secundária opcional. Espaços externos são removidos e campos opcionais vazios são tratados como ausência. A sigla aceita somente `A-Z` e números e é normalizada para maiúsculas. Cores usam `#RRGGBB` em maiúsculas e a secundária exige uma principal. Na ausência de nome curto, a interface usa o nome completo; sem sigla ou cores, usa apresentação neutra. Escudo e uploads permanecem fora do MVP.

### 4. Status — decidido

O ciclo possui somente `Draft`, `Published` e `Finished`. Os códigos estáveis são `1 = Draft`, `2 = Published` e `3 = Finished`. `Published` cobre todo o período público e ativo. Não haverá `InProgress`, `Cancelled` ou `Archived` nesta etapa.

Transições permitidas:

```text
Draft → Published
Published → Draft, somente enquanto não existirem rodadas ou partidas
Published → Finished
Finished → Published
```

Não existem transições diretas entre `Draft` e `Finished`. Retornos exigem confirmação na interface.

### 5. Publicação — decidido

A publicação é permissiva quanto aos participantes: não exige nenhuma equipe inscrita. Exige nome e slug válidos e, quando informadas, datas coerentes. A descrição permanece opcional. A visibilidade pública é determinada pelo status, sem campo `is_public` separado: `Draft` é privado, enquanto `Published` e `Finished` são públicos. O retorno para `Draft` segue as transições já aprovadas.

### 6. Slug — decidido

O slug é globalmente único, gerado a partir do nome em ASCII minúsculo com palavras separadas por hífens. Colisões recebem sufixo numérico crescente, como `copa-do-bairro-2` e `copa-do-bairro-3`, mantendo constraint única no banco para concorrência. Pode ser ajustado manualmente enquanto o campeonato estiver em `Draft` e nunca tiver sido publicado. Depois da primeira publicação torna-se imutável, mesmo que o campeonato retorne a `Draft` ou que o nome exibido seja alterado, preservando a URL pública.

### 7. Unicidade de nomes — decidido

Nomes de campeonato podem se repetir porque o slug os identifica publicamente. O nome da equipe é único dentro do catálogo do proprietário, ignorando maiúsculas, minúsculas e espaços externos. A aplicação mantém uma coluna técnica `normalized_name` com maiúsculas invariantes, protegida por índice único junto de `owner_user_id`; o nome exibido preserva a capitalização original.

### 8. Remoção e reinscrição — decidido

Remover uma equipe preenche `removed_at` e mantém o vínculo. Reinscrever a mesma equipe limpa `removed_at` e reativa esse registro, preservando seu identificador para referências futuras. O MVP não registra múltiplos períodos completos de entrada e saída. Equipes removidas não aparecem na lista pública atual, mas permanecem visíveis na administração e em registros históricos que as referenciem.

### 9. Exclusão — decidido

Campeonato só pode ser excluído fisicamente quando estiver em `Draft` e não possuir qualquer vínculo de participação ou outra dependência. Equipe só pode ser excluída fisicamente se nunca tiver sido inscrita em campeonato. Nos demais casos a operação é bloqueada. Não haverá soft delete nem arquivamento nesta etapa.

### 10. Participantes — decidido

Não há quantidade mínima para publicação nem limite máximo de equipes. A combinação campeonato/equipe é única e a reinscrição reutiliza o vínculo removido. Equipes removidas não aparecem na lista pública atual. Participantes podem ser adicionados ou removidos em `Draft` e `Published`. Em `Finished`, alterações exigem reabertura. A etapa 5 definirá as restrições de remoção quando existirem partidas pendentes. A ordenação padrão é alfabética pelo nome da equipe, ignorando maiúsculas e acentos; `registered_at` não determina a ordenação.

### 11. Recurso de outro proprietário — decidido

Recursos administrativos são consultados por ID já filtrando o proprietário. Recurso inexistente e recurso pertencente a outro usuário retornam igualmente `404 Not Found`, evitando confirmar sua existência. `403 Forbidden` permanece disponível para contextos em que a existência do recurso já seja legitimamente conhecida.

### 12. Concorrência — decidido

Será usada a política de última gravação válida, sem token de concorrência otimista nesta etapa. Duas abas podem sobrescrever alterações entre si. `updated_at` permanece informativo e não representa versão.

### 13. Paginação administrativa — decidido

Listas administrativas de campeonatos, equipes e participantes não serão paginadas nesta etapa. Campeonatos são ordenados por `updated_at` decrescente, depois por nome e UUID. O catálogo de equipes e participantes usa ordem alfabética pelo nome, ignorando maiúsculas e acentos, com UUID como desempate. Paginação e listagem pública ampla permanecem para a etapa 7 ou para quando houver volume real que justifique a mudança.

### 14. Página pública inicial — decidido

Um único endpoint retorna os dados básicos do campeonato e seus participantes ativos. Rodadas, partidas e estatísticas terão endpoints próprios nas etapas correspondentes.

## Registro da implementação — 24 de outubro de 2026

Foram entregues:

- entidades de domínio para campeonato, status, equipe e participação, com as transições e restrições aprovadas;
- casos de uso com `Result`/`Result<T>`, autorização por proprietário e DTOs administrativos e públicos distintos;
- endpoints administrativos de campeonatos, comandos de status, catálogo de equipes e gerenciamento idempotente de participantes;
- endpoint público agregado por slug, limitado a campeonatos publicados ou finalizados e participantes ativos;
- persistência EF Core com Fluent API, relacionamentos restritos, índices de unicidade e a tabela de referência de status;
- migration `20261024211759_ChampionshipsAndTeams`, revisada e aplicada explicitamente ao PostgreSQL local;
- painel Angular protegido, catálogo e formulários de equipes, listagem, formulário, detalhe e participantes do campeonato;
- página pública responsiva em `/championships/:slug`, incluindo estados de carregamento, vazio, erro e não encontrado;
- quatro testes unitários das regras de domínio da etapa, somados aos testes existentes.

O campo técnico `published_at` registra a primeira publicação. Ele permite manter o slug bloqueado mesmo se o campeonato retornar a rascunho, sem expor esse instante nos contratos públicos.

Validações concluídas:

- solution backend compilada sem erros ou avisos;
- 15 testes backend aprovados;
- build de produção Angular aprovado;
- 9 testes Angular aprovados no ChromeHeadless;
- migration aplicada ao database local existente sem recriá-lo;
- fluxo manual validado com dois usuários, dois times e um campeonato: criação, inscrição, publicação, consulta pública e finalização;
- a consulta pública retornou apenas as duas equipes ativas;
- o segundo usuário recebeu `404` ao consultar administrativamente o campeonato do primeiro;
- a remoção de participante após a finalização foi recusada com `409`;
- o campeonato finalizado permaneceu acessível pela URL pública.
- nome de equipe duplicado para o mesmo proprietário foi recusado com `409`;
- inscrição já ativa e reinscrição foram idempotentes, reutilizando o vínculo com `200`;
- campeonato em rascunho permaneceu indisponível publicamente com `404`;
- alteração do slug após a primeira publicação foi recusada com `409`;
- equipe nunca utilizada pôde ser excluída, enquanto equipe já participante e campeonato com histórico tiveram a exclusão recusada com `409`.

A validação manual criou dados identificados como teste no database compartilhado e não executou limpeza automática.

## Dependências

- etapa 2 concluída e validada;
- identidade autenticada disponível para obter o proprietário;
- `LocalScoreDbContext` e infraestrutura de migrations existentes;
- nenhuma dependência de Docker ou Testcontainers.
---

# Etapa 4 — Jogadores e inscrições

**Estado:** planejada; requer refinamento antes da implementação.

## Objetivo

Permitir cadastrar jogadores e formar os elencos das equipes sem escalação por partida.

## Funcionalidades previstas

- cadastro e edição de jogador;
- inscrição em equipe participante;
- número de camisa opcional;
- posição opcional;
- encerramento ou remoção controlada da inscrição;
- consulta do elenco.

## Frontend

- lista e formulário de jogadores;
- gerenciamento do elenco;
- visualização pública básica.

## Backend

- casos de uso e endpoints de jogador e inscrição;
- validação de equipe participante;
- validação da inscrição ativa.

## Banco de dados

- `players`;
- `player_registrations`;
- índices por campeonato, equipe e jogador;
- migration própria da etapa.

## Dependências

- etapa 3 concluída.

## Critério de conclusão

Cada equipe possui um elenco consultável, com vínculos historicamente consistentes.

## Decisões já tomadas

- não haverá escalação no MVP;
- eventos poderão referenciar jogadores ativos da equipe no campeonato;
- um jogador possui somente uma inscrição ativa por campeonato;
- dados pessoais devem permanecer mínimos;
- fotos e uploads ficam fora.

## Decisões pendentes

- campos definitivos do jogador;
- regra e histórico de transferência;
- comportamento de exclusão;
- visibilidade pública dos dados;
- posições disponíveis e sua representação.

---

# Etapa 5 — Rodadas e partidas

**Estado:** planejada; requer refinamento antes da implementação.

## Objetivo

Permitir montar manualmente a agenda e registrar o resultado oficial das partidas.

## Funcionalidades previstas

- CRUD manual de rodadas;
- CRUD manual de partidas;
- associação de mandante e visitante;
- data e hora;
- local como texto livre;
- status da partida;
- placar manual;
- filtros por rodada, equipe, status e data.

## Frontend

- administração de rodadas;
- agenda e formulário de partidas;
- detalhe administrativo e público da partida.

## Backend

- casos de uso e endpoints;
- validação de participantes;
- validação de mandante diferente de visitante;
- transições de status aprovadas;
- placar como fonte oficial do resultado.

## Banco de dados

- `rounds`;
- `match_statuses`;
- `matches`;
- índices por campeonato, rodada, equipes, status e data;
- migration própria da etapa.

## Dependências

- etapas 3 e 4 concluídas.

## Critério de conclusão

Usuário cria rodadas e partidas manualmente, registra placares e o público consulta agenda e resultados.

## Decisões já tomadas

- rodada criada manualmente;
- número único dentro do campeonato;
- partida pertence obrigatoriamente a uma rodada;
- datas da rodada são informativas;
- local é texto livre;
- placar é informado manualmente e é a fonte oficial;
- status usa tabela numérica separada;
- sem geração automática, escalação, WO ou fluxo avançado de correção nesta fase.

## Decisões pendentes

- campos finais e obrigatoriedade de data/hora;
- códigos e transições finais de status;
- formato do minuto/duração da partida;
- conflitos de agenda;
- regras de edição e exclusão após finalização;
- comportamento de partidas adiadas e canceladas.

---

# Etapa 6 — Eventos manuais da partida

**Estado:** planejada; requer refinamento antes da implementação.

## Objetivo

Registrar manualmente os acontecimentos básicos e produzir artilharia a partir dos gols informados.

## Funcionalidades previstas

- gols;
- cartões amarelos;
- cartões vermelhos;
- substituições;
- linha do tempo;
- edição e remoção de eventos;
- aviso de divergência entre gols e placar.

## Frontend

- formulário rápido de evento;
- seleção de equipe e jogador;
- seleção dos jogadores de entrada e saída;
- linha do tempo da partida;
- aviso de inconsistência.

## Backend

- CRUD de eventos;
- validação de equipe participante;
- validação de jogador ativo na equipe;
- validação mínima de substituição;
- consulta cronológica.

## Banco de dados

- `match_event_types`;
- `match_events`;
- índices cronológicos e relacionamentos;
- migration própria da etapa.

## Dependências

- etapas 4 e 5 concluídas.

## Critério de conclusão

Eventos podem ser registrados e corrigidos manualmente; a linha do tempo é consistente e gols alimentam a artilharia.

## Decisões já tomadas

- tipos iniciais: gol, amarelo, vermelho e substituição;
- tipo usa código numérico, descrição e enum correspondente;
- sem escalação;
- jogadores da substituição devem ser diferentes e pertencer à mesma equipe;
- não validar limite, janela, entrada anterior ou saída anterior;
- placar não é calculado pelos eventos;
- divergência gera aviso e não bloqueia finalização;
- gol contra e assistência ficam fora.

## Decisões pendentes

- códigos numéricos definitivos dos tipos;
- representação de minuto e acréscimo;
- regras de edição e exclusão;
- ordenação em eventos com o mesmo minuto;
- eventos permitidos por status da partida.

---

# Etapa 7 — Área pública e estatísticas básicas

**Estado:** planejada; requer refinamento antes da implementação.

## Objetivo

Oferecer ao visitante uma experiência responsiva para acompanhar o campeonato.

## Funcionalidades previstas

- lista de campeonatos públicos;
- página pública do campeonato;
- rodadas e partidas;
- partidas futuras e realizadas;
- placares;
- equipes e jogadores;
- classificação;
- artilharia baseada em gols registrados.

## Frontend

- navegação pública responsiva;
- filtros básicos;
- tabela de classificação;
- artilharia;
- estados de carregamento, vazio e erro;
- URLs estáveis.

## Backend

- endpoints públicos de leitura;
- consultas otimizadas e paginadas quando necessário;
- cálculo de classificação a partir de partidas finalizadas;
- cálculo de gols registrados por jogador.

## Banco de dados

- revisão de índices conforme consultas reais;
- sem tabela materializada de classificação inicialmente.

## Dependências

- etapas 3 a 6 concluídas.

## Critério de conclusão

Visitante acompanha o campeonato em celular e desktop sem autenticação.

## Decisões já tomadas

- vitória 3 pontos, empate 1 e derrota 0;
- sem critério esportivo de desempate;
- equipes empatadas em pontos ocupam a mesma posição;
- nome pode ordenar visualmente, sem valor esportivo;
- artilharia representa somente gols efetivamente registrados;
- classificação e estatísticas são calculadas por consulta no início.

## Decisões pendentes

- paginação e filtros definitivos;
- quais status de campeonato são públicos;
- formato visual das páginas;
- quais dados de jogador serão públicos;
- regras para partidas canceladas e adiadas nas estatísticas.

---

# Etapa 8 — Qualidade, documentação e entrega

**Estado:** planejada; requer refinamento antes da implementação.

## Objetivo

Consolidar o MVP para demonstração e futura publicação.

## Funcionalidades previstas

- testes dos fluxos críticos;
- revisão de segurança e autorização;
- documentação de execução e arquitetura;
- dados de demonstração sem segredos;
- CI para build e testes;
- planejamento de deploy, migrations e backup.

## Frontend

- build de produção;
- revisão responsiva e de acessibilidade;
- testes dos fluxos principais;
- tratamento final de erros.

## Backend

- testes unitários e de integração;
- revisão de autorização por proprietário;
- health checks adequados ao ambiente final;
- configuração de produção e observabilidade.

## Banco de dados

- migrations revisadas;
- estratégia de backup e restauração;
- ambiente isolado para testes;
- possível adoção de Testcontainers após autorização de Docker.

## Dependências

- etapas 1 a 7 concluídas.

## Critério de conclusão

Fluxos principais passam no CI, a aplicação pode ser demonstrada com dados consistentes e existe um processo documentado de implantação e recuperação.

## Decisões pendentes

- autorização para Docker;
- provedor e arquitetura de hospedagem;
- plataforma de CI/CD;
- estratégia de testes E2E;
- armazenamento de segredos de produção;
- política de backup, retenção e rollback.

---

# Backlog posterior ao MVP

- geração automática de confrontos;
- turno e returno automatizados;
- grupos e mata-mata;
- critérios configuráveis de desempate;
- compartilhamento da administração;
- organizações;
- confirmação e recuperação de e-mail;
- edição e exclusão de conta;
- assistências, gol contra e suspensões;
- escalações;
- locais reutilizáveis;
- árbitros e comissão técnica;
- atualizações em tempo real;
- notificações;
- importação e exportação;
- relatórios;
- uploads e object storage;
- PWA ou aplicativo móvel;
- planos e pagamentos.
