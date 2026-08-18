namespace Odonto.Application.DTO.Autenticacao;

public sealed record ResultadoRegistroDTO(
    bool Sucesso,
    IReadOnlyCollection<string> Erros);
