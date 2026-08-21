using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Odonto.Infrastructure.Services;

namespace Odonto.IoC.DI;

public static class InicializacaoAplicacao
{
    public static async Task ProvisionarAdministradorInicialAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(services);

        var configuration = services.GetRequiredService<IConfiguration>();
        if (!configuration.GetValue<bool>("Bootstrap:Enabled"))
        {
            return;
        }

        await using var scope = services.CreateAsyncScope();
        var seeder = scope.ServiceProvider
            .GetRequiredService<AdministradorInicialSeeder>();

        await seeder.ExecutarAsync(cancellationToken);
    }
}
