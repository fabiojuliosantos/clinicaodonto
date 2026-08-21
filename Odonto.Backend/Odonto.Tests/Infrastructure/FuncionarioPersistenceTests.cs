using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Odonto.Domain.Equipe;
using Odonto.Infrastructure.Context;
using Odonto.Infrastructure.User;
using Xunit;

namespace Odonto.Tests.Infrastructure;

public sealed class FuncionarioPersistenceTests
{
    [Fact]
    public void Modelo_ConfiguraVinculoUmParaUmComExclusaoRestrita()
    {
        using var context = CriarContexto();
        var usuario = context.Model.FindEntityType(typeof(AppUser));
        var relacionamento = Assert.Single(
            usuario!.GetForeignKeys(),
            chave => chave.PrincipalEntityType.ClrType == typeof(Funcionario));

        Assert.True(relacionamento.IsUnique);
        Assert.True(relacionamento.IsRequired);
        Assert.Equal(DeleteBehavior.Restrict, relacionamento.DeleteBehavior);
    }

    [Fact]
    public void MigrationDeFuncionario_EstaRegistradaEGeraScriptSql()
    {
        using var context = CriarContexto();
        var migrationsAssembly = context.GetService<IMigrationsAssembly>();
        var migrator = context.GetService<IMigrator>();

        Assert.Contains(
            "20260821180000_ModelagemFuncionario",
            migrationsAssembly.Migrations.Keys);

        var script = migrator.GenerateScript();
        Assert.Contains("CREATE TABLE [Funcionarios]", script);
        Assert.Contains("FK_AspNetUsers_Funcionarios_FuncionarioId", script);
    }

    private static AppDbContext CriarContexto()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(
                "Server=localhost;Database=OdontoModelTests;User Id=tests;Password=tests;TrustServerCertificate=True")
            .Options;

        return new AppDbContext(options);
    }
}
