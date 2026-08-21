namespace Odonto.Infrastructure.Services;

public interface IArmazenamentoFotoPerfil
{
    Task<string> SalvarAsync(
        Stream conteudo,
        string contentType,
        long tamanho,
        CancellationToken cancellationToken = default);

    Task<ArquivoFotoArmazenada?> ObterAsync(
        string fotoKey,
        CancellationToken cancellationToken = default);

    Task RemoverAsync(
        string fotoKey,
        CancellationToken cancellationToken = default);
}

public sealed record ArquivoFotoArmazenada(
    Stream Conteudo,
    string ContentType);
