using JobMailer.Data.Entities;

namespace JobMailer.Data.Repositories;

public interface IProfileRepository
{
    Task<IReadOnlyList<Profile>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<Profile?> GetByIdAsync(int profileId, int userId, CancellationToken cancellationToken = default);

    Task AddAsync(Profile profile, CancellationToken cancellationToken = default);

    void Remove(Profile profile);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
