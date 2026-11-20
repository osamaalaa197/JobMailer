using JobMailer.Api.Contracts;
using JobMailer.Api.Extensions;
using JobMailer.Business.Abstractions;
using JobMailer.Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobMailer.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/email")]
public sealed class EmailController(
    ITemplateService templates,
    IEmailService email) : ControllerBase
{
    [HttpPost("render")]
    public async Task<ActionResult<RenderedEmail>> Render(
        RenderRequest request,
        CancellationToken cancellationToken)
    {
        var rendered = await templates.RenderAsync(
            User.GetUserId(),
            new RenderEmailRequest(
                request.ProfileId,
                request.TemplateId,
                request.Company,
                request.JobTitle,
                request.ReceiverName),
            cancellationToken);

        return Ok(rendered);
    }

    [HttpPost("send")]
    public async Task<ActionResult<ApplicationDto>> Send(
        SendRequest request,
        CancellationToken cancellationToken)
    {
        var sent = await email.SendAsync(
            User.GetUserId(),
            new SendEmailRequest(
                request.ProfileId,
                request.To,
                request.Subject,
                request.Body,
                request.Company,
                request.JobTitle),
            cancellationToken);

        return Ok(sent);
    }
}
