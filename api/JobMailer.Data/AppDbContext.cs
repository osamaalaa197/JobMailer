using JobMailer.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobMailer.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Profile> Profiles => Set<Profile>();

    public DbSet<Template> Templates => Set<Template>();

    public DbSet<Application> Applications => Set<Application>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.UserId);
            entity.Property(x => x.GoogleId).HasMaxLength(255).IsRequired();
            entity.HasIndex(x => x.GoogleId).IsUnique();
            entity.Property(x => x.Email).HasMaxLength(255).IsRequired();
            entity.Property(x => x.FullName).HasMaxLength(255).IsRequired();
            entity.Property(x => x.AccessToken).HasColumnType("nvarchar(max)");
            entity.Property(x => x.RefreshToken).HasColumnType("nvarchar(max)");
        });

        modelBuilder.Entity<Profile>(entity =>
        {
            entity.ToTable("Profiles");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(120).IsRequired();
            entity.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Phone).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(255).IsRequired();
            entity.Property(x => x.CvFileName).HasMaxLength(255);
            entity.Property(x => x.CvBase64).HasColumnType("nvarchar(max)");
            entity.HasIndex(x => x.UserId);
            entity.HasOne(x => x.User)
                .WithMany(x => x.Profiles)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Template>(entity =>
        {
            entity.ToTable("Templates");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Key).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Subject).HasMaxLength(400).IsRequired();
            entity.Property(x => x.Body).HasColumnType("nvarchar(max)").IsRequired();

            entity.HasData(
                new Template
                {
                    Id = 1,
                    Key = "Full Stack",
                    Subject = "Application for {{JobTitle}} - {{FullName}}",
                    Body = "Dear Hiring Team,\n\nI am excited to apply for the {{JobTitle}} position at {{CompanyName}}. " +
                           "I specialize in full-stack web applications with React and .NET and would welcome the chance to contribute to your team.\n\n" +
                           "My CV is attached. You can reach me at {{Phone}} or {{Email}}.\n\nBest regards,\n{{FullName}}"
                },
                new Template
                {
                    Id = 2,
                    Key = "GIS",
                    Subject = "Application for {{JobTitle}} - {{FullName}}",
                    Body = "Dear Hiring Team,\n\nI am excited to apply for the {{JobTitle}} position at {{CompanyName}}. " +
                           "I specialize in GIS solutions, spatial data and web mapping and would welcome the chance to contribute to your team.\n\n" +
                           "My CV is attached. You can reach me at {{Phone}} or {{Email}}.\n\nBest regards,\n{{FullName}}"
                });
        });

        modelBuilder.Entity<Application>(entity =>
        {
            entity.ToTable("Applications");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ProfileTitle).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Company).HasMaxLength(255);
            entity.Property(x => x.JobTitle).HasMaxLength(255);
            entity.Property(x => x.ToEmail).HasMaxLength(255).IsRequired();
            entity.Property(x => x.Subject).HasMaxLength(400).IsRequired();
            entity.HasIndex(x => new { x.UserId, x.SentAt });
            entity.HasOne(x => x.User)
                .WithMany(x => x.Applications)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
