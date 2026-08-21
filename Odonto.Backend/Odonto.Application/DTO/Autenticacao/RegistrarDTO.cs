using System.ComponentModel.DataAnnotations;

namespace Odonto.Application.DTO.Autenticacao;

public sealed class RegistrarDTO
{
    // Este DTO representa somente os dados aceitos pelo caso de uso. Campos internos
    // do Identity, como tokens e estado de ativação, não podem ser enviados pelo cliente.
    [Required(ErrorMessage = "Nome de usuário é obrigatório")]
    public required string UserName { get; init; }

    [Required(ErrorMessage = "Senha é obrigatória")]
    public required string Password { get; init; }

    [Required(ErrorMessage = "E-mail é obrigatório")]
    [EmailAddress(ErrorMessage = "E-mail inválido")]
    public required string Email { get; init; }

    public Guid FuncionarioId { get; init; }
}
