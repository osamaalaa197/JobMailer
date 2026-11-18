using JobMailer.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobMailer.Data.Repositories;

public sealed class TemplateRepository(AppDbContext db) : ITemplateRepository
{
    public async Task<IReadOnlyList<Template>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await db.Templates
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .ToListAsync(cancellationToken);

    public Task<Template?> GetByIdAsync(int templateId, CancellationToken cancellationToken = default) =>
        db.Templates.FirstOrDefaultAsync(x => x.Id == templateId, cancellationToken);
}
