using Odonto.Application.DTO;

namespace Odonto.Application.Interfaces;

public interface IMeuPerfilService
{
    Task<MeuPerfilDTO?> ObterAsync(
        Guid funcionarioId,
        CancellationToken cancellationToken = default);

    Task<MeuPerfilDTO?> AtualizarAsync(
        Guid funcionarioId,
        AtualizarMeuPerfilDTO dto,
        CancellationToken cancellationToken = default);
}
