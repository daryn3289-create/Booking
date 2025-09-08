using Booking.Shared.Application;
using Booking.Shared.Common;
using HotelCatalog.Domain;
using HotelCatalog.Domain.ValueObjects;
using HotelCatalog.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace HotelCatalog.Application.Contexts.Handlers.Queries;

public class GetHotelsQuery : IQuery<IReadOnlyCollection<HotelResponse>>
{
    public Filter Filter { get; set; } = new();
}

public class GetHotelsQueryHandler : IQueryHandler<GetHotelsQuery, IReadOnlyCollection<HotelResponse>>
{
    private readonly HotelCatalogDbContext _dbContext;

    public GetHotelsQueryHandler(HotelCatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<HotelResponse>> Handle(GetHotelsQuery query,
        CancellationToken cancellationToken)
    {
        var hotels = await _dbContext.Hotels
            .Skip((query.Filter.Page - 1) * query.Filter.PageSize)
            .Take(query.Filter.PageSize)
            .Select(h => new HotelResponse
            {
                Id = h.Id,
                Name = h.Name,
                Description = h.Description,
                Address = h.Address,
                Rating = h.Rating,
                Status = h.Status,
                Photos = h.Photos.ToList(),
                Rooms = h.Rooms.ToList()
            })
            .ToListAsync(cancellationToken);

        return hotels;
    }
}

public record HotelResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Address Address { get; set; }
    public int Rating { get; set; }
    public HotelStatus Status { get; set; }
    public List<HotelPhoto> Photos { get; set; } = [];
    public List<Room> Rooms { get; set; } = [];
}