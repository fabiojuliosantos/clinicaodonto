using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Odonto.Application;
using Odonto.Application.DTO;
using Odonto.Application.Interfaces;

namespace Odonto.API.Controllers;

[ApiController]
[Authorize]
[Route("api/me")]
public sealed class MeuPerfilController(
    IMeuPerfilService meuPerfilService,
    IFotoPerfilService fotoPerfilService) : ControllerBase
{
    private const int LimiteRequisicaoMultipart = 3 * 1024 * 1024;

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

    [HttpPut("foto")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(LimiteRequisicaoMultipart)]
    [ProducesResponseType<FotoPerfilDTO>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status413PayloadTooLarge)]
    public async Task<IActionResult> AtualizarFoto(
        [FromForm] AtualizarFotoPerfilRequest request,
        CancellationToken cancellationToken)
    {
        if (!TentarObterFuncionarioId(out var funcionarioId))
        {
            return Unauthorized();
        }

        var foto = request.Foto;
        if (foto is null || foto.Length == 0)
        {
            return ProblemaFoto(
                StatusCodes.Status400BadRequest,
                "Foto de perfil inválida",
                "Informe um arquivo de foto.");
        }

        if (foto.Length > FotoPerfilConfiguracao.TamanhoMaximoBytes)
        {
            return ProblemaFoto(
                StatusCodes.Status413PayloadTooLarge,
                "Foto de perfil muito grande",
                $"A foto deve possuir no máximo {FotoPerfilConfiguracao.TamanhoMaximoBytes / 1024 / 1024} MB.");
        }

        try
        {
            await using var conteudo = foto.OpenReadStream();
            var resultado = await fotoPerfilService.AtualizarAsync(
                funcionarioId,
                conteudo,
                foto.ContentType,
                foto.Length,
                cancellationToken);

            return resultado is null ? NotFound() : Ok(resultado);
        }
        catch (FotoPerfilMuitoGrandeException exception)
        {
            return ProblemaFoto(
                StatusCodes.Status413PayloadTooLarge,
                "Foto de perfil muito grande",
                exception.Message);
        }
        catch (FotoPerfilInvalidaException exception)
        {
            return ProblemaFoto(
                StatusCodes.Status400BadRequest,
                "Foto de perfil inválida",
                exception.Message);
        }
    }

    [HttpGet("foto")]
    [Produces(FotoPerfilConfiguracao.ContentTypeFinal)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileStreamResult))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterFoto(CancellationToken cancellationToken)
    {
        if (!TentarObterFuncionarioId(out var funcionarioId))
        {
            return Unauthorized();
        }

        var arquivo = await fotoPerfilService.ObterAsync(
            funcionarioId,
            cancellationToken);
        if (arquivo is null)
        {
            return NotFound();
        }

        Response.Headers.CacheControl = "private, no-store";
        Response.Headers.Pragma = "no-cache";
        return File(arquivo.Conteudo, arquivo.ContentType);
    }

    [HttpDelete("foto")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoverFoto(CancellationToken cancellationToken)
    {
        if (!TentarObterFuncionarioId(out var funcionarioId))
        {
            return Unauthorized();
        }

        var removido = await fotoPerfilService.RemoverAsync(
            funcionarioId,
            cancellationToken);
        return removido ? NoContent() : NotFound();
    }

    private ObjectResult ProblemaFoto(int status, string titulo, string detalhe) =>
        StatusCode(status, new ProblemDetails
        {
            Title = titulo,
            Detail = detalhe,
            Status = status
        });

    private bool TentarObterFuncionarioId(out Guid funcionarioId)
    {
        var claim = User.FindFirst(ClaimsSistema.FuncionarioId)?.Value;
        return Guid.TryParse(claim, out funcionarioId) && funcionarioId != Guid.Empty;
    }
}

public sealed class AtualizarFotoPerfilRequest
{
    public IFormFile? Foto { get; init; }
}
