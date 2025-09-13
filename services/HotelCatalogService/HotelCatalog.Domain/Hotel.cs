using Booking.Shared.Domain;
using HotelCatalog.Domain.ValueObjects;

namespace HotelCatalog.Domain;

public class Hotel : BaseEntity<int>
{
    public Hotel()
    {
    }
    public int OwnerId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Address Address { get; set; }
    public int Rating { get; set; } 
    public HotelStatus Status { get; set; }
    
    private readonly List<HotelPhoto> _photos = [];
    public IReadOnlyCollection<HotelPhoto> Photos => _photos;

    private readonly List<Room> _rooms = [];
    public IReadOnlyCollection<Room> Rooms => _rooms;

    
    public void AddPhoto(HotelPhoto photo) => _photos.Add(photo);

    public void AddRoom(Room room) => _rooms.Add(room);
    public Hotel(int ownerId, string name, string description, Address address, int rating, HotelStatus status)
    {
        OwnerId = ownerId;
        Name = name;
        Description = description;
        Address = address;
        Rating = rating;
        Status = status;
    }

    public static Hotel Create(int ownerId, string name, string description, Address address, int rating, HotelStatus status) 
        => new(ownerId, name, description, address, rating, status);
}

public class HotelDetailsReadModel : BaseEntity<int>
{
    public HotelDetailsReadModel()
    {
    }
    
    public int HotelId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Address { get; set; }
    public int Rating { get; set; }
    public HotelStatus Status { get; set; }

    public List<string> Photos { get; set; } = new();
    public List<RoomReadModel> Rooms { get; set; } = new();
}

public class RoomReadModel : BaseEntity<int>
{
    public int RoomId { get; set; }
    public string Name { get; set; }
    public int Capacity { get; set; }
    public decimal PricePerNight { get; set; }
}

public record HotelCreatedEvent(int HotelId, int OwnerId, string Name, string Description, Address Address, int Rating, HotelStatus Status);
public record RoomCreatedEvent(int RoomId, int HotelId, RoomType RoomType, int Capacity, decimal PricePerNight, bool IsAvailable);
public record HotelPhotoAddedEvent(int HotelId, string PhotoUrl);