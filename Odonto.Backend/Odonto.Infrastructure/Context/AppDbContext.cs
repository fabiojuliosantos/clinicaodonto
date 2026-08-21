using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Odonto.Domain.Equipe;
using Odonto.Infrastructure.User;

namespace Odonto.Infrastructure.Context;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public DbSet<Funcionario> Funcionarios => Set<Funcionario>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
