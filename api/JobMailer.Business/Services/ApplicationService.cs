using JobMailer.Business.Abstractions;
using JobMailer.Business.Models;
using JobMailer.Data.Entities;
using JobMailer.Data.Repositories;

namespace JobMailer.Business.Services;

public sealed class ApplicationService(IApplicationRepository applications) : IApplicationService
{
    public async Task<IReadOnlyList<ApplicationDto>> GetApplicationsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var list = await applications.GetByUserIdAsync(userId, cancellationToken);
        return list.Select(ToDto).ToList();
    }

    private static ApplicationDto ToDto(Application application) =>
        new(
            application.Id,
            application.Company,
            application.JobTitle,
            application.ProfileTitle,
            application.ToEmail,
            application.Subject,
            DateTime.SpecifyKind(application.SentAt, DateTimeKind.Utc),
            application.OpenCount > 0,
            application.OpenCount,
            AsUtc(application.FirstOpenedAt),
            AsUtc(application.LastOpenedAt));

    private static DateTime? AsUtc(DateTime? value) =>
        value is null ? null : DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
}
