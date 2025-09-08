using Booking.Shared.Api;
using HotelCatalog.Application.Contexts.Handlers.Commands;
using Microsoft.AspNetCore.Mvc;

namespace HotelCatalog.Api.Controllers;

public class HotelController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> CreateHotel(
        [FromBody] CreateHotelCommand command, 
        [FromServices] CreateHotelCommandHandler handler, 
        CancellationToken cancellationToken)    
    {
        await handler.Handle(command, cancellationToken);
        return Created();
    }
}