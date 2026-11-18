using JobMailer.Business.Models;

namespace JobMailer.Business.Abstractions;

public interface IProfileService
{
    Task<IReadOnlyList<ProfileDto>> GetProfilesAsync(int userId, CancellationToken cancellationToken = default);

    Task<int> CreateProfileAsync(int userId, ProfileInput input, CancellationToken cancellationToken = default);

    Task UpdateProfileAsync(int userId, int profileId, ProfileInput input, CancellationToken cancellationToken = default);

    Task DeleteProfileAsync(int userId, int profileId, CancellationToken cancellationToken = default);
}
