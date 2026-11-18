using JobMailer.Business.Abstractions;
using JobMailer.Business.Models;
using JobMailer.Data.Repositories;

namespace JobMailer.Business.Services;

public sealed class ApplicationService(IApplicationRepository applications) : IApplicationService
{
    public async Task<IReadOnlyList<ApplicationDto>> GetApplicationsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var list = await applications.GetByUserIdAsync(userId, cancellationToken);

        return list.Select(x => new ApplicationDto(
                x.Id,
                x.Company,
                x.JobTitle,
                x.ProfileTitle,
                x.ToEmail,
                x.Subject,
                DateTime.SpecifyKind(x.SentAt, DateTimeKind.Utc)))
            .ToList();
    }
}
