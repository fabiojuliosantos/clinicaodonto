using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Odonto.Infrastructure.Context;
using Odonto.Infrastructure.Services;
using Odonto.Infrastructure.User;
using Xunit;

namespace Odonto.Tests.Infrastructure;

public sealed class AdministradorInicialSeederTests
{
    [Fact]
    public async Task ExecutarAsync_BancoVazio_CriaFuncionarioUsuarioERoleUmaUnicaVez()
    {
        await using var fixture = await CriarFixtureAsync(CriarConfiguracao());

        await fixture.Seeder.ExecutarAsync(TestCancellationToken);
        await fixture.Seeder.ExecutarAsync(TestCancellationToken);

        fixture.Context.ChangeTracker.Clear();
        var usuario = await fixture.Context.Users.SingleAsync(TestCancellationToken);
        var funcionario = await fixture.Context.Funcionarios.SingleAsync(TestCancellationToken);

        Assert.Equal("gestor.ti@exemplo.com", usuario.Email);
        Assert.Equal(funcionario.Id, usuario.FuncionarioId);
        Assert.True(usuario.Ativo);
        Assert.NotEqual("SenhaInicial123!", usuario.PasswordHash);
        Assert.Equal("Gestor de Tecnologia da Informação", funcionario.NomeCompleto);
        Assert.Equal("Gestor TI", funcionario.NomeExibicao);
        Assert.Equal(1, await fixture.Context.Users.CountAsync(TestCancellationToken));
        Assert.Equal(1, await fixture.Context.Funcionarios.CountAsync(TestCancellationToken));
        Assert.True(await fixture.UserManager.IsInRoleAsync(usuario, "AdministradorSistema"));
    }

    [Fact]
    public async Task ExecutarAsync_Desabilitado_NaoCriaDadosNemExigeCredenciais()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Bootstrap:Enabled"] = "false"
            })
            .Build();
        await using var fixture = await CriarFixtureAsync(configuration);

        await fixture.Seeder.ExecutarAsync(TestCancellationToken);

        Assert.False(await fixture.Context.Users.AnyAsync(TestCancellationToken));
        Assert.False(await fixture.Context.Funcionarios.AnyAsync(TestCancellationToken));
    }

    [Fact]
    public async Task ExecutarAsync_SenhaRejeitada_DesfazCriacaoDoFuncionario()
    {
        var configuration = CriarConfiguracao(new Dictionary<string, string?>
        {
            ["Bootstrap:Password"] = "fraca"
        });
        await using var fixture = await CriarFixtureAsync(configuration);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            fixture.Seeder.ExecutarAsync(TestCancellationToken));

        fixture.Context.ChangeTracker.Clear();
        Assert.False(await fixture.Context.Users.AnyAsync(TestCancellationToken));
        Assert.False(await fixture.Context.Funcionarios.AnyAsync(TestCancellationToken));
    }

    private static IConfiguration CriarConfiguracao(
        Dictionary<string, string?>? valoresSubstitutos = null)
    {
        var valores = new Dictionary<string, string?>
        {
            ["Bootstrap:Enabled"] = "true",
            ["Bootstrap:NomeCompleto"] = "Gestor de Tecnologia da Informação",
            ["Bootstrap:NomeExibicao"] = "Gestor TI",
            ["Bootstrap:Email"] = "gestor.ti@exemplo.com",
            ["Bootstrap:Password"] = "SenhaInicial123!",
            ["Bootstrap:Role"] = "AdministradorSistema"
        };

        if (valoresSubstitutos is not null)
        {
            foreach (var item in valoresSubstitutos)
            {
                valores[item.Key] = item.Value;
            }
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(valores)
            .Build();
    }

    private static async Task<SeederFixture> CriarFixtureAsync(
        IConfiguration configuration)
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(TestCancellationToken);

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(configuration);
        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connection));
        services
            .AddIdentityCore<AppUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();
        services.AddScoped<AdministradorInicialSeeder>();

        var provider = services.BuildServiceProvider();
        var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureCreatedAsync(TestCancellationToken);

        return new SeederFixture(
            connection,
            provider,
            scope,
            context,
            scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>(),
            scope.ServiceProvider.GetRequiredService<AdministradorInicialSeeder>());
    }

    private static CancellationToken TestCancellationToken =>
        TestContext.Current.CancellationToken;

    private sealed class SeederFixture(
        SqliteConnection connection,
        ServiceProvider provider,
        AsyncServiceScope scope,
        AppDbContext context,
        UserManager<AppUser> userManager,
        AdministradorInicialSeeder seeder) : IAsyncDisposable
    {
        public AppDbContext Context { get; } = context;
        public UserManager<AppUser> UserManager { get; } = userManager;
        public AdministradorInicialSeeder Seeder { get; } = seeder;

        public async ValueTask DisposeAsync()
        {
            await scope.DisposeAsync();
            await provider.DisposeAsync();
            await connection.DisposeAsync();
        }
    }
}
