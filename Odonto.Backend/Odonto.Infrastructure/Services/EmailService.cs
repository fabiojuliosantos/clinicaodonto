using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Odonto.Application.DTO;
using Odonto.Application.Interfaces;

namespace Odonto.Infrastructure.Services;

public sealed class EmailService(IConfiguration configuration) : IEmailService
{
    public async Task<bool> EnviarEmailAsync(
        EmailDTO dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var host = GetRequiredConfiguration("EmailSmtpCliente");
        var usuario = GetRequiredConfiguration("EmailSmtpUsuario");
        var senha = GetRequiredConfiguration("EmailSmtpSenha");
        var portaConfigurada = GetRequiredConfiguration("EmailSmtpPorta");

        if (!int.TryParse(portaConfigurada, out var porta) || porta is <= 0 or > 65535)
        {
            throw new InvalidOperationException("A porta SMTP configurada é inválida.");
        }

        using var mensagem = new MailMessage
        {
            From = new MailAddress(usuario, "Almeida Estética e Sorriso"),
            Body = dto.Conteudo,
            Subject = dto.Assunto,
            IsBodyHtml = true
        };

        foreach (var destinatario in dto.Destinatarios)
        {
            mensagem.To.Add(destinatario);
        }

        using var smtpClient = new SmtpClient(host, porta)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(usuario, senha)
        };

        await smtpClient.SendMailAsync(mensagem, cancellationToken);
        return true;
    }

    private string GetRequiredConfiguration(string key)
    {
        return configuration[key]
            ?? throw new InvalidOperationException($"A configuração '{key}' não foi definida.");
    }
}
