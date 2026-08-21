using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Odonto.Application;
using Odonto.Application.DTO;
using Odonto.Application.Interfaces;

namespace Odonto.API.Controllers;

[ApiController]
[Authorize]
[Route("api/me")]
public sealed class MeuPerfilController(IMeuPerfilService meuPerfilService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<MeuPerfilDTO>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obter(CancellationToken cancellationToken)
    {
        if (!TentarObterFuncionarioId(out var funcionarioId))
        {
            return Unauthorized();
        }

        var perfil = await meuPerfilService.ObterAsync(funcionarioId, cancellationToken);
        return perfil is null ? NotFound() : Ok(perfil);
    }

    [HttpPatch]
    [ProducesResponseType<MeuPerfilDTO>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(
        [FromBody] AtualizarMeuPerfilDTO dto,
        CancellationToken cancellationToken)
    {
        if (!TentarObterFuncionarioId(out var funcionarioId))
        {
            return Unauthorized();
        }

        try
        {
            var perfil = await meuPerfilService.AtualizarAsync(
                funcionarioId,
                dto,
                cancellationToken);

            return perfil is null ? NotFound() : Ok(perfil);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Dados de perfil inválidos",
                Detail = exception.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    private bool TentarObterFuncionarioId(out Guid funcionarioId)
    {
        var claim = User.FindFirst(ClaimsSistema.FuncionarioId)?.Value;
        return Guid.TryParse(claim, out funcionarioId) && funcionarioId != Guid.Empty;
    }
}
