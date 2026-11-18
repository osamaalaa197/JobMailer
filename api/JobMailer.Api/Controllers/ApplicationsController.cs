using JobMailer.Api.Extensions;
using JobMailer.Business.Abstractions;
using JobMailer.Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobMailer.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/applications")]
public sealed class ApplicationsController(IApplicationService applications) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ApplicationDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await applications.GetApplicationsAsync(User.GetUserId(), cancellationToken));
}
