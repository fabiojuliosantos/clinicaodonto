using Odonto.Application.DTO.Autenticacao;

namespace Odonto.Application.Interfaces;

public interface IAutenticacaoService
{
    // O Application define o contrato; a implementação técnica com ASP.NET Identity
    // fica na Infrastructure e nunca expõe AppUser para as camadas internas.
    Task<ResultadoRegistroDTO> CadastrarUsuarioAsync(
        RegistrarDTO dto,
        CancellationToken cancellationToken = default);
}
