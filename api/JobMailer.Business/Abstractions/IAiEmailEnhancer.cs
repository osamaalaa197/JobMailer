using JobMailer.Business.Models;

namespace JobMailer.Business.Abstractions;

public interface IAiEmailEnhancer
{
    Task<RenderedEmail> EnhanceAsync(
        string subject,
        string body,
        string jobDescription,
        string? company,
        string? jobTitle,
        CancellationToken cancellationToken = default);
}
