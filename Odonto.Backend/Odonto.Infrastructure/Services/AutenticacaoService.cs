using System.IdentityModel.Tokens.Jwt;
using System.Globalization;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Odonto.Application;
using Odonto.Application.DTO;
using Odonto.Application.DTO.Autenticacao;
using Odonto.Application.Interfaces;
using Odonto.Infrastructure.User;

namespace Odonto.Infrastructure.Services;

public sealed class AutenticacaoService(
    UserManager<AppUser> userManager,
    IConfiguration config,
    IEmailService emailService,
    ILogger<AutenticacaoService> logger) : IAutenticacaoService
{
    private readonly IConfiguration _config = config;
    private readonly IEmailService _emailService = emailService;

    #region Cadastrar Usuario
    public async Task<ResultadoRegistroDTO> CadastrarUsuarioAsync(
        RegistrarDTO dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        cancellationToken.ThrowIfCancellationRequested();
        if (dto.FuncionarioId == Guid.Empty)
        {
            throw new ArgumentException("O funcionário vinculado é obrigatório.", nameof(dto));
        }

        // AppUser é um detalhe do ASP.NET Identity. A conversão acontece aqui para
        // manter o Application independente da Infrastructure.
        var usuario = new AppUser
        {
            UserName = dto.UserName,
            Email = dto.Email,
            FuncionarioId = dto.FuncionarioId,
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

        if (usuario is null || !usuario.Ativo)
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
            new (JwtRegisteredClaimNames.Sub, usuario.Id),
            new (ClaimTypes.Name, usuario.UserName ?? string.Empty),
            new (ClaimTypes.Email, usuario.Email ?? string.Empty),
            new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new (ClaimsSistema.FuncionarioId, usuario.FuncionarioId.ToString())
        };
        
        foreach (var role in userRoles) claims.Add(new Claim("role", role));

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
    private JwtSecurityToken CriarToken(IEnumerable<Claim> claims)
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

    #region Redefinir Senha
    public async Task<bool> RedefinirSenhaAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var usuario = await userManager.FindByEmailAsync(email);
            if (usuario is null)
            {
                return true;
            }

            cancellationToken.ThrowIfCancellationRequested();
            var token = RandomNumberGenerator
                .GetInt32(0, 1_000_000)
                .ToString("D6", CultureInfo.InvariantCulture);

            usuario.TokenResetarSenha = CalcularHashToken(token);
            usuario.TempoExpiracaoResetarSenha = DateTime.UtcNow.AddMinutes(10);

            var tokenPersistido = await userManager.UpdateAsync(usuario);
            if (!tokenPersistido.Succeeded)
            {
                logger.LogError(
                    "Não foi possível persistir o código de redefinição do usuário {UserId}.",
                    usuario.Id);
                return true;
            }

            var corpoEmail = new StringBuilder();

            corpoEmail.Append("<h1 style='color: #a29bfe;'>Olá!</h1>");
            corpoEmail.Append("<h2 style='color: #222f3e;'>Não responda esse e-mail.</h2>");
            corpoEmail.Append("<p style='color: #222f3e;'>Recebemos uma solicitação para redefinir sua senha.</p>");
            corpoEmail.Append($"<p style='color: #222f3e;'>Seu código de verificação é: <strong>{token}</strong></p>");
            corpoEmail.Append("<p style='color: #222f3e;'>Seu código de verificação é válido por <strong>dez minutos</strong>.</p>");
            corpoEmail.Append("<p style='color: #222f3e;'>Se você não solicitou a redefinição, ignore este e-mail.</p>");
            corpoEmail.Append("<hr />");
            corpoEmail.Append("<p style='color: #222f3e;'>Almeida Estética e Sorriso</p>");

            var emailRedefinicaoSenha = new EmailDTO
            {
                Destinatarios = [usuario.Email!],
                Assunto = "Redefinição de Senha - Almeida Estética e Sorriso",
                Conteudo = corpoEmail.ToString()
            };

            return await _emailService.EnviarEmailAsync(emailRedefinicaoSenha, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao processar uma solicitação de redefinição de senha.");
            return true;
        }
    }
    #endregion Redefinir Senha

    #region Atualizar Senha
    public async Task<bool> AtualizarSenhaAsync(TrocarSenhaDTO dto, CancellationToken cancellationToken = default)
    {
        var usuario = await userManager.FindByEmailAsync(dto.Email);
        if (usuario is null
            || usuario.TempoExpiracaoResetarSenha is null
            || usuario.TempoExpiracaoResetarSenha <= DateTime.UtcNow
            || !TokenCorresponde(dto.Token, usuario.TokenResetarSenha))
        {
            throw new UnauthorizedAccessException("Token inválido ou expirado.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        // Consome o código antes da troca para impedir reutilização e requisições concorrentes.
        usuario.TokenResetarSenha = null;
        usuario.TempoExpiracaoResetarSenha = null;
        var tokenConsumido = await userManager.UpdateAsync(usuario);
        if (!tokenConsumido.Succeeded)
        {
            throw new InvalidOperationException("Não foi possível validar o código de redefinição.");
        }

        var tokenIdentity = await userManager.GeneratePasswordResetTokenAsync(usuario);
        var senhaAtualizada = await userManager.ResetPasswordAsync(usuario, tokenIdentity, dto.NovaSenha);

        if (!senhaAtualizada.Succeeded)
        {
            var erros = string.Join(" ", senhaAtualizada.Errors.Select(erro => erro.Description));
            throw new InvalidOperationException(erros);
        }

        return true;
    }
    #endregion Atualizar Senha

    private static string CalcularHashToken(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hash);
    }

    private static bool TokenCorresponde(string token, string? hashArmazenado)
    {
        if (string.IsNullOrWhiteSpace(hashArmazenado))
        {
            return false;
        }

        try
        {
            var hashRecebido = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            var hashEsperado = Convert.FromBase64String(hashArmazenado);
            return CryptographicOperations.FixedTimeEquals(hashRecebido, hashEsperado);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
