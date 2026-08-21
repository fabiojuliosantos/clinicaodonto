using Odonto.Application.DTO;

namespace Odonto.Application.Interfaces;

public interface IEmailService
{
    Task<bool> EnviarEmailAsync(
        EmailDTO email,
        CancellationToken cancellationToken = default);
}
