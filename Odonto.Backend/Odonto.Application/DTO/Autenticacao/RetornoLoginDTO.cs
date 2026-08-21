namespace Odonto.Application.DTO.Autenticacao;

public sealed class RetornoLoginDTO
{
    public required string Token { get; init; }
    public required string RefreshToken { get; init; }
    public DateTime Expiracao { get; init; }
}
