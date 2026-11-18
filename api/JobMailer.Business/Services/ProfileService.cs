using System.Net.Mail;
using JobMailer.Business.Abstractions;
using JobMailer.Business.Exceptions;
using JobMailer.Business.Models;
using JobMailer.Data.Entities;
using JobMailer.Data.Repositories;

namespace JobMailer.Business.Services;

public sealed class ProfileService(IProfileRepository profiles) : IProfileService
{
    private const int MaxCvBytes = 5 * 1024 * 1024;
    private static readonly string[] AllowedCvExtensions = { ".pdf", ".doc", ".docx" };

    public async Task<IReadOnlyList<ProfileDto>> GetProfilesAsync(int userId, CancellationToken cancellationToken = default)
    {
        var list = await profiles.GetByUserIdAsync(userId, cancellationToken);
        return list.Select(ToDto).ToList();
    }

    public async Task<int> CreateProfileAsync(int userId, ProfileInput input, CancellationToken cancellationToken = default)
    {
        Validate(input);

        var profile = new Profile
        {
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        Apply(profile, input);

        await profiles.AddAsync(profile, cancellationToken);
        await profiles.SaveChangesAsync(cancellationToken);
        return profile.Id;
    }

    public async Task UpdateProfileAsync(int userId, int profileId, ProfileInput input, CancellationToken cancellationToken = default)
    {
        Validate(input);

        var profile = await profiles.GetByIdAsync(profileId, userId, cancellationToken)
            ?? throw new NotFoundException("Profile not found.");

        Apply(profile, input);
        profile.UpdatedAt = DateTime.UtcNow;

        await profiles.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteProfileAsync(int userId, int profileId, CancellationToken cancellationToken = default)
    {
        var profile = await profiles.GetByIdAsync(profileId, userId, cancellationToken)
            ?? throw new NotFoundException("Profile not found.");

        profiles.Remove(profile);
        await profiles.SaveChangesAsync(cancellationToken);
    }

    private static void Apply(Profile profile, ProfileInput input)
    {
        profile.Title = input.Title.Trim();
        profile.FullName = input.FullName.Trim();
        profile.Phone = input.Phone.Trim();
        profile.Email = input.Email.Trim();

        if (!string.IsNullOrWhiteSpace(input.CvBase64))
        {
            profile.CvBase64 = input.CvBase64;
            profile.CvFileName = string.IsNullOrWhiteSpace(input.CvFileName) ? "CV.pdf" : input.CvFileName;
        }
    }

    private static void Validate(ProfileInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Title))
        {
            throw new AppException("Profile title is required.");
        }

        if (string.IsNullOrWhiteSpace(input.FullName))
        {
            throw new AppException("Full name is required.");
        }

        if (string.IsNullOrWhiteSpace(input.Phone))
        {
            throw new AppException("Phone is required.");
        }

        if (string.IsNullOrWhiteSpace(input.Email) || !MailAddress.TryCreate(input.Email, out _))
        {
            throw new AppException("A valid contact email is required.");
        }

        if (string.IsNullOrWhiteSpace(input.CvBase64))
        {
            return;
        }

        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(input.CvBase64);
        }
        catch (FormatException)
        {
            throw new AppException("The CV file could not be read.");
        }

        if (bytes.Length > MaxCvBytes)
        {
            throw new AppException("The CV must be 5 MB or smaller.");
        }

        var extension = Path.GetExtension(input.CvFileName ?? string.Empty).ToLowerInvariant();
        if (!AllowedCvExtensions.Contains(extension))
        {
            throw new AppException("The CV must be a PDF, DOC or DOCX file.");
        }
    }

    private static ProfileDto ToDto(Profile profile) =>
        new(
            profile.Id,
            profile.Title,
            profile.FullName,
            profile.Phone,
            profile.Email,
            profile.CvFileName,
            !string.IsNullOrEmpty(profile.CvBase64));
}
