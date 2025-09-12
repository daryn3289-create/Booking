using Booking.Shared.Api;
using Booking.Shared.Identity;
using Identity.Application.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers;

public class UsersController : BaseController
{
    [HttpGet]
    [Authorize(Policy = AuthorizationConstants.ClientPolicy)]
    public async Task<IActionResult> Get(
        [FromQuery] GetIdentityUsersQuery query,
        [FromServices] GetIdentityUsersQueryHandler handler,
        CancellationToken cancellationToken)
    {
        var users = await handler.Handle(query, cancellationToken);
        return Ok(users);
    }
}