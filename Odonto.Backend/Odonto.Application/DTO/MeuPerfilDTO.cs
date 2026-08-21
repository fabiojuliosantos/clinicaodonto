namespace Odonto.Application.DTO;

public sealed record MeuPerfilDTO(
    Guid Id,
    string NomeCompleto,
    string NomeExibicao,
    string Email,
    string? Telefone,
    string? FotoKey);
