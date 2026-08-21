using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Odonto.Domain.Equipe;
using Odonto.Infrastructure.Context;
using Odonto.Infrastructure.Services;
using Odonto.Infrastructure.User;
using Xunit;

namespace Odonto.Tests.Infrastructure;

public sealed class FotoPerfilServiceTests
{
    [Fact]
    public async Task Atualizar_SubstituiChaveNoFuncionarioERemoveArquivoAnterior()
    {
        await using var fixture = await CriarFixtureAsync();
        fixture.Funcionario.AtualizarFoto("foto-anterior.webp");
        await fixture.Context.SaveChangesAsync(TestCancellationToken);

        var armazenamento = new Mock<IArmazenamentoFotoPerfil>(MockBehavior.Strict);
        armazenamento
            .Setup(item => item.SalvarAsync(
                It.IsAny<Stream>(),
                "image/png",
                3,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("foto-nova.webp");
        armazenamento
            .Setup(item => item.RemoverAsync(
                "foto-anterior.webp",
                CancellationToken.None))
            .Returns(Task.CompletedTask);
        var service = CriarService(fixture.Context, armazenamento.Object);
        await using var conteudo = new MemoryStream([1, 2, 3]);

        var resultado = await service.AtualizarAsync(
            fixture.Funcionario.Id,
            conteudo,
            "image/png",
            conteudo.Length,
            TestCancellationToken);

        Assert.Equal("/api/me/foto", resultado?.Url);
        Assert.Equal("foto-nova.webp", fixture.Funcionario.FotoKey);
        armazenamento.VerifyAll();
    }

    [Fact]
    public async Task Remover_LimpaChaveNoBancoERemoveArquivo()
    {
        await using var fixture = await CriarFixtureAsync();
        fixture.Funcionario.AtualizarFoto("foto.webp");
        await fixture.Context.SaveChangesAsync(TestCancellationToken);

        var armazenamento = new Mock<IArmazenamentoFotoPerfil>(MockBehavior.Strict);
        armazenamento
            .Setup(item => item.RemoverAsync("foto.webp", CancellationToken.None))
            .Returns(Task.CompletedTask);
        var service = CriarService(fixture.Context, armazenamento.Object);

        var resultado = await service.RemoverAsync(
            fixture.Funcionario.Id,
            TestCancellationToken);

        Assert.True(resultado);
        Assert.Null(fixture.Funcionario.FotoKey);
        armazenamento.VerifyAll();
    }

    private static FotoPerfilService CriarService(
        AppDbContext context,
        IArmazenamentoFotoPerfil armazenamento) =>
        new(
            context,
            armazenamento,
            Mock.Of<ILogger<FotoPerfilService>>());

    private static async Task<Fixture> CriarFixtureAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(TestCancellationToken);
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync(TestCancellationToken);

        var funcionario = Funcionario.Criar("Julia Guerra", "Julia");
        var usuario = new AppUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "julia@exemplo.com",
            NormalizedUserName = "JULIA@EXEMPLO.COM",
            Email = "julia@exemplo.com",
            NormalizedEmail = "JULIA@EXEMPLO.COM",
            FuncionarioId = funcionario.Id,
            Ativo = true,
            SecurityStamp = Guid.NewGuid().ToString()
        };

        context.Funcionarios.Add(funcionario);
        context.Users.Add(usuario);
        await context.SaveChangesAsync(TestCancellationToken);
        return new Fixture(connection, context, funcionario);
    }

    private sealed class Fixture(
        SqliteConnection connection,
        AppDbContext context,
        Funcionario funcionario) : IAsyncDisposable
    {
        public AppDbContext Context { get; } = context;
        public Funcionario Funcionario { get; } = funcionario;

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    private static CancellationToken TestCancellationToken =>
        TestContext.Current.CancellationToken;
}
