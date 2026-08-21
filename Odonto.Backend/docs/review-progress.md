# Continuidade do code review — Odonto Backend

## Identificacao

- Token de referencia: `ODONTO-REV-7K3M`
- Ultima atualizacao: 2026-08-21
- Objetivo: registrar o contexto necessario para retomar o code review e acompanhar as correcoes do backend em conversas futuras.

> O token e apenas um identificador deste documento. Para retomar o trabalho, abra este repositorio e solicite: "Leia `docs/review-progress.md` e continue o trabalho do token `ODONTO-REV-7K3M`".

## Contexto atual

O repositorio esta no inicio da estruturacao de um backend para gestao de clinica odontologica, usando C# e .NET 10. A arquitetura pretendida e Clean Architecture com DDD, dividida nos projetos:

- `Odonto.API`
- `Odonto.Application`
- `Odonto.Domain`
- `Odonto.Infrastructure`
- `Odonto.IoC`
- `Odonto.Tests`

Os modulos de pacientes, profissionais, especialidades, agendamentos e consultas ainda nao foram implementados. Domain, Application e Tests ainda possuem somente classes vazias do template.

## Achados do review

### Prioridade critica

- [x] Implementar a composicao de dependencias. `ResolveDependencies` registra `AppDbContext` com SQL Server e o nucleo do Identity com roles, stores do EF Core e provedores de token. Ainda nao existem outros servicos concretos em Application ou Infrastructure para registrar.
- [x] Adicionar ao `Odonto.IoC` as referencias de projeto necessarias para registrar Infrastructure e Application.
- [ ] Corrigir a vulnerabilidade de alta severidade `NU1903` em `Microsoft.OpenApi 2.0.0`, trazida pela combinacao atual de OpenAPI/Scalar. O alerta impede o build da API no ambiente revisado.
- [x] Substituir o `HintPath` de `Microsoft.Extensions.DependencyInjection.Abstractions` em `Odonto.IoC.csproj` pela referencia de framework `Microsoft.AspNetCore.App`.
- [x] Transformar `Odonto.Tests` em um projeto de testes real, adicionando `Microsoft.NET.Test.Sdk`, xUnit, runner, referencias de projeto e testes executaveis.

### Prioridade alta

- [ ] Rever o armazenamento de `RefreshToken` e `TokenResetarSenha` em `AppUser`. Tokens nao devem ser persistidos em texto puro; refresh tokens devem ter hash, rotacao e revogacao. Para recuperacao de senha, preferir o mecanismo do ASP.NET Core Identity.
- [x] Obter a connection string por `IConfiguration.GetConnectionString`, validar sua existencia e usa-la no registro de `AppDbContext` com SQL Server.
- [x] Remover `WeatherForecastController`, `WeatherForecast.cs` e as requisicoes de exemplo sem relacao com o dominio.

### Prioridade media

- [x] Remover `AppUser.Nome` e transferir o nome para o agregado `Funcionario`, vinculado ao usuario do Identity por chave estrangeira unica.
- [x] Alterar o construtor de `AppDbContext` para receber `DbContextOptions<AppDbContext>`.
- [x] Configurar Identity, autenticacao JWT, autorizacao, `UseAuthentication()` e `UseAuthorization()` antes de criar endpoints protegidos.
- [ ] Definir uma estrategia consistente para validacao, representacao de erros e tratamento global de excecoes.
- [ ] Adicionar configuracao reproduzivel de build e CI com restore, auditoria de dependencias, build e testes.

## Verificacoes ja realizadas

- Domain, Application, Infrastructure e IoC compilam isoladamente.
- O modelo de `Funcionario` e o relacionamento um-para-um com `AppUser` compilam sem warnings.
- A API reporta `NU1903` para `Microsoft.OpenApi 2.0.0` e falha no build no ambiente revisado.
- `dotnet test` termina sem executar testes porque `Odonto.Tests` ainda e uma biblioteca comum.
- O build completo da solucao tambem encontrou uma falha local nos resolvers de workload do SDK .NET 10. Esse ponto aparenta ser do ambiente instalado e deve ser validado separadamente em outra maquina ou CI.

## Arquivos mais relevantes

- `Odonto.IoC/DI/DependencyInjection.cs`
- `Odonto.IoC/Odonto.IoC.csproj`
- `Odonto.Infrastructure/Context/AppDbContext.cs`
- `Odonto.Infrastructure/User/AppUser.cs`
- `Odonto.API/Program.cs`
- `Odonto.API/Odonto.API.csproj`
- `Odonto.Tests/Odonto.Tests.csproj`
- `Odonto.API/Controllers/WeatherForecastController.cs`

## Ordem sugerida de execucao

1. Corrigir as dependencias e obter um build limpo e reproduzivel.
2. Implementar corretamente IoC, EF Core, SQL Server e Identity.
3. Configurar a infraestrutura de testes e adicionar testes de inicializacao.
4. Remover os artefatos do template.
5. Definir o desenho seguro de autenticacao e tokens.
6. Implementar Pacientes como o primeiro fluxo vertical completo, incluindo dominio, caso de uso, persistencia, endpoint, validacao e testes.
7. Configurar CI para impedir regressao de build, vulnerabilidades e testes.

## Historico de andamento

### 2026-07-15

- Code review geral inicial concluido.
- Este documento de continuidade foi criado.
- Composicao de dependencias implementada em `Odonto.IoC/DI/DependencyInjection.cs`.
- Nome `ResolveDependecies` corrigido para `ResolveDependencies` e chamada da API atualizada.
- IoC passou a referenciar Application e Infrastructure.
- `AppDbContext` registrado com SQL Server usando `ConnectionStrings:SqlServer` e validacao de configuracao ausente.
- Identity Core registrado com `AppUser`, roles, stores do EF Core e provedores padrao de token.
- Referencia fisica ao cache NuGet removida e substituida por `Microsoft.AspNetCore.App`.
- Construtor de `AppDbContext` alterado para `DbContextOptions<AppDbContext>`.
- `Odonto.IoC` restaurado e compilado com zero warnings e zero erros.
- A integracao completa ate `Odonto.API` foi compilada com zero warnings e zero erros, desabilitando `NuGetAudit` somente nessa verificacao porque o `NU1903` permanece como item separado.
- Alteracoes revisadas e deixadas no working tree para commit manual pelo responsavel do repositorio.

### 2026-08-21

- Agregado `Funcionario` criado no modulo Equipe com fabrica e invariantes para os dados editaveis do perfil.
- `AppUser.Nome` removido e substituido pelo vinculo obrigatorio e unico `AppUser.FuncionarioId`.
- Configuracoes do EF Core e migration de preservacao dos usuarios existentes adicionadas.
- E-mail de recuperacao tornou-se generico para nao acoplar autenticacao ao dominio de funcionarios.
- Build completo concluído com sucesso; as correções de modelagem não adicionaram warnings. Permanecem cinco warnings anteriores nos DTOs de autenticação e na visibilidade de `CriarToken`. Os 18 testes foram executados com sucesso, incluindo domínio, relacionamento e geração do script de migration.
- Autenticacao JWT Bearer configurada com validacao de assinatura, emissor, audiencia e expiracao.
- JWT passou a transportar `sub`, `funcionario_id` e somente as roles reais do Identity; a claim fixa de administrador foi removida.
- `GET /api/me` e `PATCH /api/me` implementados para consultar e editar somente o funcionario autenticado.
- Cadastro de usuario deixou de ser anonimo, enquanto login e recuperacao de senha permanecem acessiveis sem sessao.
- Build concluido com zero warnings e zero erros; 26 testes executados com sucesso.
- Seeder controlado de provisionamento inicial adicionado à inicialização da API.
- O primeiro `Funcionario`, `AppUser` e a role configurada são criados em uma transação serializável somente quando o bootstrap está habilitado e não existem contas.
- Cenários de criação, idempotência, bootstrap desabilitado e rollback foram cobertos com SQLite em memória.
- Build concluído com zero warnings e zero erros; 29 testes executados com sucesso.
- OpenAPI passou a declarar o esquema JWT Bearer e a marcar somente as operações protegidas, permitindo autenticação correta pelo Scalar.
- Build concluído com zero warnings e zero erros; 30 testes executados com sucesso.

## Como atualizar este documento

Ao concluir uma correcao:

1. Marcar o item correspondente com `[x]`.
2. Registrar no historico a data, os arquivos alterados e as verificacoes executadas.
3. Atualizar a ordem sugerida se a proxima prioridade tiver mudado.
4. Manter anotados bloqueios, decisoes arquiteturais e riscos que ainda exigem validacao.
