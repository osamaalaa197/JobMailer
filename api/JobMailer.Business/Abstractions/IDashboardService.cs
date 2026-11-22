using JobMailer.Business.Models;

namespace JobMailer.Business.Abstractions;

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync(
        int userId,
        int days = 30,
        CancellationToken cancellationToken = default);
}
