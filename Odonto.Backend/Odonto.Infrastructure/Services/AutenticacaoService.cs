using Microsoft.AspNetCore.Identity;
using Odonto.Application.DTO.Autenticacao;
using Odonto.Application.Interfaces;
using Odonto.Infrastructure.User;

namespace Odonto.Infrastructure.Services;

public sealed class AutenticacaoService(
    UserManager<AppUser> userManager) : IAutenticacaoService
{
    public async Task<ResultadoRegistroDTO> CadastrarUsuarioAsync(
        RegistrarDTO dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        cancellationToken.ThrowIfCancellationRequested();

        // AppUser é um detalhe do ASP.NET Identity. A conversão acontece aqui para
        // manter o Application independente da Infrastructure.
        var usuario = new AppUser
        {
            UserName = dto.UserName,
            Email = dto.Email,
            Nome = dto.Nome,
            Ativo = true
        };

        // O UserManager cria o hash da senha e aplica as regras configuradas pelo
        // Identity; a senha em texto puro nunca é armazenada no AppUser.
        var resultado = await userManager.CreateAsync(usuario, dto.Password);

        // Os erros são convertidos para um tipo do Application, sem deixar que
        // IdentityResult atravesse o limite da Infrastructure.
        return new ResultadoRegistroDTO(
            resultado.Succeeded,
            resultado.Errors
                .Select(erro => erro.Description)
                .ToArray());
    }
}
