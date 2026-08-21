namespace Odonto.Domain.Equipe;

public sealed class Funcionario
{
    public const int TamanhoMaximoNome = 100;
    public const int TamanhoMaximoTelefone = 30;
    public const int TamanhoMaximoFotoKey = 512;

    private Funcionario()
    {
    }

    private Funcionario(Guid id, string nomeCompleto, string nomeExibicao)
    {
        Id = id;
        NomeCompleto = NormalizarCampoObrigatorio(
            nomeCompleto,
            nameof(nomeCompleto),
            TamanhoMaximoNome);
        NomeExibicao = NormalizarCampoObrigatorio(
            nomeExibicao,
            nameof(nomeExibicao),
            TamanhoMaximoNome);
    }

    public Guid Id { get; private set; }
    public string NomeCompleto { get; private set; } = string.Empty;
    public string NomeExibicao { get; private set; } = string.Empty;
    public string? Telefone { get; private set; }
    public string? FotoKey { get; private set; }

    public static Funcionario Criar(string nomeCompleto, string nomeExibicao)
    {
        return new Funcionario(Guid.NewGuid(), nomeCompleto, nomeExibicao);
    }

    public void AtualizarPerfil(string nomeExibicao, string? telefone)
    {
        NomeExibicao = NormalizarCampoObrigatorio(
            nomeExibicao,
            nameof(nomeExibicao),
            TamanhoMaximoNome);
        Telefone = NormalizarCampoOpcional(
            telefone,
            nameof(telefone),
            TamanhoMaximoTelefone);
    }

    public void AtualizarFoto(string fotoKey)
    {
        FotoKey = NormalizarCampoObrigatorio(
            fotoKey,
            nameof(fotoKey),
            TamanhoMaximoFotoKey);
    }

    public void RemoverFoto()
    {
        FotoKey = null;
    }

    private static string NormalizarCampoObrigatorio(
        string campo,
        string nomeParametro,
        int tamanhoMaximo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(campo, nomeParametro);

        var campoNormalizado = campo.Trim();
        if (campoNormalizado.Length > tamanhoMaximo)
        {
            throw new ArgumentException(
                $"O campo deve possuir no máximo {tamanhoMaximo} caracteres.",
                nomeParametro);
        }

        return campoNormalizado;
    }

    private static string? NormalizarCampoOpcional(
        string? campo,
        string nomeParametro,
        int tamanhoMaximo)
    {
        if (string.IsNullOrWhiteSpace(campo))
        {
            return null;
        }

        var campoNormalizado = campo.Trim();
        if (campoNormalizado.Length > tamanhoMaximo)
        {
            throw new ArgumentException(
                $"O campo deve possuir no máximo {tamanhoMaximo} caracteres.",
                nomeParametro);
        }

        return campoNormalizado;
    }
}
