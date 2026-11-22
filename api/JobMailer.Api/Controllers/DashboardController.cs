using JobMailer.Api.Extensions;
using JobMailer.Business.Abstractions;
using JobMailer.Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobMailer.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public sealed class DashboardController(IDashboardService dashboard) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<DashboardDto>> Get(
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default) =>
        Ok(await dashboard.GetDashboardAsync(User.GetUserId(), days, cancellationToken));
}
