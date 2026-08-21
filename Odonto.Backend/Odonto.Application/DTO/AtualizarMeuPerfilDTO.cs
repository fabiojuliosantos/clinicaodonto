using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Odonto.Domain.Equipe;

namespace Odonto.Application.DTO;

public sealed class AtualizarMeuPerfilDTO
{
    private string? _telefone;

    [MaxLength(Funcionario.TamanhoMaximoNome)]
    public string? NomeExibicao { get; init; }

    [MaxLength(Funcionario.TamanhoMaximoTelefone)]
    public string? Telefone
    {
        get => _telefone;
        init
        {
            _telefone = value;
            TelefoneFoiInformado = true;
        }
    }

    [JsonIgnore]
    public bool TelefoneFoiInformado { get; private set; }
}
