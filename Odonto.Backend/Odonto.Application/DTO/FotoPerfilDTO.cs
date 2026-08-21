namespace Odonto.Application.DTO;

public sealed record FotoPerfilDTO(string Url);

public sealed record ArquivoFotoPerfilDTO(
    Stream Conteudo,
    string ContentType);
