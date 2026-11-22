using JobMailer.Business.Abstractions;
using JobMailer.Business.Models;

namespace JobMailer.Business.Services;

public sealed class DashboardService(IApplicationService applications) : IDashboardService
{
    public async Task<DashboardDto> GetDashboardAsync(
        int userId,
        int days = 30,
        CancellationToken cancellationToken = default)
    {
        days = Math.Clamp(days, 7, 90);

        var all = await applications.GetApplicationsAsync(userId, cancellationToken);
        var totalSent = all.Count;
        var opened = all.Count(x => x.Opened);
        var totalOpens = all.Sum(x => x.OpenCount);

        var endDate = DateTime.UtcNow.Date;
        var startDate = endDate.AddDays(-(days - 1));

        var dailyActivity = Enumerable.Range(0, days)
            .Select(offset =>
            {
                var date = startDate.AddDays(offset);
                return new DashboardDayDto(
                    date,
                    all.Count(x => x.SentAt.Date == date),
                    all.Count(x => x.FirstOpenedAt?.Date == date));
            })
            .ToList();

        var topCompanies = all
            .Where(x => !string.IsNullOrWhiteSpace(x.Company))
            .GroupBy(x => x.Company!.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(group => new CompanyStatsDto(
                group.Key,
                group.Count(),
                group.Count(x => x.Opened)))
            .OrderByDescending(x => x.Sent)
            .ThenByDescending(x => x.Opened)
            .ThenBy(x => x.Company)
            .Take(5)
            .ToList();

        var recentApplications = all
            .OrderByDescending(x => x.SentAt)
            .Take(8)
            .ToList();

        return new DashboardDto(
            totalSent,
            opened,
            totalSent - opened,
            totalOpens,
            totalSent == 0 ? 0 : Math.Round(opened * 100d / totalSent, 1),
            dailyActivity,
            topCompanies,
            recentApplications);
    }
}
