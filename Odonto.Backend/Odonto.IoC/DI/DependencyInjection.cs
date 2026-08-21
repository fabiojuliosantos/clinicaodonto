using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
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

        var jwtKey = configuration["Jwt:Key"];
        var jwtIssuer = configuration["Jwt:Issuer"];
        var jwtAudience = configuration["Jwt:Audience"];

        if (string.IsNullOrWhiteSpace(jwtKey)
            || string.IsNullOrWhiteSpace(jwtIssuer)
            || string.IsNullOrWhiteSpace(jwtAudience))
        {
            throw new InvalidOperationException(
                "As configurações Jwt:Key, Jwt:Issuer e Jwt:Audience são obrigatórias.");
        }

        if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
        {
            throw new InvalidOperationException(
                "A configuração Jwt:Key deve possuir ao menos 32 bytes.");
        }

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = "role"
                };
            });

        services.AddAuthorization();

        // O IoC conecta o contrato do Application à implementação da Infrastructure.
        // Scoped mantém o serviço no mesmo ciclo de vida do DbContext e do UserManager.
        services.AddScoped<IAutenticacaoService, AutenticacaoService>();
        services.AddScoped<IMeuPerfilService, MeuPerfilService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<AdministradorInicialSeeder>();
        return services;
    }
}
