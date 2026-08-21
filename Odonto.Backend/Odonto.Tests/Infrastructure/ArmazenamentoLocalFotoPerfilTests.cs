using Microsoft.Extensions.Configuration;
using Odonto.Application;
using Odonto.Infrastructure.Services;
using SkiaSharp;
using Xunit;

namespace Odonto.Tests.Infrastructure;

public sealed class ArmazenamentoLocalFotoPerfilTests
{
    [Fact]
    public async Task Salvar_ImagemPng_GeraWebpQuadradoComChaveAleatoria()
    {
        var diretorio = CriarDiretorioTemporario();

        try
        {
            var armazenamento = CriarArmazenamento(diretorio);
            await using var entrada = CriarImagemPng(800, 400);

            var fotoKey = await armazenamento.SalvarAsync(
                entrada,
                "image/png",
                entrada.Length,
                TestCancellationToken);

            Assert.EndsWith(".webp", fotoKey, StringComparison.Ordinal);
            Assert.True(Guid.TryParseExact(
                Path.GetFileNameWithoutExtension(fotoKey),
                "N",
                out _));

            var arquivo = await armazenamento.ObterAsync(
                fotoKey,
                TestCancellationToken);
            Assert.NotNull(arquivo);
            Assert.Equal(FotoPerfilConfiguracao.ContentTypeFinal, arquivo.ContentType);

            await using var conteudo = arquivo.Conteudo;
            using var codec = SKCodec.Create(conteudo);
            Assert.NotNull(codec);
            Assert.Equal(SKEncodedImageFormat.Webp, codec.EncodedFormat);
            Assert.Equal(FotoPerfilConfiguracao.TamanhoFinalPixels, codec.Info.Width);
            Assert.Equal(FotoPerfilConfiguracao.TamanhoFinalPixels, codec.Info.Height);
        }
        finally
        {
            Directory.Delete(diretorio, recursive: true);
        }
    }

    [Fact]
    public async Task Salvar_ConteudoQueNaoEImagem_RejeitaMesmoComContentTypePermitido()
    {
        var diretorio = CriarDiretorioTemporario();

        try
        {
            var armazenamento = CriarArmazenamento(diretorio);
            await using var entrada = new MemoryStream("não é uma imagem"u8.ToArray());

            await Assert.ThrowsAsync<FotoPerfilInvalidaException>(() =>
                armazenamento.SalvarAsync(
                    entrada,
                    "image/png",
                    entrada.Length,
                    TestCancellationToken));

            Assert.Empty(Directory.EnumerateFiles(diretorio));
        }
        finally
        {
            Directory.Delete(diretorio, recursive: true);
        }
    }

    [Fact]
    public async Task Salvar_TamanhoDeclaradoAcimaDoLimite_RejeitaAntesDeLer()
    {
        var diretorio = CriarDiretorioTemporario();

        try
        {
            var armazenamento = CriarArmazenamento(diretorio);
            await using var entrada = new MemoryStream([1]);

            await Assert.ThrowsAsync<FotoPerfilMuitoGrandeException>(() =>
                armazenamento.SalvarAsync(
                    entrada,
                    "image/jpeg",
                    FotoPerfilConfiguracao.TamanhoMaximoBytes + 1,
                    TestCancellationToken));
        }
        finally
        {
            Directory.Delete(diretorio, recursive: true);
        }
    }

    [Fact]
    public async Task Obter_ChaveComSaidaDoDiretorio_RejeitaCaminho()
    {
        var diretorio = CriarDiretorioTemporario();

        try
        {
            var armazenamento = CriarArmazenamento(diretorio);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                armazenamento.ObterAsync(
                    "../foto.webp",
                    TestCancellationToken));
        }
        finally
        {
            Directory.Delete(diretorio, recursive: true);
        }
    }

    [Fact]
    public async Task Remover_ArquivoExistente_DeixaFotoIndisponivel()
    {
        var diretorio = CriarDiretorioTemporario();

        try
        {
            var armazenamento = CriarArmazenamento(diretorio);
            await using var entrada = CriarImagemPng(200, 200);
            var fotoKey = await armazenamento.SalvarAsync(
                entrada,
                "image/png",
                entrada.Length,
                TestCancellationToken);

            await armazenamento.RemoverAsync(fotoKey, TestCancellationToken);

            Assert.Null(await armazenamento.ObterAsync(
                fotoKey,
                TestCancellationToken));
        }
        finally
        {
            Directory.Delete(diretorio, recursive: true);
        }
    }

    private static ArmazenamentoLocalFotoPerfil CriarArmazenamento(string diretorio)
    {
        var configuracao = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Storage:ProfilePhotosPath"] = diretorio
            })
            .Build();

        return new ArmazenamentoLocalFotoPerfil(configuracao);
    }

    private static MemoryStream CriarImagemPng(int largura, int altura)
    {
        using var bitmap = new SKBitmap(largura, altura);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.CornflowerBlue);
        using var imagem = SKImage.FromBitmap(bitmap);
        using var dados = imagem.Encode(SKEncodedImageFormat.Png, 100);
        var memoria = new MemoryStream();
        dados.SaveTo(memoria);
        memoria.Position = 0;
        return memoria;
    }

    private static string CriarDiretorioTemporario()
    {
        var diretorio = Path.Combine(
            Path.GetTempPath(),
            $"odonto-foto-perfil-{Guid.NewGuid():N}");
        Directory.CreateDirectory(diretorio);
        return diretorio;
    }

    private static CancellationToken TestCancellationToken =>
        TestContext.Current.CancellationToken;
}
