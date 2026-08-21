using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Odonto.Domain.Equipe;
using Odonto.Infrastructure.Context;
using Odonto.Infrastructure.User;

namespace Odonto.Infrastructure.Services;

public sealed class AdministradorInicialSeeder(
    AppDbContext context,
    UserManager<AppUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IConfiguration configuration,
    ILogger<AdministradorInicialSeeder> logger)
{
    public async Task ExecutarAsync(CancellationToken cancellationToken = default)
    {
        if (!configuration.GetValue<bool>("Bootstrap:Enabled"))
        {
            return;
        }

        await using var transaction = await context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        if (await context.Users.AsNoTracking().AnyAsync(cancellationToken))
        {
            logger.LogInformation(
                "Provisionamento inicial ignorado porque já existe uma conta cadastrada.");
            return;
        }

        var nomeCompleto = ObterConfiguracaoObrigatoria("Bootstrap:NomeCompleto");
        var nomeExibicao = ObterConfiguracaoObrigatoria("Bootstrap:NomeExibicao");
        var email = ObterConfiguracaoObrigatoria("Bootstrap:Email");
        var password = ObterConfiguracaoObrigatoria(
            "Bootstrap:Password",
            normalizar: false);
        var role = ObterConfiguracaoObrigatoria("Bootstrap:Role");

        var funcionario = Funcionario.Criar(nomeCompleto, nomeExibicao);
        context.Funcionarios.Add(funcionario);
        await context.SaveChangesAsync(cancellationToken);

        var usuario = new AppUser
        {
            UserName = email,
            Email = email,
            FuncionarioId = funcionario.Id,
            Ativo = true
        };

        var criacaoUsuario = await userManager.CreateAsync(usuario, password);
        ValidarResultadoIdentity(criacaoUsuario, "criar a conta inicial");

        if (!await roleManager.RoleExistsAsync(role))
        {
            var criacaoRole = await roleManager.CreateAsync(new IdentityRole(role));
            ValidarResultadoIdentity(criacaoRole, "criar a role inicial");
        }

        var atribuicaoRole = await userManager.AddToRoleAsync(usuario, role);
        ValidarResultadoIdentity(atribuicaoRole, "atribuir a role inicial");

        await transaction.CommitAsync(cancellationToken);

        logger.LogInformation(
            "Conta administrativa inicial provisionada com sucesso. UserId: {UserId}.",
            usuario.Id);
    }

    private string ObterConfiguracaoObrigatoria(
        string chave,
        bool normalizar = true)
    {
        var valor = configuration[chave];
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new InvalidOperationException(
                $"A configuração '{chave}' é obrigatória para o provisionamento inicial.");
        }

        return normalizar ? valor.Trim() : valor;
    }

    private static void ValidarResultadoIdentity(
        IdentityResult resultado,
        string operacao)
    {
        if (resultado.Succeeded)
        {
            return;
        }

        var erros = string.Join(
            " ",
            resultado.Errors.Select(erro => erro.Description));

        throw new InvalidOperationException(
            $"Não foi possível {operacao}. {erros}");
    }
}
