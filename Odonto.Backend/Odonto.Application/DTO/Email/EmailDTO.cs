namespace Odonto.Application.DTO;

public sealed class EmailDTO
{
    public required IReadOnlyCollection<string> Destinatarios { get; init; }
    public required string Assunto { get; init; }
    public required string Conteudo { get; init; }
}
