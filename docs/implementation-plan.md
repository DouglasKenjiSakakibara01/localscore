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
| 2 | Contas e autenticação | Decisões fechadas; implementação não iniciada |
| 3 | Campeonatos e equipes | Planejada; requer refinamento |
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

**Estado:** decisões fechadas; implementação não iniciada.

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

Implementar `Result`, `Result<T>`, erros de aplicação, conversão para `ProblemDetails`, handler global, `traceId` e testes. Não adicionar banco ou Identity.

**Concluído quando:** falhas esperadas e inesperadas têm contratos distintos e testados; backend compila sem erros.

### 2.2 — PostgreSQL, EF Core e Identity

Adicionar pacotes, User Secrets, `ApplicationUser`, `LocalScoreDbContext`, mapeamentos, política de senha/bloqueio e registros de DI. Não criar endpoints ou JWT.

**Concluído quando:** contexto é descoberto pelo EF, configurações sensíveis não são versionadas e backend compila.

### 2.3 — Refresh tokens e primeira migration

Implementar a entidade, hash, família, rotação persistente, índices e primeira migration. Aplicar manualmente e inspecionar o PostgreSQL local.

**Concluído quando:** somente tabelas de autenticação existem e a migration foi validada.

### 2.4 — JWT e serviços de autenticação

Implementar emissão, claims, sessão, rotação, detecção de reutilização, múltiplas famílias e limpeza oportunística, com testes unitários.

**Concluído quando:** regras de token e sessão estão testadas sem expor segredos.

### 2.5 — Endpoints da API

Implementar os cinco endpoints, cookie, validações, rate limiting e autorização bearer.

**Concluído quando:** contratos e fluxos funcionam manualmente no banco local.

### 2.6 — Infraestrutura de autenticação no Angular

Implementar proxy, cliente HTTP, estado, armazenamento, parser, interceptor, refresh, coordenação entre abas e guardas.

**Concluído quando:** sessão é restaurada e renovada, e rotas protegidas funcionam corretamente.

### 2.7 — Telas

Implementar login, cadastro e `/app` com formulários reativos, feedback, loading, erros por campo e responsividade.

**Concluído quando:** fluxo completo pode ser utilizado em celular e desktop.

### 2.8 — Validação e documentação

Executar builds, testes, roteiro manual, revisão de segurança e atualização documental.

**Concluído quando:** todos os critérios aprovados passam e pendências ficam registradas.

## Dependências

- etapa 1 concluída;
- PostgreSQL local acessível;
- credenciais configuradas pelo usuário em User Secrets antes de aplicar a migration;
- nenhuma dependência de Docker.

## Critério final da etapa 2

Um usuário consegue se cadastrar, entrar automaticamente, recarregar a aplicação, acessar `/app`, renovar a sessão, usar múltiplas sessões e sair. Falhas retornam contratos consistentes, segredos não vazam, a migration foi validada manualmente e nenhuma tabela de domínio foi criada.

---

# Etapa 3 — Campeonatos e equipes

**Estado:** planejada; requer refinamento antes da implementação.

## Objetivo

Permitir que um usuário autenticado crie e administre seus campeonatos e equipes participantes.

## Funcionalidades previstas

- CRUD de campeonato;
- vínculo obrigatório entre campeonato e proprietário;
- estados do campeonato;
- publicação e consulta pública conforme status;
- CRUD de equipes;
- inscrição e remoção controlada de equipes no campeonato.

## Frontend

- painel dos campeonatos do usuário;
- formulários de campeonato e equipe;
- lista de participantes;
- página pública inicial do campeonato.

## Backend

- controllers e casos de uso;
- autorização por `owner_user_id`;
- validações de status e duplicidade;
- DTOs públicos e administrativos separados quando necessário.

## Banco de dados

- `championship_statuses`;
- `championships`;
- `teams`;
- `championship_teams`;
- índices de proprietário, slug e vínculos;
- migration própria da etapa.

## Dependências

- etapa 2 concluída.

## Critério de conclusão

Usuário cria um campeonato, inclui equipes e somente ele pode alterar esses dados. Um campeonato publicável pode ser consultado anonimamente.

## Decisões já tomadas

- usuário gerencia somente campeonatos próprios;
- status em tabela separada com código numérico e descrição;
- códigos controlados por migration e enum;
- campeonato em rascunho não é público;
- imagens ficam fora do MVP.

## Decisões pendentes

- campos definitivos de campeonato e equipe;
- códigos e transições finais de status;
- regras para publicação;
- formato e unicidade do slug;
- comportamento de exclusão;
- reutilização de equipe em vários campeonatos;
- endpoints definitivos e telas.

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
