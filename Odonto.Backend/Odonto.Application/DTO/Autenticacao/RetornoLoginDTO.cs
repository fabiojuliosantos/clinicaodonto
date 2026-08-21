namespace Odonto.Application.DTO.Autenticacao;

public class RetornoLoginDTO
{
    public string Token {get; set;}
    public string RefreshToken {get; set;}
    public DateTime Expiracao {get; set;}
}
