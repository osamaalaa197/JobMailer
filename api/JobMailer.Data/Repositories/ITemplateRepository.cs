using JobMailer.Data.Entities;

namespace JobMailer.Data.Repositories;

public interface ITemplateRepository
{
    Task<IReadOnlyList<Template>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Template?> GetByIdAsync(int templateId, CancellationToken cancellationToken = default);
}
