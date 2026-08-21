using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Odonto.Application.DTO.Autenticacao;
using Odonto.Application.Interfaces;

namespace Odonto.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AutenticacaoController : ControllerBase
{
    private readonly IAutenticacaoService _autenticacaoService;
    public AutenticacaoController(IAutenticacaoService autenticacaoService)
    {
        _autenticacaoService = autenticacaoService;
    }

    [HttpPost("cadastrar")]
    public async Task<IActionResult> CadastrarUsuario([FromBody] RegistrarDTO dto)
    {
        var resultado = await _autenticacaoService.CadastrarUsuarioAsync(dto);
        return Ok(resultado);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginDTO dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await _autenticacaoService.Login(dto, cancellationToken);
            return Ok(resultado);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Credenciais inválidas",
                Detail = "E-mail ou senha inválidos.",
                Status = StatusCodes.Status401Unauthorized
            });
        }
    }

    [HttpPost("redefinir-senha")]
    [EnableRateLimiting("password-recovery")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> RedefinirSenha(
        [FromBody] SolicitarRedefinicaoSenhaDTO dto,
        CancellationToken cancellationToken)
    {
        await _autenticacaoService.RedefinirSenhaAsync(dto.Email, cancellationToken);
        return Accepted(new
        {
            mensagem = "Se o e-mail estiver cadastrado, enviaremos um código de redefinição."
        });
    }

    [HttpPost("atualizar-senha")]
    [EnableRateLimiting("password-reset")]
    public async Task<IActionResult> AtualizarSenha(
        [FromBody] TrocarSenhaDTO dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var atualizacaoSenha = await _autenticacaoService.AtualizarSenhaAsync(dto, cancellationToken);
            return Ok(atualizacaoSenha);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Token inválido ou expirado",
                Detail = "O token fornecido é inválido ou expirou.",
                Status = StatusCodes.Status401Unauthorized
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Erro ao atualizar senha",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception)
        {
            return Problem(
                title: "Erro ao atualizar senha",
                detail: "Não foi possível atualizar a senha.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
