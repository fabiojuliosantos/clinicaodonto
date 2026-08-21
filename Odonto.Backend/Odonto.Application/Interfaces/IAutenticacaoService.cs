using Odonto.Application.DTO.Autenticacao;

namespace Odonto.Application.Interfaces;

public interface IAutenticacaoService
{
    // O Application define o contrato; a implementação técnica com ASP.NET Identity
    // fica na Infrastructure e nunca expõe AppUser para as camadas internas.
    Task<ResultadoRegistroDTO> CadastrarUsuarioAsync(
        RegistrarDTO dto,
        CancellationToken cancellationToken = default);
    Task<RetornoLoginDTO> Login(
        LoginDTO dto,
        CancellationToken cancellationToken = default);

    Task<bool> RedefinirSenhaAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<bool> AtualizarSenhaAsync(
        TrocarSenhaDTO dto,
        CancellationToken cancellationToken = default);
}
