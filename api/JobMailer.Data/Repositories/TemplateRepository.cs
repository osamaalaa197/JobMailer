using JobMailer.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobMailer.Data.Repositories;

public sealed class TemplateRepository(AppDbContext db) : ITemplateRepository
{
    public async Task<IReadOnlyList<Template>> GetAllAsync(
        int userId,
        CancellationToken cancellationToken = default) =>
        await db.Templates
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Id)
            .ToListAsync(cancellationToken);

    public Task<Template?> GetByIdAsync(
        int templateId,
        int userId,
        CancellationToken cancellationToken = default) =>
        db.Templates.FirstOrDefaultAsync(
            x => x.Id == templateId && x.UserId == userId,
            cancellationToken);

    public async Task AddAsync(Template template, CancellationToken cancellationToken = default) =>
        await db.Templates.AddAsync(template, cancellationToken);

    public void Remove(Template template) => db.Templates.Remove(template);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
