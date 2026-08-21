namespace Odonto.Application.DTO.Autenticacao;

public sealed class LoginDTO
{
    public required string Email { get; init; }
    public required string Senha { get; init; }
}
