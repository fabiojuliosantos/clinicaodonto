using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Odonto.Application.DTO;
using Odonto.Application.DTO.Autenticacao;
using Odonto.Application.Interfaces;
using Odonto.Infrastructure.Services;
using Odonto.Infrastructure.User;
using Xunit;

namespace Odonto.Tests.Infrastructure;

public sealed class AutenticacaoServiceTests
{
    [Fact]
    public async Task RedefinirSenhaAsync_UsuarioInexistente_NaoRevelaContaNemEnviaEmail()
    {
        var fixture = CriarFixture();
        fixture.UserManager
            .Setup(manager => manager.FindByEmailAsync("ausente@exemplo.com"))
            .ReturnsAsync((AppUser?)null);

        var resultado = await fixture.Service.RedefinirSenhaAsync("ausente@exemplo.com", TestCancellationToken);

        Assert.True(resultado);
        fixture.EmailService.Verify(
            service => service.EnviarEmailAsync(It.IsAny<EmailDTO>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RedefinirSenhaAsync_UsuarioExistente_PersisteSomenteHashDoCodigo()
    {
        var usuario = CriarUsuario();
        var fixture = CriarFixture();
        EmailDTO? emailEnviado = null;
        fixture.UserManager.Setup(manager => manager.FindByEmailAsync(usuario.Email!)).ReturnsAsync(usuario);
        fixture.UserManager.Setup(manager => manager.UpdateAsync(usuario)).ReturnsAsync(IdentityResult.Success);
        fixture.EmailService
            .Setup(service => service.EnviarEmailAsync(It.IsAny<EmailDTO>(), It.IsAny<CancellationToken>()))
            .Callback<EmailDTO, CancellationToken>((email, _) => emailEnviado = email)
            .ReturnsAsync(true);

        var resultado = await fixture.Service.RedefinirSenhaAsync(usuario.Email!, TestCancellationToken);

        Assert.True(resultado);
        Assert.NotNull(emailEnviado);
        var codigo = Regex.Match(emailEnviado.Conteudo, @"\b[0-9]{6}\b").Value;
        Assert.Matches("^[0-9]{6}$", codigo);
        Assert.NotEqual(codigo, usuario.TokenResetarSenha);
        Assert.Equal(CalcularHash(codigo), usuario.TokenResetarSenha);
        Assert.InRange(
            usuario.TempoExpiracaoResetarSenha!.Value,
            DateTime.UtcNow.AddMinutes(9),
            DateTime.UtcNow.AddMinutes(10));
    }

    [Fact]
    public async Task RedefinirSenhaAsync_FalhaAoPersistir_NaoEnviaCodigoInvalido()
    {
        var usuario = CriarUsuario();
        var fixture = CriarFixture();
        fixture.UserManager.Setup(manager => manager.FindByEmailAsync(usuario.Email!)).ReturnsAsync(usuario);
        fixture.UserManager
            .Setup(manager => manager.UpdateAsync(usuario))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Falha de persistência" }));

        var resultado = await fixture.Service.RedefinirSenhaAsync(usuario.Email!, TestCancellationToken);

        Assert.True(resultado);
        fixture.EmailService.Verify(
            service => service.EnviarEmailAsync(It.IsAny<EmailDTO>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RedefinirSenhaAsync_FalhaSmtp_MantemRespostaGenerica()
    {
        var usuario = CriarUsuario();
        var fixture = CriarFixture();
        fixture.UserManager.Setup(manager => manager.FindByEmailAsync(usuario.Email!)).ReturnsAsync(usuario);
        fixture.UserManager.Setup(manager => manager.UpdateAsync(usuario)).ReturnsAsync(IdentityResult.Success);
        fixture.EmailService
            .Setup(service => service.EnviarEmailAsync(It.IsAny<EmailDTO>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Segredo SMTP"));

        var resultado = await fixture.Service.RedefinirSenhaAsync(usuario.Email!, TestCancellationToken);

        Assert.True(resultado);
    }

    [Fact]
    public async Task AtualizarSenhaAsync_TokenExpirado_RejeitaSemAlterarSenha()
    {
        var usuario = CriarUsuario();
        usuario.TokenResetarSenha = CalcularHash("123456");
        usuario.TempoExpiracaoResetarSenha = DateTime.UtcNow.AddMinutes(-1);
        var fixture = CriarFixture();
        fixture.UserManager.Setup(manager => manager.FindByEmailAsync(usuario.Email!)).ReturnsAsync(usuario);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            fixture.Service.AtualizarSenhaAsync(CriarTrocaSenha("123456"), TestCancellationToken));

        fixture.UserManager.Verify(
            manager => manager.ResetPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task AtualizarSenhaAsync_TokenValido_ConsomeCodigoEAtualizaSenha()
    {
        var usuario = CriarUsuario();
        usuario.TokenResetarSenha = CalcularHash("123456");
        usuario.TempoExpiracaoResetarSenha = DateTime.UtcNow.AddMinutes(5);
        var fixture = CriarFixture();
        fixture.UserManager.Setup(manager => manager.FindByEmailAsync(usuario.Email!)).ReturnsAsync(usuario);
        fixture.UserManager.Setup(manager => manager.UpdateAsync(usuario)).ReturnsAsync(IdentityResult.Success);
        fixture.UserManager.Setup(manager => manager.GeneratePasswordResetTokenAsync(usuario)).ReturnsAsync("identity-token");
        fixture.UserManager
            .Setup(manager => manager.ResetPasswordAsync(usuario, "identity-token", "NovaSenha123!"))
            .ReturnsAsync(IdentityResult.Success);

        var resultado = await fixture.Service.AtualizarSenhaAsync(CriarTrocaSenha("123456"), TestCancellationToken);

        Assert.True(resultado);
        Assert.Null(usuario.TokenResetarSenha);
        Assert.Null(usuario.TempoExpiracaoResetarSenha);
    }

    [Fact]
    public async Task AtualizarSenhaAsync_SenhaRejeitada_PropagaPoliticaSemPermitirReuso()
    {
        var usuario = CriarUsuario();
        usuario.TokenResetarSenha = CalcularHash("123456");
        usuario.TempoExpiracaoResetarSenha = DateTime.UtcNow.AddMinutes(5);
        var fixture = CriarFixture();
        fixture.UserManager.Setup(manager => manager.FindByEmailAsync(usuario.Email!)).ReturnsAsync(usuario);
        fixture.UserManager.Setup(manager => manager.UpdateAsync(usuario)).ReturnsAsync(IdentityResult.Success);
        fixture.UserManager.Setup(manager => manager.GeneratePasswordResetTokenAsync(usuario)).ReturnsAsync("identity-token");
        fixture.UserManager
            .Setup(manager => manager.ResetPasswordAsync(usuario, "identity-token", "NovaSenha123!"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "A senha não atende à política." }));

        var erro = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            fixture.Service.AtualizarSenhaAsync(CriarTrocaSenha("123456"), TestCancellationToken));

        Assert.Contains("não atende à política", erro.Message);
        Assert.Null(usuario.TokenResetarSenha);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            fixture.Service.AtualizarSenhaAsync(CriarTrocaSenha("123456"), TestCancellationToken));
    }

    private static TestFixture CriarFixture()
    {
        var store = new Mock<IUserStore<AppUser>>();
        var userManager = new Mock<UserManager<AppUser>>(
            store.Object,
            null!,
            new PasswordHasher<AppUser>(),
            Array.Empty<IUserValidator<AppUser>>(),
            Array.Empty<IPasswordValidator<AppUser>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null!,
            NullLogger<UserManager<AppUser>>.Instance);
        var emailService = new Mock<IEmailService>();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "chave-de-teste-com-tamanho-suficiente-123456",
                ["Jwt:Issuer"] = "Odonto.Tests",
                ["Jwt:Audience"] = "Odonto.Tests"
            })
            .Build();

        var service = new AutenticacaoService(
            userManager.Object,
            configuration,
            emailService.Object,
            NullLogger<AutenticacaoService>.Instance);

        return new TestFixture(service, userManager, emailService);
    }

    private static AppUser CriarUsuario() => new()
    {
        Id = "usuario-1",
        UserName = "funcionario@exemplo.com",
        Email = "funcionario@exemplo.com",
        Nome = "Funcionário"
    };

    private static TrocarSenhaDTO CriarTrocaSenha(string token) => new()
    {
        Email = "funcionario@exemplo.com",
        NovaSenha = "NovaSenha123!",
        Token = token
    };

    private static string CalcularHash(string token) =>
        Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private static CancellationToken TestCancellationToken => TestContext.Current.CancellationToken;

    private sealed record TestFixture(
        AutenticacaoService Service,
        Mock<UserManager<AppUser>> UserManager,
        Mock<IEmailService> EmailService);
}
