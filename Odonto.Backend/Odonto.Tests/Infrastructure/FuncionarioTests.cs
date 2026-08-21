using Odonto.Domain.Equipe;
using Xunit;

namespace Odonto.Tests.Domain;

public sealed class FuncionarioTests
{
    [Fact]
    public void Criar_NormalizaNomesEInicializaPerfilValido()
    {
        var funcionario = Funcionario.Criar(
            "  Julia Guerra de Almeida  ",
            "  Julia Almeida  ");

        Assert.NotEqual(Guid.Empty, funcionario.Id);
        Assert.Equal("Julia Guerra de Almeida", funcionario.NomeCompleto);
        Assert.Equal("Julia Almeida", funcionario.NomeExibicao);
        Assert.Null(funcionario.Telefone);
        Assert.Null(funcionario.FotoKey);
    }

    [Fact]
    public void AtualizarPerfil_NormalizaValoresEOmiteTelefoneEmBranco()
    {
        var funcionario = Funcionario.Criar("Julia Guerra", "Julia");

        funcionario.AtualizarPerfil("  Julia Almeida  ", "   ");

        Assert.Equal("Julia Almeida", funcionario.NomeExibicao);
        Assert.Null(funcionario.Telefone);
    }

    [Fact]
    public void AtualizarPerfil_RejeitaNomeDeExibicaoEmBranco()
    {
        var funcionario = Funcionario.Criar("Julia Guerra", "Julia");

        Assert.Throws<ArgumentException>(() =>
            funcionario.AtualizarPerfil(" ", null));
    }

    [Fact]
    public void Foto_PodeSerAtualizadaERemovida()
    {
        var funcionario = Funcionario.Criar("Julia Guerra", "Julia");

        funcionario.AtualizarFoto("  perfis/julia.webp  ");
        Assert.Equal("perfis/julia.webp", funcionario.FotoKey);

        funcionario.RemoverFoto();
        Assert.Null(funcionario.FotoKey);
    }
}
