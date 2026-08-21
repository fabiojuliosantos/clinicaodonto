using System.ComponentModel.DataAnnotations;

namespace Odonto.Application.DTO.Autenticacao;

public sealed class SolicitarRedefinicaoSenhaDTO
{
    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    public required string Email { get; init; }
}
