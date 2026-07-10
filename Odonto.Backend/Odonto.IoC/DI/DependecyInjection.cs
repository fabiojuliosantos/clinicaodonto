using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Odonto.IoC.DI;

public static class DependecyInjection
{
    public static IServiceCollection ResolveDependecies(this IServiceCollection service, IConfiguration config)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStringsSqlServer");

        return service;
    }
}