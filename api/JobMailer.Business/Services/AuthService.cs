using System.Security.Cryptography;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using JobMailer.Business.Abstractions;
using JobMailer.Business.Exceptions;
using JobMailer.Business.Models;
using JobMailer.Business.Options;
using JobMailer.Data.Entities;
using JobMailer.Data.Repositories;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JobMailer.Business.Services;

public sealed class AuthService(
    IUserRepository users,
    ITokenProtector protector,
    IJwtTokenService jwtTokens,
    IOptions<GoogleOptions> googleOptions,
    IHttpClientFactory httpClientFactory,
    IDataProtectionProvider dataProtection,
    ILogger<AuthService> logger) : IAuthService
{
    private readonly GoogleOptions _google = googleOptions.Value;
    private readonly ITimeLimitedDataProtector _stateProtector =
        dataProtection.CreateProtector("JobMailer.OAuthState.v1").ToTimeLimitedDataProtector();

    public string CreateGoogleAuthorizationUrl()
    {
        var query = new Dictionary<string, string>
        {
            ["client_id"] = _google.ClientId,
            ["redirect_uri"] = _google.RedirectUri,
            ["response_type"] = "code",
            ["scope"] = string.Join(" ", _google.Scopes),
            ["access_type"] = "offline",
            ["prompt"] = "consent",
            ["include_granted_scopes"] = "true",
            ["state"] = _stateProtector.Protect(Guid.NewGuid().ToString("N"), TimeSpan.FromMinutes(10))
        };

        var queryString = string.Join(
            "&",
            query.Select(pair => $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value)}"));

        return "https://accounts.google.com/o/oauth2/v2/auth?" + queryString;
    }

    public async Task<LoginResult> HandleGoogleCallbackAsync(
        string code,
        string state,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new AppException("Google authorization code is missing.");
        }

        ValidateState(state);

        TokenResponse token;
        try
        {
            token = await CreateFlow().ExchangeCodeForTokenAsync(
                userId: string.Empty,
                code: code,
                redirectUri: _google.RedirectUri,
                taskCancellationToken: cancellationToken);
        }
        catch (TokenResponseException ex)
        {
            logger.LogWarning(ex, "Google authorization code exchange failed.");
            throw new UnauthorizedAppException("Google sign-in failed. Please try again.");
        }

        if (string.IsNullOrWhiteSpace(token.IdToken))
        {
            throw new UnauthorizedAppException("Google did not return an identity token. Please sign in again.");
        }

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(token.IdToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _google.ClientId }
            });
        }
        catch (InvalidJwtException ex)
        {
            logger.LogWarning(ex, "Google identity token validation failed.");
            throw new UnauthorizedAppException("Google identity token is invalid. Please sign in again.");
        }

        var user = await users.GetByGoogleIdAsync(payload.Subject, cancellationToken);
        if (user is null)
        {
            user = new User
            {
                GoogleId = payload.Subject,
                CreatedAt = DateTime.UtcNow
            };
            await users.AddAsync(user, cancellationToken);
        }

        user.Email = payload.Email;
        user.FullName = string.IsNullOrWhiteSpace(payload.Name) ? payload.Email : payload.Name;

        if (!string.IsNullOrWhiteSpace(token.AccessToken))
        {
            user.AccessToken = protector.Protect(token.AccessToken);
        }

        // Keep the existing refresh token if Google does not return a new one.
        if (!string.IsNullOrWhiteSpace(token.RefreshToken))
        {
            user.RefreshToken = protector.Protect(token.RefreshToken);
        }

        user.TokenExpiry = DateTime.UtcNow.AddSeconds(token.ExpiresInSeconds ?? 3600);
        user.LastLogin = DateTime.UtcNow;

        await users.SaveChangesAsync(cancellationToken);

        var jwt = jwtTokens.Create(user);
        return new LoginResult(jwt.Token, jwt.ExpiresAt, new UserInfo(user.UserId, user.Email, user.FullName));
    }

    public async Task<UserInfo> GetCurrentUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await users.GetByIdAsync(userId, cancellationToken)
            ?? throw new UnauthorizedAppException("Your session is no longer valid.");

        return new UserInfo(user.UserId, user.Email, user.FullName);
    }

    public async Task LogoutAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await users.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return;
        }

        var tokenToRevoke = protector.Unprotect(user.RefreshToken)
            ?? protector.Unprotect(user.AccessToken);

        if (!string.IsNullOrWhiteSpace(tokenToRevoke))
        {
            await RevokeGoogleTokenAsync(tokenToRevoke, cancellationToken);
        }

        user.AccessToken = null;
        user.RefreshToken = null;
        user.TokenExpiry = null;

        await users.SaveChangesAsync(cancellationToken);
    }

    private GoogleAuthorizationCodeFlow CreateFlow() =>
        new(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = _google.ClientId,
                ClientSecret = _google.ClientSecret
            },
            Scopes = _google.Scopes
        });

    private void ValidateState(string state)
    {
        if (string.IsNullOrWhiteSpace(state))
        {
            throw new AppException("Google sign-in session is missing. Please try again.");
        }

        try
        {
            _stateProtector.Unprotect(state);
        }
        catch (Exception ex) when (ex is CryptographicException or FormatException)
        {
            throw new AppException("Google sign-in session has expired. Please try again.");
        }
    }

    private async Task RevokeGoogleTokenAsync(string token, CancellationToken cancellationToken)
    {
        try
        {
            using var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["token"] = token
            });
            using var response = await httpClientFactory.CreateClient("google")
                .PostAsync("https://oauth2.googleapis.com/revoke", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Google token revocation returned {StatusCode}.", (int)response.StatusCode);
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(ex, "Google token revocation request failed.");
        }
    }
}
