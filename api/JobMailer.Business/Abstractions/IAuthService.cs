using JobMailer.Business.Models;
using JobMailer.Data.Entities;

namespace JobMailer.Business.Abstractions;

public interface IAuthService
{
    string CreateGoogleAuthorizationUrl();

    Task<LoginResult> HandleGoogleCallbackAsync(string code, string state, CancellationToken cancellationToken = default);

    Task<UserInfo> GetCurrentUserAsync(int userId, CancellationToken cancellationToken = default);

    Task LogoutAsync(int userId, CancellationToken cancellationToken = default);
}

public interface IJwtTokenService
{
    JwtToken Create(User user);
}

public interface ITokenProtector
{
    string Protect(string plainText);

    string? Unprotect(string? protectedText);
}
