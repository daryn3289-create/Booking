using Booking.Shared.Api;
using HotelCatalog.Application.Contexts.Handlers.Commands;
using HotelCatalog.Application.Contexts.Handlers.Queries;
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
    
    [HttpGet]
    public async Task<IActionResult> GetHotels(
        [FromQuery] GetHotelsQuery query, 
        [FromServices] GetHotelsQueryHandler handler, 
        CancellationToken cancellationToken)    
    {
        var hotels = await handler.Handle(query, cancellationToken);
        return Ok(hotels);
    }
}