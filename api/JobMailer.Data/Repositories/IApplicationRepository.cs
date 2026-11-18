using JobMailer.Data.Entities;

namespace JobMailer.Data.Repositories;

public interface IApplicationRepository
{
    Task<IReadOnlyList<Application>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task AddAsync(Application application, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
