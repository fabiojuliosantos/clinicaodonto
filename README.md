# Clínica Odonto

Backend para gerenciamento de clínicas odontológicas, desenvolvido com C# e ASP.NET Core. A solução segue princípios de Clean Architecture e Domain-Driven Design para manter as regras de negócio isoladas da infraestrutura.

> O projeto está em desenvolvimento. A estrutura inicial, o Entity Framework Core e o ASP.NET Core Identity já estão configurados; os módulos odontológicos serão implementados incrementalmente.

## Tecnologias

- .NET 10 e ASP.NET Core
- Entity Framework Core 10
- SQL Server 2025
- ASP.NET Core Identity
- OpenAPI e Scalar
- xUnit
- Docker

## Estrutura do projeto

```text
Odonto.Backend/
├── Odonto.API/             # API HTTP e ponto de entrada
├── Odonto.Application/     # Casos de uso da aplicação
├── Odonto.Domain/          # Entidades e regras de negócio
├── Odonto.Infrastructure/  # Persistência, Identity e integrações
├── Odonto.IoC/             # Registro de dependências
└── Odonto.Tests/           # Testes automatizados
```

O fluxo principal de uma requisição é:

```text
HTTP → API → Application → Domain → Infrastructure → SQL Server
```

Uma descrição mais detalhada das camadas e do escopo está disponível no [README do backend](Odonto.Backend/README.md).

## Pré-requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0)
- Docker
- Git
- Opcional: Rider, DataGrip ou DBeaver para consultar o banco graficamente

Confira a instalação:

```bash
dotnet --version
docker --version
```

## Início rápido

### 1. Clone e restaure o projeto

```bash
git clone <URL_DO_REPOSITORIO>
cd clinicaodonto
dotnet restore Odonto.Backend/Odonto.Backend.sln
```

### 2. Inicie o SQL Server

Defina uma senha forte para o usuário `sa` e não a versione:

```bash
export ODONTO_SA_PASSWORD='SUA_SENHA_FORTE'

docker volume create odonto-sqlserver-data

docker run -d \
  --name sqlserver-dev \
  --restart unless-stopped \
  --workdir /var/opt/mssql \
  -e HOME=/var/opt/mssql \
  -e ACCEPT_EULA=Y \
  -e MSSQL_PID=Developer \
  -e MSSQL_SA_PASSWORD="$ODONTO_SA_PASSWORD" \
  -p 1433:1433 \
  -v odonto-sqlserver-data:/var/opt/mssql \
  mcr.microsoft.com/mssql/server:2025-latest
```

Verifique a inicialização:

```bash
docker ps --filter name=sqlserver-dev
docker logs --tail 100 sqlserver-dev
```

O servidor estará pronto quando o log mostrar `SQL Server is now ready for client connections`.

### 3. Configure a conexão local

A API usa a chave `ConnectionStrings:SqlServer`. Armazene-a com User Secrets:

```bash
dotnet user-secrets set "ConnectionStrings:SqlServer" \
  "Server=localhost,1433;Database=OdontoDB;User Id=sa;Password=$ODONTO_SA_PASSWORD;Encrypt=True;TrustServerCertificate=True" \
  --project Odonto.Backend/Odonto.API/Odonto.API.csproj
```

Para habilitar o envio dos códigos de redefinição de senha, configure também o
SMTP por User Secrets. Não armazene a senha do e-mail em `appsettings.json`:

```bash
dotnet user-secrets set "EmailSmtpCliente" "smtp.exemplo.com" --project Odonto.Backend/Odonto.API/Odonto.API.csproj
dotnet user-secrets set "EmailSmtpPorta" "587" --project Odonto.Backend/Odonto.API/Odonto.API.csproj
dotnet user-secrets set "EmailSmtpUsuario" "sistema@exemplo.com" --project Odonto.Backend/Odonto.API/Odonto.API.csproj
dotnet user-secrets set "EmailSmtpSenha" "SENHA_DO_SMTP" --project Odonto.Backend/Odonto.API/Odonto.API.csproj
```

### 4. Instale a ferramenta do EF Core

```bash
dotnet tool install --global dotnet-ef --version 10.0.9
```

Se ela já estiver instalada:

```bash
dotnet tool update --global dotnet-ef --version 10.0.9
```

Certifique-se de que `~/.dotnet/tools` está no `PATH` do shell.

### 5. Aplique as migrations

```bash
cd Odonto.Backend/Odonto.API

dotnet ef database update \
  --project ../Odonto.Infrastructure/Odonto.Infrastructure.csproj \
  --startup-project Odonto.API.csproj \
  --context AppDbContext
```

Esse comando cria o banco `OdontoDB` e aplica as migrations pendentes.

### 6. Execute a API

```bash
dotnet run --project Odonto.API.csproj
```

Em ambiente de desenvolvimento, acesse a documentação interativa no endereço `/scalar` exibido pelo terminal.

## Consultando o banco no Fedora

Abra o `sqlcmd` dentro do próprio container:

```bash
docker exec -it sqlserver-dev \
  /opt/mssql-tools18/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P "$ODONTO_SA_PASSWORD" \
  -C \
  -d OdontoDB
```

Exemplos de consultas:

```sql
SELECT TABLE_SCHEMA, TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
ORDER BY TABLE_SCHEMA, TABLE_NAME;
GO

SELECT * FROM AspNetUsers;
GO
```

Para sair do `sqlcmd`:

```text
QUIT
```

Em ferramentas gráficas, utilize:

| Campo | Valor |
|---|---|
| Host | `localhost` |
| Porta | `1433` |
| Banco | `OdontoDB` |
| Usuário | `sa` |
| Criptografia | habilitada |
| Confiar no certificado do servidor | habilitado em desenvolvimento |

## Comandos úteis

```bash
# Compilar
dotnet build Odonto.Backend/Odonto.Backend.sln

# Executar os testes
dotnet test --project Odonto.Backend/Odonto.Tests/Odonto.Tests.csproj

# Executar os testes com cobertura (formato Cobertura)
dotnet test --project Odonto.Backend/Odonto.Tests/Odonto.Tests.csproj -- \
  --coverage --coverage-output-format cobertura

# Ver as migrations
dotnet ef migrations list \
  --project Odonto.Backend/Odonto.Infrastructure/Odonto.Infrastructure.csproj \
  --startup-project Odonto.Backend/Odonto.API/Odonto.API.csproj \
  --context AppDbContext

# Parar e iniciar o banco
docker stop sqlserver-dev
docker start sqlserver-dev
```

## Segurança

- Não versione senhas, tokens ou connection strings com credenciais.
- Use User Secrets durante o desenvolvimento local.
- O uso de `TrustServerCertificate=True` é indicado somente para o ambiente local.
- Antes de publicar, configure os segredos por variáveis de ambiente ou por um cofre de segredos.

## Licença

Consulte o arquivo [LICENSE](LICENSE).
