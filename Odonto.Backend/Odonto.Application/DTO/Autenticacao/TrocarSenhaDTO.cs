using System.ComponentModel.DataAnnotations;

namespace Odonto.Application.DTO.Autenticacao;

public sealed class TrocarSenhaDTO
{
    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    public required string Email { get; init; }

    [Required(ErrorMessage = "A nova senha é obrigatória.")]
    public required string NovaSenha { get; init; }

    [Required(ErrorMessage = "O código de verificação é obrigatório.")]
    [RegularExpression("^[0-9]{6}$", ErrorMessage = "O código de verificação deve possuir seis dígitos.")]
    public required string Token { get; init; }
}
