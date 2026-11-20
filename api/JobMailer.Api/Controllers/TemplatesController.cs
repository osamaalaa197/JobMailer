using JobMailer.Api.Contracts;
using JobMailer.Api.Extensions;
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
        Ok(await templates.GetTemplatesAsync(User.GetUserId(), cancellationToken));

    [HttpPost]
    public async Task<ActionResult<int>> Create(TemplateRequest request, CancellationToken cancellationToken)
    {
        var id = await templates.CreateTemplateAsync(User.GetUserId(), request.ToInput(), cancellationToken);
        return Ok(id);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, TemplateRequest request, CancellationToken cancellationToken)
    {
        await templates.UpdateTemplateAsync(User.GetUserId(), id, request.ToInput(), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await templates.DeleteTemplateAsync(User.GetUserId(), id, cancellationToken);
        return NoContent();
    }
}
