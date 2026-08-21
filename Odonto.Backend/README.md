# Odonto Backend

Backend de uma aplicação para gestão de clínica odontológica, desenvolvido em C# com ASP.NET Core e organizado segundo os princípios de Clean Architecture e Domain-Driven Design (DDD).

O projeto tem como objetivo centralizar os principais processos da clínica, como o gerenciamento de pacientes, profissionais, especialidades, agendamentos e consultas. A implementação será feita de forma incremental, mantendo as regras de negócio isoladas das tecnologias externas e priorizando código simples, testável e fácil de evoluir.

> O sistema ainda está em fase inicial de estruturação. As funcionalidades descritas neste documento representam o escopo proposto e poderão ser refinadas conforme as regras de negócio forem definidas.

## Escopo proposto

Inicialmente, o sistema deverá oferecer suporte aos seguintes módulos:

- **Pacientes:** cadastro, consulta, atualização, pesquisa e controle de situação.
- **Profissionais:** cadastro dos dentistas responsáveis pelos atendimentos.
- **Especialidades:** gerenciamento das áreas de atuação dos profissionais.
- **Agendamentos:** reserva, remarcação e cancelamento de horários.
- **Consultas:** registro dos atendimentos realizados para os pacientes.
- **Usuários e acesso:** autenticação, autorização, recuperação de senha e controle de usuários ativos.

Regras mais específicas, como disponibilidade dos profissionais, conflitos de horários, estados do agendamento e informações do prontuário, ainda deverão ser formalizadas antes da implementação de cada módulo.

## Arquitetura

A solução segue Clean Architecture, com as responsabilidades divididas entre os seguintes projetos:

```text
Odonto.API
Odonto.Application
Odonto.Domain
Odonto.Infrastructure
Odonto.IoC
Odonto.Tests
```

### Odonto.API

Expõe os endpoints HTTP, recebe e valida as requisições, aplica autenticação e autorização e transforma os resultados da aplicação em respostas HTTP. A API não deve conter regras de negócio.

### Odonto.Application

Coordena os casos de uso e o fluxo da aplicação. Essa camada utiliza o domínio e os contratos necessários para executar as operações, evitando concentrar regras que pertencem às entidades e aos serviços de domínio.

### Odonto.Domain

Representa o núcleo do sistema. Contém entidades, value objects, regras de negócio, serviços de domínio, exceções e contratos de repositórios. Não deve depender das demais camadas nem de tecnologias externas.

### Odonto.Infrastructure

Implementa persistência, repositórios e integrações externas. O projeto utilizará Entity Framework Core, SQL Server e ASP.NET Core Identity.

### Odonto.IoC

Centraliza a composição da aplicação e o registro das dependências utilizadas pelas diferentes camadas.

### Odonto.Tests

Contém os testes automatizados do domínio, dos casos de uso e, quando necessário, testes de integração da infraestrutura e da API.

O fluxo esperado para uma requisição é:

```text
HTTP Request
    -> API
    -> Application
    -> Domain contract
    -> Infrastructure
    -> Database
```

As dependências devem apontar em direção ao domínio, preservando as regras de negócio de detalhes como banco de dados, autenticação e protocolo HTTP.

## Tecnologias previstas

- C# e .NET 10
- ASP.NET Core
- Entity Framework Core
- SQL Server
- ASP.NET Core Identity
- JWT Bearer
- FluentValidation
- OpenAPI e Scalar
- xUnit
- Docker e Docker Compose

Nem todas as tecnologias listadas já estão configuradas no estado atual do repositório.

## Estratégia inicial de desenvolvimento

A evolução proposta para o backend é:

1. Ajustar a estrutura da solução, as referências entre projetos e a injeção de dependências.
2. Configurar o banco de dados, Entity Framework Core e ASP.NET Core Identity.
3. Configurar os testes automatizados e remover os arquivos de exemplo do template.
4. Formalizar as regras de negócio iniciais no contexto do projeto.
5. Implementar pacientes como a primeira funcionalidade completa, atravessando todas as camadas.
6. Implementar autenticação e autorização.
7. Implementar profissionais e especialidades.
8. Implementar disponibilidade e agendamentos.
9. Implementar consultas e, posteriormente, o prontuário odontológico.

Cada funcionalidade deve ser desenvolvida de ponta a ponta, com regras documentadas, validação, persistência, endpoints e testes.

## Princípios de desenvolvimento

- Regras de negócio devem permanecer no domínio sempre que possível.
- Controllers devem ser pequenos e delegar o fluxo para a camada de aplicação.
- A infraestrutura não deve definir regras de negócio.
- Entradas externas devem ser validadas.
- Operações de I/O devem utilizar APIs assíncronas.
- Consultas devem evitar N+1 e utilizar paginação quando apropriado.
- Segredos e credenciais não devem ser versionados.
- Mudanças devem ser pequenas, focadas e acompanhadas por testes relevantes.
- Commits devem seguir o padrão Conventional Commits.

## Estado atual

No momento, o repositório contém a estrutura inicial da solução, a configuração básica da API e o início da infraestrutura de persistência e identidade. Os módulos odontológicos ainda não foram implementados e as regras detalhadas do domínio ainda precisam ser definidas.

As decisões arquiteturais, padrões de código e contexto do projeto estão documentados no diretório [`.forge`](.forge/README.md).

## Configuração de autenticação

A API exige `ConnectionStrings:SqlServer`, `Jwt:Key`, `Jwt:Issuer` e
`Jwt:Audience` na inicialização. A chave JWT deve possuir pelo menos 32 bytes e
não deve ser versionada. Em desenvolvimento, use User Secrets ou variáveis de
ambiente, como `Jwt__Key` e `ConnectionStrings__SqlServer`.

### Primeiro usuário

O primeiro usuário pode ser provisionado na inicialização da API com as chaves
`Bootstrap:Enabled`, `Bootstrap:NomeCompleto`, `Bootstrap:NomeExibicao`,
`Bootstrap:Email`, `Bootstrap:Password` e `Bootstrap:Role`. O schema do banco
deve estar atualizado antes da execução. O seeder cria funcionário, conta e
role em uma única transação e é ignorado quando já existe qualquer usuário.

Depois do provisionamento, defina `Bootstrap:Enabled` como `false`, remova
`Bootstrap:Password` da configuração e altere a senha inicial.
