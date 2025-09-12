using Booking.Shared.Api;
using Identity.Application.Handlers.Authorize;
using Identity.Application.Handlers.Register;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers;

public class AuthorizationController(IHttpContextAccessor httpContextAccessor) : BaseController
{
    [HttpPost("authorize")]
    public async Task<IActionResult> Authorize(
        [FromBody] AuthorizeCommand command, 
        [FromServices] AuthorizeCommandHandler handler,
        CancellationToken cancellationToken)
    {
        var tokenResponse = await handler.Handle(command, cancellationToken);
        
        httpContextAccessor.HttpContext!.Response.Cookies.Append("access_token", tokenResponse.AccessToken);
        
        return Ok(tokenResponse);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterCommand command,
        [FromServices] RegisterCommandHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(command, cancellationToken);
        return Created();
    }
}