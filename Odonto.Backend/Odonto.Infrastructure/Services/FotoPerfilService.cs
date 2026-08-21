using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Odonto.Application.DTO;
using Odonto.Application.Interfaces;
using Odonto.Infrastructure.Context;

namespace Odonto.Infrastructure.Services;

public sealed class FotoPerfilService(
    AppDbContext context,
    IArmazenamentoFotoPerfil armazenamento,
    ILogger<FotoPerfilService> logger) : IFotoPerfilService
{
    private const string UrlFotoPerfil = "/api/me/foto";

    public async Task<FotoPerfilDTO?> AtualizarAsync(
        Guid funcionarioId,
        Stream conteudo,
        string contentType,
        long tamanho,
        CancellationToken cancellationToken = default)
    {
        ValidarFuncionarioId(funcionarioId);
        ArgumentNullException.ThrowIfNull(conteudo);

        var funcionario = await ObterFuncionarioAtivoAsync(funcionarioId, cancellationToken);
        if (funcionario is null)
        {
            return null;
        }

        var fotoKeyAnterior = funcionario.FotoKey;
        var novaFotoKey = await armazenamento.SalvarAsync(
            conteudo,
            contentType,
            tamanho,
            cancellationToken);

        funcionario.AtualizarFoto(novaFotoKey);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await TentarRemoverArquivoAsync(novaFotoKey, CancellationToken.None);
            throw;
        }

        if (!string.IsNullOrWhiteSpace(fotoKeyAnterior)
            && !string.Equals(fotoKeyAnterior, novaFotoKey, StringComparison.Ordinal))
        {
            await TentarRemoverArquivoAsync(fotoKeyAnterior, CancellationToken.None);
        }

        return new FotoPerfilDTO(UrlFotoPerfil);
    }

    public async Task<ArquivoFotoPerfilDTO?> ObterAsync(
        Guid funcionarioId,
        CancellationToken cancellationToken = default)
    {
        ValidarFuncionarioId(funcionarioId);

        var fotoKey = await (
                from usuario in context.Users.AsNoTracking()
                join funcionario in context.Funcionarios.AsNoTracking()
                    on usuario.FuncionarioId equals funcionario.Id
                where usuario.FuncionarioId == funcionarioId && usuario.Ativo
                select funcionario.FotoKey)
            .SingleOrDefaultAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(fotoKey))
        {
            return null;
        }

        var arquivo = await armazenamento.ObterAsync(fotoKey, cancellationToken);
        return arquivo is null
            ? null
            : new ArquivoFotoPerfilDTO(arquivo.Conteudo, arquivo.ContentType);
    }

    public async Task<bool> RemoverAsync(
        Guid funcionarioId,
        CancellationToken cancellationToken = default)
    {
        ValidarFuncionarioId(funcionarioId);

        var funcionario = await ObterFuncionarioAtivoAsync(funcionarioId, cancellationToken);
        if (funcionario is null)
        {
            return false;
        }

        var fotoKey = funcionario.FotoKey;
        if (string.IsNullOrWhiteSpace(fotoKey))
        {
            return true;
        }

        funcionario.RemoverFoto();
        await context.SaveChangesAsync(cancellationToken);
        await TentarRemoverArquivoAsync(fotoKey, CancellationToken.None);
        return true;
    }

    private async Task<Odonto.Domain.Equipe.Funcionario?> ObterFuncionarioAtivoAsync(
        Guid funcionarioId,
        CancellationToken cancellationToken)
    {
        var usuarioAtivo = await context.Users
            .AsNoTracking()
            .AnyAsync(
                usuario => usuario.FuncionarioId == funcionarioId && usuario.Ativo,
                cancellationToken);

        return usuarioAtivo
            ? await context.Funcionarios.SingleOrDefaultAsync(
                funcionario => funcionario.Id == funcionarioId,
                cancellationToken)
            : null;
    }

    private async Task TentarRemoverArquivoAsync(
        string fotoKey,
        CancellationToken cancellationToken)
    {
        try
        {
            await armazenamento.RemoverAsync(fotoKey, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Não foi possível remover o arquivo de foto de perfil {FotoKey}.",
                fotoKey);
        }
    }

    private static void ValidarFuncionarioId(Guid funcionarioId)
    {
        if (funcionarioId == Guid.Empty)
        {
            throw new ArgumentException(
                "O identificador do funcionário é obrigatório.",
                nameof(funcionarioId));
        }
    }
}
