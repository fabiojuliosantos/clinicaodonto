using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Odonto.Application.DTO.Autenticacao;
using Odonto.Application.Interfaces;
using Xunit;

namespace Odonto.Tests.API;

public sealed class RecuperacaoSenhaApiTests
{
    [Fact]
    public async Task RedefinirSenha_DtoInvalido_RetornaBadRequestSemChamarServico()
    {
        var service = CriarServico();
        using var factory = CriarFactory(service);
        using var client = CriarClient(factory);

        var response = await client.PostAsJsonAsync(
            "/api/Autenticacao/redefinir-senha",
            new { email = "email-invalido" },
            TestCancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        service.Verify(
            item => item.RedefinirSenhaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RedefinirSenha_ContaExistenteOuNao_RetornaMesmaRespostaGenerica()
    {
        var service = CriarServico();
        service
            .Setup(item => item.RedefinirSenhaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        using var factory = CriarFactory(service);
        using var client = CriarClient(factory);

        var primeira = await client.PostAsJsonAsync(
            "/api/Autenticacao/redefinir-senha",
            new { email = "existente@exemplo.com" },
            TestCancellationToken);
        var segunda = await client.PostAsJsonAsync(
            "/api/Autenticacao/redefinir-senha",
            new { email = "ausente@exemplo.com" },
            TestCancellationToken);

        Assert.Equal(HttpStatusCode.Accepted, primeira.StatusCode);
        Assert.Equal(primeira.StatusCode, segunda.StatusCode);
        Assert.Equal(
            await primeira.Content.ReadAsStringAsync(TestCancellationToken),
            await segunda.Content.ReadAsStringAsync(TestCancellationToken));
        Assert.DoesNotContain(
            "existente@exemplo.com",
            await primeira.Content.ReadAsStringAsync(TestCancellationToken));
    }

    [Fact]
    public async Task AtualizarSenha_TokenInvalido_RetornaUnauthorizedGenerico()
    {
        var service = CriarServico();
        service
            .Setup(item => item.AtualizarSenhaAsync(It.IsAny<TrocarSenhaDTO>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("informação interna"));
        using var factory = CriarFactory(service);
        using var client = CriarClient(factory);

        var response = await client.PostAsJsonAsync(
            "/api/Autenticacao/atualizar-senha",
            NovaTrocaSenha(),
            TestCancellationToken);
        var body = await response.Content.ReadAsStringAsync(TestCancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Contains("inválido ou expirou", body);
        Assert.DoesNotContain("informação interna", body);
    }

    [Fact]
    public async Task AtualizarSenha_ErroInesperado_RetornaErroInternoSemDetalhes()
    {
        var service = CriarServico();
        service
            .Setup(item => item.AtualizarSenhaAsync(It.IsAny<TrocarSenhaDTO>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("segredo da infraestrutura"));
        using var factory = CriarFactory(service);
        using var client = CriarClient(factory);

        var response = await client.PostAsJsonAsync(
            "/api/Autenticacao/atualizar-senha",
            NovaTrocaSenha(),
            TestCancellationToken);
        var body = await response.Content.ReadAsStringAsync(TestCancellationToken);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.DoesNotContain("segredo da infraestrutura", body);
    }

    [Fact]
    public async Task RedefinirSenha_AposCincoTentativas_RetornaTooManyRequests()
    {
        var service = CriarServico();
        service
            .Setup(item => item.RedefinirSenhaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        using var factory = CriarFactory(service);
        using var client = CriarClient(factory);

        for (var tentativa = 0; tentativa < 5; tentativa++)
        {
            var permitida = await client.PostAsJsonAsync(
                "/api/Autenticacao/redefinir-senha",
                new { email = "funcionario@exemplo.com" },
                TestCancellationToken);
            Assert.Equal(HttpStatusCode.Accepted, permitida.StatusCode);
        }

        var bloqueada = await client.PostAsJsonAsync(
            "/api/Autenticacao/redefinir-senha",
            new { email = "funcionario@exemplo.com" },
            TestCancellationToken);

        Assert.Equal(HttpStatusCode.TooManyRequests, bloqueada.StatusCode);
        service.Verify(
            item => item.RedefinirSenhaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Exactly(5));
    }

    private static Mock<IAutenticacaoService> CriarServico() => new(MockBehavior.Strict);

    private static WebApplicationFactory<Program> CriarFactory(Mock<IAutenticacaoService> service)
    {
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting(
                "ConnectionStrings:SqlServer",
                "Server=localhost;Database=OdontoTests;User Id=tests;Password=tests;TrustServerCertificate=True");
            builder.UseSetting("Jwt:Key", "chave-de-teste-com-tamanho-suficiente-123456");
            builder.UseSetting("Jwt:Issuer", "Odonto.Tests");
            builder.UseSetting("Jwt:Audience", "Odonto.Tests");
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:SqlServer"] = "Server=localhost;Database=OdontoTests;User Id=tests;Password=tests;TrustServerCertificate=True",
                    ["Jwt:Key"] = "chave-de-teste-com-tamanho-suficiente-123456",
                    ["Jwt:Issuer"] = "Odonto.Tests",
                    ["Jwt:Audience"] = "Odonto.Tests"
                });
            });
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IAutenticacaoService>();
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

    private static TrocarSenhaDTO NovaTrocaSenha() => new()
    {
        Email = "funcionario@exemplo.com",
        NovaSenha = "NovaSenha123!",
        Token = "123456"
    };

    private static CancellationToken TestCancellationToken => TestContext.Current.CancellationToken;
}
