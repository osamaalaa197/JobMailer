using JobMailer.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobMailer.Data.Repositories;

public sealed class UserRepository(AppDbContext db) : IUserRepository
{
    public Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default) =>
        db.Users.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

    public Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken cancellationToken = default) =>
        db.Users.FirstOrDefaultAsync(x => x.GoogleId == googleId, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default) =>
        await db.Users.AddAsync(user, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
