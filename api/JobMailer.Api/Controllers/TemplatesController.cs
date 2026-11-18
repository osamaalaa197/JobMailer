using JobMailer.Business.Abstractions;
using JobMailer.Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobMailer.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/templates")]
public sealed class TemplatesController(ITemplateService templates) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TemplateDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await templates.GetTemplatesAsync(cancellationToken));
}
