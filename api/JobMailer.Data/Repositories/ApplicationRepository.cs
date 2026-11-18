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
}
