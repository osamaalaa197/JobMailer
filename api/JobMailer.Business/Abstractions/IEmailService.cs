using JobMailer.Business.Models;

namespace JobMailer.Business.Abstractions;

public interface ITemplateService
{
    Task<IReadOnlyList<TemplateDto>> GetTemplatesAsync(CancellationToken cancellationToken = default);

    Task<RenderedEmail> RenderAsync(int userId, RenderEmailRequest request, CancellationToken cancellationToken = default);
}

public interface IEmailService
{
    Task<ApplicationDto> SendAsync(int userId, SendEmailRequest request, CancellationToken cancellationToken = default);
}

public interface IApplicationService
{
    Task<IReadOnlyList<ApplicationDto>> GetApplicationsAsync(int userId, CancellationToken cancellationToken = default);
}
