using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Odonto.Domain.Equipe;

namespace Odonto.Infrastructure.Configuration;

public sealed class FuncionarioConfiguration : IEntityTypeConfiguration<Funcionario>
{
    public void Configure(EntityTypeBuilder<Funcionario> builder)
    {
        builder.ToTable("Funcionarios");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.NomeCompleto)
            .IsRequired()
            .HasMaxLength(Funcionario.TamanhoMaximoNome);

        builder.Property(f => f.NomeExibicao)
            .IsRequired()
            .HasMaxLength(Funcionario.TamanhoMaximoNome);

        builder.Property(f => f.Telefone)
            .HasMaxLength(Funcionario.TamanhoMaximoTelefone);

        builder.Property(f => f.FotoKey)
            .HasMaxLength(Funcionario.TamanhoMaximoFotoKey);
    }
}
