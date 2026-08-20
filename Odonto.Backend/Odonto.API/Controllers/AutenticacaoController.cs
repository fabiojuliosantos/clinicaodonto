using Microsoft.AspNetCore.Mvc;
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
}
