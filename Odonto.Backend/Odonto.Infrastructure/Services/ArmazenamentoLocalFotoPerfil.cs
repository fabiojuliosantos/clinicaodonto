using Microsoft.Extensions.Configuration;
using Odonto.Application;
using SkiaSharp;

namespace Odonto.Infrastructure.Services;

public sealed class ArmazenamentoLocalFotoPerfil(IConfiguration configuration)
    : IArmazenamentoFotoPerfil
{
    private const int QualidadeWebp = 82;
    private const int TamanhoBuffer = 81920;

    private static readonly HashSet<string> ContentTypesPermitidos =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

    public async Task<string> SalvarAsync(
        Stream conteudo,
        string contentType,
        long tamanho,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(conteudo);

        if (tamanho <= 0)
        {
            throw new FotoPerfilInvalidaException("O arquivo da foto está vazio.");
        }

        if (tamanho > FotoPerfilConfiguracao.TamanhoMaximoBytes)
        {
            throw CriarExcecaoArquivoMuitoGrande();
        }

        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new FotoPerfilInvalidaException(
                "O tipo do arquivo da foto não foi informado.");
        }

        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new FotoPerfilInvalidaException(
                "O tipo do arquivo da foto não foi informado.");
        }

        var contentTypeNormalizado = contentType.Split(';', 2)[0].Trim();
        if (!ContentTypesPermitidos.Contains(contentTypeNormalizado))
        {
            throw new FotoPerfilInvalidaException(
                "A foto deve estar nos formatos JPEG, PNG ou WebP.");
        }

        using var entrada = await LerComLimiteAsync(conteudo, cancellationToken);
        using var codec = SKCodec.Create(entrada, out var resultadoCodec);

        if (codec is null || resultadoCodec != SKCodecResult.Success)
        {
            throw new FotoPerfilInvalidaException(
                "Não foi possível interpretar o arquivo como uma imagem válida.");
        }

        if (codec.EncodedFormat is not (
            SKEncodedImageFormat.Jpeg or
            SKEncodedImageFormat.Png or
            SKEncodedImageFormat.Webp))
        {
            throw new FotoPerfilInvalidaException(
                "O conteúdo da foto deve estar nos formatos JPEG, PNG ou WebP.");
        }

        if (codec.FrameCount > 1)
        {
            throw new FotoPerfilInvalidaException("Fotos animadas não são permitidas.");
        }

        var info = codec.Info;
        if (info.Width <= 0 || info.Height <= 0
            || info.Width > FotoPerfilConfiguracao.DimensaoMaximaPixels
            || info.Height > FotoPerfilConfiguracao.DimensaoMaximaPixels)
        {
            throw new FotoPerfilInvalidaException(
                $"A foto deve possuir no máximo {FotoPerfilConfiguracao.DimensaoMaximaPixels} pixels em cada dimensão.");
        }

        using var imagemDecodificada = SKBitmap.Decode(codec);
        if (imagemDecodificada is null)
        {
            throw new FotoPerfilInvalidaException(
                "Não foi possível decodificar o conteúdo da foto.");
        }

        using var imagemOrientada = CorrigirOrientacao(
            imagemDecodificada,
            codec.EncodedOrigin);
        using var imagemFinal = RecortarEredimensionar(imagemOrientada);
        using var imagem = SKImage.FromBitmap(imagemFinal);
        using var dadosWebp = imagem.Encode(SKEncodedImageFormat.Webp, QualidadeWebp);

        if (dadosWebp is null)
        {
            throw new InvalidOperationException("Não foi possível gerar a foto de perfil.");
        }

        var diretorio = ObterDiretorioRaiz();
        Directory.CreateDirectory(diretorio);

        var fotoKey = $"{Guid.NewGuid():N}.webp";
        var caminhoFinal = ResolverCaminhoSeguro(diretorio, fotoKey);
        var caminhoTemporario = Path.Combine(
            diretorio,
            $".tmp-{Guid.NewGuid():N}");

        try
        {
            await using (var arquivo = new FileStream(
                             caminhoTemporario,
                             FileMode.CreateNew,
                             FileAccess.Write,
                             FileShare.None,
                             TamanhoBuffer,
                             FileOptions.Asynchronous))
            {
                await dadosWebp.AsStream().CopyToAsync(arquivo, cancellationToken);
                await arquivo.FlushAsync(cancellationToken);
            }

            File.Move(caminhoTemporario, caminhoFinal);
            return fotoKey;
        }
        finally
        {
            if (File.Exists(caminhoTemporario))
            {
                File.Delete(caminhoTemporario);
            }
        }
    }

    public Task<ArquivoFotoArmazenada?> ObterAsync(
        string fotoKey,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var caminho = ResolverCaminhoSeguro(ObterDiretorioRaiz(), fotoKey);
        if (!File.Exists(caminho))
        {
            return Task.FromResult<ArquivoFotoArmazenada?>(null);
        }

        Stream conteudo = new FileStream(
            caminho,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            TamanhoBuffer,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        return Task.FromResult<ArquivoFotoArmazenada?>(
            new ArquivoFotoArmazenada(conteudo, FotoPerfilConfiguracao.ContentTypeFinal));
    }

    public Task RemoverAsync(
        string fotoKey,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        File.Delete(ResolverCaminhoSeguro(ObterDiretorioRaiz(), fotoKey));
        return Task.CompletedTask;
    }

    private static async Task<MemoryStream> LerComLimiteAsync(
        Stream conteudo,
        CancellationToken cancellationToken)
    {
        var memoria = new MemoryStream();
        var buffer = new byte[TamanhoBuffer];

        try
        {
            while (true)
            {
                var quantidade = await conteudo.ReadAsync(buffer, cancellationToken);
                if (quantidade == 0)
                {
                    break;
                }

                if (memoria.Length + quantidade > FotoPerfilConfiguracao.TamanhoMaximoBytes)
                {
                    throw CriarExcecaoArquivoMuitoGrande();
                }

                await memoria.WriteAsync(buffer.AsMemory(0, quantidade), cancellationToken);
            }

            if (memoria.Length == 0)
            {
                throw new FotoPerfilInvalidaException("O arquivo da foto está vazio.");
            }

            memoria.Position = 0;
            return memoria;
        }
        catch
        {
            await memoria.DisposeAsync();
            throw;
        }
    }

    private static SKBitmap CorrigirOrientacao(
        SKBitmap origem,
        SKEncodedOrigin orientacao)
    {
        var trocaDimensoes = orientacao is
            SKEncodedOrigin.LeftTop or
            SKEncodedOrigin.RightTop or
            SKEncodedOrigin.RightBottom or
            SKEncodedOrigin.LeftBottom;

        var destino = new SKBitmap(
            trocaDimensoes ? origem.Height : origem.Width,
            trocaDimensoes ? origem.Width : origem.Height,
            SKColorType.Rgba8888,
            SKAlphaType.Premul);

        using var canvas = new SKCanvas(destino);
        canvas.Clear(SKColors.Transparent);

        switch (orientacao)
        {
            case SKEncodedOrigin.TopRight:
                canvas.Translate(destino.Width, 0);
                canvas.Scale(-1, 1);
                break;
            case SKEncodedOrigin.BottomRight:
                canvas.Translate(destino.Width, destino.Height);
                canvas.RotateDegrees(180);
                break;
            case SKEncodedOrigin.BottomLeft:
                canvas.Translate(0, destino.Height);
                canvas.Scale(1, -1);
                break;
            case SKEncodedOrigin.LeftTop:
                canvas.RotateDegrees(90);
                canvas.Scale(1, -1);
                break;
            case SKEncodedOrigin.RightTop:
                canvas.Translate(destino.Width, 0);
                canvas.RotateDegrees(90);
                break;
            case SKEncodedOrigin.RightBottom:
                canvas.Translate(destino.Width, destino.Height);
                canvas.RotateDegrees(90);
                canvas.Scale(-1, 1);
                break;
            case SKEncodedOrigin.LeftBottom:
                canvas.Translate(0, destino.Height);
                canvas.RotateDegrees(270);
                break;
        }

        canvas.DrawBitmap(
            origem,
            0,
            0,
            new SKSamplingOptions(SKFilterMode.Nearest, SKMipmapMode.None));
        canvas.Flush();
        return destino;
    }

    private static SKBitmap RecortarEredimensionar(SKBitmap origem)
    {
        var lado = Math.Min(origem.Width, origem.Height);
        var esquerda = (origem.Width - lado) / 2f;
        var topo = (origem.Height - lado) / 2f;
        var origemRecorte = SKRect.Create(esquerda, topo, lado, lado);
        var destino = new SKBitmap(
            FotoPerfilConfiguracao.TamanhoFinalPixels,
            FotoPerfilConfiguracao.TamanhoFinalPixels,
            SKColorType.Rgba8888,
            SKAlphaType.Premul);

        using var canvas = new SKCanvas(destino);
        canvas.Clear(SKColors.Transparent);
        using var paint = new SKPaint
        {
            IsAntialias = true
        };
        canvas.DrawBitmap(
            origem,
            origemRecorte,
            SKRect.Create(
                FotoPerfilConfiguracao.TamanhoFinalPixels,
                FotoPerfilConfiguracao.TamanhoFinalPixels),
            new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None),
            paint);
        canvas.Flush();
        return destino;
    }

    private string ObterDiretorioRaiz()
    {
        var caminhoConfigurado = configuration["Storage:ProfilePhotosPath"];
        if (string.IsNullOrWhiteSpace(caminhoConfigurado))
        {
            throw new InvalidOperationException(
                "A configuração Storage:ProfilePhotosPath é obrigatória para armazenar fotos de perfil.");
        }

        if (!Path.IsPathFullyQualified(caminhoConfigurado))
        {
            throw new InvalidOperationException(
                "A configuração Storage:ProfilePhotosPath deve conter um caminho absoluto.");
        }

        return Path.TrimEndingDirectorySeparator(Path.GetFullPath(caminhoConfigurado));
    }

    private static string ResolverCaminhoSeguro(string diretorio, string fotoKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fotoKey);

        var raiz = Path.GetFullPath(diretorio);
        var caminho = Path.GetFullPath(Path.Combine(raiz, fotoKey));
        var comparacao = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        var prefixoRaiz = raiz + Path.DirectorySeparatorChar;

        if (!caminho.StartsWith(prefixoRaiz, comparacao))
        {
            throw new ArgumentException("A chave da foto é inválida.", nameof(fotoKey));
        }

        return caminho;
    }

    private static FotoPerfilMuitoGrandeException CriarExcecaoArquivoMuitoGrande() =>
        new($"A foto deve possuir no máximo {FotoPerfilConfiguracao.TamanhoMaximoBytes / 1024 / 1024} MB.");
}
