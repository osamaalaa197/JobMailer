using JobMailer.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobMailer.Data.Repositories;

public sealed class ProfileRepository(AppDbContext db) : IProfileRepository
{
    public async Task<IReadOnlyList<Profile>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default) =>
        await db.Profiles
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Title)
            .ToListAsync(cancellationToken);

    public Task<Profile?> GetByIdAsync(int profileId, int userId, CancellationToken cancellationToken = default) =>
        db.Profiles.FirstOrDefaultAsync(x => x.Id == profileId && x.UserId == userId, cancellationToken);

    public async Task AddAsync(Profile profile, CancellationToken cancellationToken = default) =>
        await db.Profiles.AddAsync(profile, cancellationToken);

    public void Remove(Profile profile) => db.Profiles.Remove(profile);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
