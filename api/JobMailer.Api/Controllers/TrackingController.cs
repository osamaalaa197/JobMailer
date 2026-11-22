using JobMailer.Business.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobMailer.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/tracking")]
public sealed class TrackingController(ITrackingService tracking) : ControllerBase
{
    private static readonly byte[] TransparentPixel = Convert.FromBase64String(
        "R0lGODlhAQABAIAAAAAAAP///ywAAAAAAQABAAACAUwAOw==");

    [HttpGet("open/{token}")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Open(string token, CancellationToken cancellationToken)
    {
        await tracking.RecordOpenAsync(token, cancellationToken);

        Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
        Response.Headers["Pragma"] = "no-cache";
        Response.Headers["Expires"] = "0";

        return File(TransparentPixel, "image/gif");
    }
}
