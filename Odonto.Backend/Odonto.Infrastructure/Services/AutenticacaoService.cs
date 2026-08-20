using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Odonto.Application.DTO.Autenticacao;
using Odonto.Application.Interfaces;
using Odonto.Infrastructure.User;

namespace Odonto.Infrastructure.Services;

public sealed class AutenticacaoService(
    UserManager<AppUser> userManager,
    IConfiguration config) : IAutenticacaoService
{
    private readonly IConfiguration _config = config;

    #region Cadastrar Usuario
    public async Task<ResultadoRegistroDTO> CadastrarUsuarioAsync(
        RegistrarDTO dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        cancellationToken.ThrowIfCancellationRequested();

        // AppUser é um detalhe do ASP.NET Identity. A conversão acontece aqui para
        // manter o Application independente da Infrastructure.
        var usuario = new AppUser
        {
            UserName = dto.UserName,
            Email = dto.Email,
            Nome = dto.Nome,
            Ativo = true
        };

        // O UserManager cria o hash da senha e aplica as regras configuradas pelo
        // Identity; a senha em texto puro nunca é armazenada no AppUser.
        var resultado = await userManager.CreateAsync(usuario, dto.Password);

        // Os erros são convertidos para um tipo do Application, sem deixar que
        // IdentityResult atravesse o limite da Infrastructure.
        return new ResultadoRegistroDTO(
            resultado.Succeeded,
            resultado.Errors
                .Select(erro => erro.Description)
                .ToArray());
    }    
    #endregion Cadastrar Usuario

    #region Login
    public async Task<RetornoLoginDTO> Login(LoginDTO dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        cancellationToken.ThrowIfCancellationRequested();

        var usuario = await userManager.FindByEmailAsync(dto.Email);

        if (usuario is null)
        {
            throw new UnauthorizedAccessException("Email ou senha inválidos.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        var resultado = await userManager.CheckPasswordAsync(usuario, dto.Senha);
        
        if (!resultado)
        {
            throw new UnauthorizedAccessException("Email ou senha inválidos.");
        }

        var userRoles = await userManager.GetRolesAsync(usuario);
        
        var claims = new List<Claim>
        {
            new (ClaimTypes.Name, usuario.UserName ?? string.Empty),
            new (ClaimTypes.Email, usuario.Email ?? string.Empty),
            new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new ("Tipo", "Admin") 
        };
        
        foreach (var role in userRoles) claims.Add(new Claim(ClaimTypes.Role, role));

        var token = CriarToken(claims);

        var retornoLoginDTO = new RetornoLoginDTO
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            RefreshToken = GerarRefreshToken(),
            Expiracao = token.ValidTo
        };

        return retornoLoginDTO;

    }
    #endregion Login

    #region Criação de Token
    protected JwtSecurityToken CriarToken(IEnumerable<Claim> claims)
    {
        var chave = _config["Jwt:Key"] ?? throw new InvalidOperationException("Chave de criptografia não configurada.");
        
        var chaveSecreta = Encoding.UTF8.GetBytes(chave);
        
        var credenciais = new SigningCredentials(new SymmetricSecurityKey(chaveSecreta), SecurityAlgorithms.HmacSha256);

        var descricaoToken = new SecurityTokenDescriptor
        {
          Subject = new ClaimsIdentity(claims),
          Expires = DateTime.UtcNow.AddHours(2),  
          SigningCredentials = credenciais,
          Issuer = _config["Jwt:Issuer"],
          Audience = _config["Jwt:Audience"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var token = tokenHandler.CreateJwtSecurityToken(descricaoToken);

        return token;
    }
    #endregion Criação de Token

    #region Criação de Refresh Token
    private string GerarRefreshToken()
    {
        var bytesSeguranca = new byte[128];
        using var geradorRandomico = RandomNumberGenerator.Create();
        geradorRandomico.GetBytes(bytesSeguranca);
        return Convert.ToBase64String(bytesSeguranca);
    }
    #endregion Criação de Refresh Token
}
