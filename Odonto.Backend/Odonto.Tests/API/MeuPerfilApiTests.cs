using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Odonto.Application;
using Odonto.Application.DTO;
using Odonto.Application.Interfaces;
using Xunit;

namespace Odonto.Tests.API;

public sealed class MeuPerfilApiTests
{
    private const string JwtKey = "chave-de-teste-com-tamanho-suficiente-123456";
    private const string JwtIssuer = "Odonto.Tests";
    private const string JwtAudience = "Odonto.Tests";
    private static readonly Guid FuncionarioId =
        Guid.Parse("f7904533-c772-4955-811e-4f499ce681af");

    [Fact]
    public async Task OpenApi_DeclaraBearerSomenteNasOperacoesProtegidas()
    {
        var service = new Mock<IMeuPerfilService>(MockBehavior.Strict);
        using var factory = CriarFactory(service);
        using var client = CriarClient(factory);

        using var document = await JsonDocument.ParseAsync(
            await client.GetStreamAsync("/openapi/v1.json", TestCancellationToken),
            cancellationToken: TestCancellationToken);
        var root = document.RootElement;

        var bearer = root
            .GetProperty("components")
            .GetProperty("securitySchemes")
            .GetProperty("Bearer");
        Assert.Equal("http", bearer.GetProperty("type").GetString());
        Assert.Equal("bearer", bearer.GetProperty("scheme").GetString());
        Assert.Equal("JWT", bearer.GetProperty("bearerFormat").GetString());

        var paths = root.GetProperty("paths");
        Assert.True(paths
            .GetProperty("/api/me")
            .GetProperty("get")
            .GetProperty("security")[0]
            .TryGetProperty("Bearer", out _));
        Assert.True(paths
            .GetProperty("/api/Autenticacao/cadastrar")
            .GetProperty("post")
            .GetProperty("security")[0]
            .TryGetProperty("Bearer", out _));
        Assert.False(paths
            .GetProperty("/api/Autenticacao/login")
            .GetProperty("post")
            .TryGetProperty("security", out _));
    }

    [Fact]
    public async Task Obter_SemJwt_RetornaUnauthorizedSemConsultarPerfil()
    {
        var service = new Mock<IMeuPerfilService>(MockBehavior.Strict);
        using var factory = CriarFactory(service);
        using var client = CriarClient(factory);

        var response = await client.GetAsync("/api/me", TestCancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Obter_JwtValido_UsaFuncionarioIdDoToken()
    {
        var perfil = NovoPerfil();
        var service = new Mock<IMeuPerfilService>(MockBehavior.Strict);
        service
            .Setup(item => item.ObterAsync(FuncionarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(perfil);
        using var factory = CriarFactory(service);
        using var client = CriarClient(factory);
        AdicionarJwt(client);

        var response = await client.GetAsync("/api/me", TestCancellationToken);
        var retorno = await response.Content.ReadFromJsonAsync<MeuPerfilDTO>(TestCancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(perfil, retorno);
        service.VerifyAll();
    }

    [Fact]
    public async Task Atualizar_JwtValido_AtualizaSomentePerfilDoFuncionarioDoToken()
    {
        var perfil = NovoPerfil() with { NomeExibicao = "Ju" };
        var service = new Mock<IMeuPerfilService>(MockBehavior.Strict);
        service
            .Setup(item => item.AtualizarAsync(
                FuncionarioId,
                It.Is<AtualizarMeuPerfilDTO>(dto =>
                    dto.NomeExibicao == "Ju"
                    && dto.Telefone == "81999999999"
                    && dto.TelefoneFoiInformado),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(perfil);
        using var factory = CriarFactory(service);
        using var client = CriarClient(factory);
        AdicionarJwt(client);

        var response = await client.PatchAsJsonAsync(
            "/api/me",
            new { nomeExibicao = "Ju", telefone = "81999999999" },
            TestCancellationToken);
        var retorno = await response.Content.ReadFromJsonAsync<MeuPerfilDTO>(TestCancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(perfil, retorno);
        service.VerifyAll();
    }

    [Fact]
    public async Task Atualizar_TelefoneNulo_RepresentaRemocaoDoTelefone()
    {
        var perfil = NovoPerfil() with { Telefone = null };
        var service = new Mock<IMeuPerfilService>(MockBehavior.Strict);
        service
            .Setup(item => item.AtualizarAsync(
                FuncionarioId,
                It.Is<AtualizarMeuPerfilDTO>(dto =>
                    dto.TelefoneFoiInformado && dto.Telefone == null),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(perfil);
        using var factory = CriarFactory(service);
        using var client = CriarClient(factory);
        AdicionarJwt(client);

        var response = await client.PatchAsJsonAsync(
            "/api/me",
            new { telefone = (string?)null },
            TestCancellationToken);
        var retorno = await response.Content.ReadFromJsonAsync<MeuPerfilDTO>(TestCancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Null(retorno?.Telefone);
        service.VerifyAll();
    }

    [Fact]
    public async Task Obter_JwtAssinadoComOutraChave_RetornaUnauthorized()
    {
        var service = new Mock<IMeuPerfilService>(MockBehavior.Strict);
        using var factory = CriarFactory(service);
        using var client = CriarClient(factory);
        AdicionarJwt(client, "outra-chave-com-tamanho-suficiente-para-testes-123456");

        var response = await client.GetAsync("/api/me", TestCancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CadastrarUsuario_SemJwt_RetornaUnauthorized()
    {
        var service = new Mock<IMeuPerfilService>(MockBehavior.Strict);
        using var factory = CriarFactory(service);
        using var client = CriarClient(factory);

        var response = await client.PostAsJsonAsync(
            "/api/Autenticacao/cadastrar",
            new
            {
                userName = "novo@exemplo.com",
                email = "novo@exemplo.com",
                password = "Senha123!",
                funcionarioId = FuncionarioId
            },
            TestCancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static WebApplicationFactory<Program> CriarFactory(Mock<IMeuPerfilService> service)
    {
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:SqlServer", "Server=localhost;Database=OdontoTests;User Id=tests;Password=tests;TrustServerCertificate=True");
            builder.UseSetting("Jwt:Key", JwtKey);
            builder.UseSetting("Jwt:Issuer", JwtIssuer);
            builder.UseSetting("Jwt:Audience", JwtAudience);
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:SqlServer"] = "Server=localhost;Database=OdontoTests;User Id=tests;Password=tests;TrustServerCertificate=True",
                    ["Jwt:Key"] = JwtKey,
                    ["Jwt:Issuer"] = JwtIssuer,
                    ["Jwt:Audience"] = JwtAudience
                });
            });
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IMeuPerfilService>();
                services.AddSingleton(service.Object);
            });
        });
    }

    private static HttpClient CriarClient(WebApplicationFactory<Program> factory) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

    private static void AdicionarJwt(HttpClient client, string key = JwtKey)
    {
        var token = new JwtSecurityToken(
            issuer: JwtIssuer,
            audience: JwtAudience,
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, "usuario-1"),
                new Claim(ClaimsSistema.FuncionarioId, FuncionarioId.ToString())
            ],
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            new JwtSecurityTokenHandler().WriteToken(token));
    }

    private static MeuPerfilDTO NovoPerfil() => new(
        FuncionarioId,
        "Julia Guerra",
        "Julia",
        "julia@exemplo.com",
        "81999999999",
        null);

    private static CancellationToken TestCancellationToken => TestContext.Current.CancellationToken;
}
