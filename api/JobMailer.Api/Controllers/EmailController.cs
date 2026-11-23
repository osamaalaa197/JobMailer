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
    IEmailService email,
    IAiEmailEnhancer aiEnhancer) : ControllerBase
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
                request.ReceiverName,
                request.JobDescription,
                request.UseAi),
            cancellationToken);

        if (request.UseAi
            && !string.IsNullOrWhiteSpace(request.JobDescription))
        {
            try
            {
                rendered = await aiEnhancer.EnhanceAsync(
                    rendered.Subject,
                    rendered.Body,
                    request.JobDescription!,
                    request.Company,
                    request.JobTitle,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                // Keep the original rendered email when AI is unavailable.
                Console.WriteLine($"AI enhancement skipped: {ex.Message}");
            }
        }

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
