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
    ILogger<AuthController> logger) : ControllerBase
{
    private string WebUrl => (configuration["Web:Url"] ?? "http://localhost:5173").TrimEnd('/');

    [AllowAnonymous]
    [HttpGet("google/start")]
    public IActionResult StartGoogleLogin() => Redirect(auth.CreateGoogleAuthorizationUrl());

    [AllowAnonymous]
    [HttpGet("google/callback")]
    public async Task<IActionResult> GoogleCallback(
        [FromQuery] string? code,
        [FromQuery] string? state,
        [FromQuery] string? error,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(error))
        {
            var message = error == "access_denied"
                ? "Google access was denied. JobMailer needs the gmail.send permission to send applications."
                : "Google sign-in failed. Please try again.";

            return RedirectToLogin(message);
        }

        try
        {
            var result = await auth.HandleGoogleCallbackAsync(code ?? string.Empty, state ?? string.Empty, cancellationToken);
            return Redirect($"{WebUrl}/#access_token={Uri.EscapeDataString(result.AccessToken)}");
        }
        catch (AppException ex)
        {
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
        Redirect($"{WebUrl}/#error={Uri.EscapeDataString(message)}");
}
