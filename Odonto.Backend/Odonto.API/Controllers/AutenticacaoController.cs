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
}
