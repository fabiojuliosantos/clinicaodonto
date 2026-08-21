using Odonto.Application.DTO;

namespace Odonto.Application.Interfaces;

public interface IFotoPerfilService
{
    Task<FotoPerfilDTO?> AtualizarAsync(
        Guid funcionarioId,
        Stream conteudo,
        string contentType,
        long tamanho,
        CancellationToken cancellationToken = default);

    Task<ArquivoFotoPerfilDTO?> ObterAsync(
        Guid funcionarioId,
        CancellationToken cancellationToken = default);

    Task<bool> RemoverAsync(
        Guid funcionarioId,
        CancellationToken cancellationToken = default);
}
