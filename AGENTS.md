# AGENTS.md

## Fontes de referência

- Leia este arquivo antes de alterar o repositório.
- Consulte `docs/implementation-plan.md` para escopo, etapas, decisões aprovadas, pendências e critérios de conclusão.
- Consulte `README.md` para pré-requisitos e execução local.
- Quando uma decisão de produto ou implementação mudar, atualize `docs/implementation-plan.md` na mesma etapa de trabalho.
- Não duplique neste arquivo detalhes que pertencem ao plano.

## Forma de trabalho

- Trabalhe em uma etapa do plano por vez.
- Cada etapa deve, preferencialmente, ser tratada em uma conversa separada.
- Não implemente nem avance para outra etapa sem solicitação explícita do usuário.
- O usuário participa das decisões técnicas e de domínio.
- Antes de uma decisão relevante ainda pendente, apresente alternativas, prós, contras e uma recomendação simples.
- Não altere decisões aprovadas silenciosamente.
- Faça mudanças pequenas, verificáveis e restritas ao incremento autorizado.
- Ao concluir, informe o que mudou, como foi validado e o que permanece pendente.
- Evite overengineering e não crie abstrações sem uso concreto.

## Stack e estrutura

- Frontend: Angular 20, componentes standalone, roteamento, SCSS, Signals e RxJS.
- Backend: ASP.NET Core Web API em .NET 9, com controllers.
- Persistência planejada: PostgreSQL, Entity Framework Core e Npgsql.
- Frontend e backend permanecem no mesmo repositório, respectivamente em `frontend/` e `backend/`.
- A solução backend fica em `backend/LocalScore.sln`.

Responsabilidades das camadas:

- `LocalScore.Domain`: entidades e regras de negócio; não depende de outras camadas do projeto.
- `LocalScore.Application`: casos de uso, contratos internos, validações e abstrações necessárias pela aplicação.
- `LocalScore.Infrastructure`: persistência e implementações de integrações.
- `LocalScore.Api`: controllers, contratos HTTP, middlewares e composição da aplicação.

Mantenha as dependências apontando para dentro. EF Core, ASP.NET Core e detalhes HTTP não devem entrar no domínio.

## Restrições atuais

- A etapa 1 está concluída.
- A etapa 2 está concluída e validada com o PostgreSQL local.
- Não adicione Docker ou Testcontainers sem nova autorização do usuário.
- Não crie commits, branches, remotes ou pull requests sem solicitação.
- Preserve alterações existentes do usuário e não reverta arquivos fora do escopo.

## Convenções de implementação

- Não exponha entidades de domínio diretamente pela API; use DTOs.
- Use `Result`/`Result<T>` para falhas esperadas nos limites dos casos de uso.
- Retorne DTOs diretamente em sucessos HTTP; não crie envelope universal `ApiResponse<T>`.
- Use `ProblemDetails` para erros HTTP e tratamento global para exceptions inesperadas.
- O backend é a autoridade final de validação; validação no Angular serve à experiência do usuário.
- Armazene instantes em UTC.
- Use UUID para entidades principais.
- No PostgreSQL, use schema `public` e nomes em `snake_case`.
- Configure entidades com Fluent API na Infrastructure.
- Tabelas de referência usam código inteiro estável, descrição e enum C# com os mesmos valores.
- Não reutilize códigos descontinuados e não crie uma tabela genérica para status de conceitos diferentes.
- Não introduza repository genérico, CQRS, MediatR, NgRx, mensageria ou microsserviços sem necessidade demonstrada e decisão prévia.
- Não armazene senhas, tokens, connection strings ou outros segredos no repositório ou em logs.

## Persistência e migrations

- Use um único `LocalScoreDbContext` no MVP.
- Mantenha contexto, mapeamentos e migrations em `LocalScore.Infrastructure/Persistence`.
- Não use `EnsureCreated` nem aplique migrations automaticamente no startup.
- Gere, revise e aplique migrations de forma explícita.
- Testes automatizados não podem limpar ou recriar o database local compartilhado.

## Validação mínima

Execute somente as verificações compatíveis com o incremento atual:

- compile `backend/LocalScore.sln`;
- execute o build do Angular;
- execute testes relacionados às mudanças;
- valide manualmente endpoints ou fluxos novos quando necessário;
- registre qualquer validação bloqueada pelo ambiente, sem tratá-la como sucesso.

Comandos de referência:

```powershell
dotnet build backend/LocalScore.sln

Set-Location frontend
npm run build
npm test -- --watch=false --browsers=ChromeHeadless
```

## Git

- A branch inicial é `main`.
- O remote pode ainda não estar configurado.
- Não altere configuração global do Git para contornar limitações da conta sandbox.
- Não execute operações destrutivas ou descarte mudanças sem autorização explícita.
