using Microsoft.AspNetCore.Identity;

namespace Odonto.Infrastructure.User;

public class AppUser : IdentityUser
{
    public Guid FuncionarioId { get; set; }
    public bool Ativo { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiracao { get; set; }
    public string? TokenResetarSenha { get; set; }
    public DateTime? TempoExpiracaoResetarSenha { get; set; }
}
