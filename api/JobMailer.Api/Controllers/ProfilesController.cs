using JobMailer.Api.Contracts;
using JobMailer.Api.Extensions;
using JobMailer.Business.Abstractions;
using JobMailer.Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobMailer.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/profiles")]
public sealed class ProfilesController(IProfileService profiles) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProfileDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await profiles.GetProfilesAsync(User.GetUserId(), cancellationToken));

    [HttpPost]
    public async Task<ActionResult<int>> Create(ProfileRequest request, CancellationToken cancellationToken)
    {
        var id = await profiles.CreateProfileAsync(User.GetUserId(), request.ToInput(), cancellationToken);
        return Ok(id);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ProfileRequest request, CancellationToken cancellationToken)
    {
        await profiles.UpdateProfileAsync(User.GetUserId(), id, request.ToInput(), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await profiles.DeleteProfileAsync(User.GetUserId(), id, cancellationToken);
        return NoContent();
    }
}
