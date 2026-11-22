using JobMailer.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobMailer.Data.Repositories;

public sealed class ApplicationRepository(AppDbContext db) : IApplicationRepository
{
    public async Task<IReadOnlyList<Application>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default) =>
        await db.Applications
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.SentAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Application application, CancellationToken cancellationToken = default) =>
        await db.Applications.AddAsync(application, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);

    public async Task<bool> RecordOpenAsync(
        string trackingToken,
        DateTime openedAt,
        CancellationToken cancellationToken = default)
    {
        var updated = await db.Applications
            .Where(x => x.TrackingToken == trackingToken)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.OpenCount, x => x.OpenCount + 1)
                    .SetProperty(x => x.FirstOpenedAt, x => x.FirstOpenedAt ?? openedAt)
                    .SetProperty(x => x.LastOpenedAt, openedAt),
                cancellationToken);

        return updated > 0;
    }
}
