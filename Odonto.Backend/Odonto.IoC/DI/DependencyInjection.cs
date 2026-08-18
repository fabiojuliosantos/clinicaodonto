using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Odonto.Application.Interfaces;
using Odonto.Infrastructure.Context;
using Odonto.Infrastructure.Services;
using Odonto.Infrastructure.User;

namespace Odonto.IoC.DI;

public static class DependencyInjection
{
    public static IServiceCollection ResolveDependencies(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        
        var connectionString = configuration.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException(
                "A connection string 'ConnectionStrings:SqlServer' nao foi configurada.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddDataProtection();
        
        services
            .AddIdentityCore<AppUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        // O IoC conecta o contrato do Application à implementação da Infrastructure.
        // Scoped mantém o serviço no mesmo ciclo de vida do DbContext e do UserManager.
        services.AddScoped<IAutenticacaoService, AutenticacaoService>();

        return services;
    }
}
