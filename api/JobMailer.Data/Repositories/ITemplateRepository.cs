using JobMailer.Data.Entities;

namespace JobMailer.Data.Repositories;

public interface ITemplateRepository
{
    Task<IReadOnlyList<Template>> GetAllAsync(int userId, CancellationToken cancellationToken = default);

    Task<Template?> GetByIdAsync(int templateId, int userId, CancellationToken cancellationToken = default);

    Task AddAsync(Template template, CancellationToken cancellationToken = default);

    void Remove(Template template);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
