using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Odonto.Domain.Equipe;
using Odonto.Infrastructure.User;

namespace Odonto.Infrastructure.Configuration;

public sealed class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.Property(usuario => usuario.Ativo)
            .IsRequired();

        builder
            .HasOne<Funcionario>()
            .WithOne()
            .HasForeignKey<AppUser>(usuario => usuario.FuncionarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
