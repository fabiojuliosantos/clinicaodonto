using Microsoft.EntityFrameworkCore;
using Odonto.Application.DTO;
using Odonto.Application.Interfaces;
using Odonto.Infrastructure.Context;

namespace Odonto.Infrastructure.Services;

public sealed class MeuPerfilService(AppDbContext context) : IMeuPerfilService
{
    private const string UrlFotoPerfil = "/api/me/foto";

    public Task<MeuPerfilDTO?> ObterAsync(
        Guid funcionarioId,
        CancellationToken cancellationToken = default)
    {
        ValidarFuncionarioId(funcionarioId);

        return (
            from usuario in context.Users.AsNoTracking()
            join funcionario in context.Funcionarios.AsNoTracking()
                on usuario.FuncionarioId equals funcionario.Id
            where usuario.FuncionarioId == funcionarioId && usuario.Ativo
            select new MeuPerfilDTO(
                funcionario.Id,
                funcionario.NomeCompleto,
                funcionario.NomeExibicao,
                usuario.Email ?? string.Empty,
                funcionario.Telefone,
                funcionario.FotoKey == null ? null : UrlFotoPerfil))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<MeuPerfilDTO?> AtualizarAsync(
        Guid funcionarioId,
        AtualizarMeuPerfilDTO dto,
        CancellationToken cancellationToken = default)
    {
        ValidarFuncionarioId(funcionarioId);
        ArgumentNullException.ThrowIfNull(dto);

        if (dto.NomeExibicao is null && !dto.TelefoneFoiInformado)
        {
            throw new ArgumentException(
                "Informe ao menos um campo para atualizar.",
                nameof(dto));
        }

        var usuario = await context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.FuncionarioId == funcionarioId && item.Ativo,
                cancellationToken);

        if (usuario is null)
        {
            return null;
        }

        var funcionario = await context.Funcionarios
            .SingleOrDefaultAsync(item => item.Id == funcionarioId, cancellationToken);

        if (funcionario is null)
        {
            return null;
        }

        funcionario.AtualizarPerfil(
            dto.NomeExibicao ?? funcionario.NomeExibicao,
            dto.TelefoneFoiInformado ? dto.Telefone : funcionario.Telefone);

        await context.SaveChangesAsync(cancellationToken);

        return new MeuPerfilDTO(
            funcionario.Id,
            funcionario.NomeCompleto,
            funcionario.NomeExibicao,
            usuario.Email ?? string.Empty,
            funcionario.Telefone,
            funcionario.FotoKey is null ? null : UrlFotoPerfil);
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
