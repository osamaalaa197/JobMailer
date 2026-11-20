using JobMailer.Api.Extensions;
using JobMailer.Business.Abstractions;
using JobMailer.Business.Exceptions;
using JobMailer.Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobMailer.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    IAuthService auth,
    IConfiguration configuration,
    IWebHostEnvironment environment,
    ILogger<AuthController> logger) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("google/start")]
    public IActionResult StartGoogleLogin()
    {
        var redirectUri = ResolveGoogleRedirectUri();

        logger.LogInformation(
            "Starting Google login. Scheme={Scheme}, Host={Host}, ForwardedProto={ForwardedProto}, RedirectUri={RedirectUri}",
            Request.Scheme,
            Request.Host.Value,
            Request.Headers["X-Forwarded-Proto"].ToString(),
            redirectUri);

        return Redirect(auth.CreateGoogleAuthorizationUrl(redirectUri));
    }

    [AllowAnonymous]
    [HttpGet("google/callback")]
    public async Task<IActionResult> GoogleCallback(
        [FromQuery] string? code,
        [FromQuery] string? state,
        [FromQuery] string? error,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Google callback received. Scheme={Scheme}, Host={Host}, ForwardedProto={ForwardedProto}, HasCode={HasCode}, HasState={HasState}, HasError={HasError}, RedirectUri={RedirectUri}",
            Request.Scheme,
            Request.Host.Value,
            Request.Headers["X-Forwarded-Proto"].ToString(),
            !string.IsNullOrWhiteSpace(code),
            !string.IsNullOrWhiteSpace(state),
            !string.IsNullOrWhiteSpace(error),
            ResolveGoogleRedirectUri());

        if (!string.IsNullOrWhiteSpace(error))
        {
            logger.LogWarning("Google returned an OAuth error: {Error}.", error);

            var message = error == "access_denied"
                ? "Google access was denied. JobMailer needs the gmail.send permission to send applications."
                : "Google sign-in failed. Please try again.";

            return RedirectToLogin(message);
        }

        try
        {
            var result = await auth.HandleGoogleCallbackAsync(
                code ?? string.Empty,
                state ?? string.Empty,
                ResolveGoogleRedirectUri(),
                cancellationToken);

            return Redirect($"{ResolveWebUrl()}/#access_token={Uri.EscapeDataString(result.AccessToken)}");
        }
        catch (AppException ex)
        {
            logger.LogWarning(ex, "Google callback rejected: {Reason}", ex.Message);
            return RedirectToLogin(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Google callback failed.");
            return RedirectToLogin("Google sign-in failed. Please try again.");
        }
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserInfo>> Me(CancellationToken cancellationToken) =>
        Ok(await auth.GetCurrentUserAsync(User.GetUserId(), cancellationToken));

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await auth.LogoutAsync(User.GetUserId(), cancellationToken);
        return NoContent();
    }

    private IActionResult RedirectToLogin(string message) =>
        Redirect($"{ResolveWebUrl()}/#error={Uri.EscapeDataString(message)}");

    /// <summary>
    /// Uses the configured URI when it is not a localhost placeholder, otherwise derives it from the
    /// current request so a same-origin deployment works without extra configuration.
    /// </summary>
    private string ResolveGoogleRedirectUri()
    {
        var configured = configuration["Google:RedirectUri"];

        if (!string.IsNullOrWhiteSpace(configured)
            && (!IsLocalhost(configured) || environment.IsDevelopment()))
        {
            return configured;
        }

        return $"{RequestOrigin()}/api/auth/google/callback";
    }

    /// <summary>
    /// Uses the configured web URL in development or when it points at a real deployment, otherwise
    /// redirects back to the current site origin.
    /// </summary>
    private string ResolveWebUrl()
    {
        var configured = configuration["Web:Url"];

        if (!string.IsNullOrWhiteSpace(configured) && !IsLocalhost(configured))
        {
            return configured.TrimEnd('/');
        }

        if (environment.IsDevelopment() && !string.IsNullOrWhiteSpace(configured))
        {
            return configured.TrimEnd('/');
        }

        return RequestOrigin();
    }

    private string RequestOrigin() =>
        $"{Request.Scheme}://{Request.Host}{Request.PathBase}".TrimEnd('/');

    private static bool IsLocalhost(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && (uri.IsLoopback || uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase));
}
