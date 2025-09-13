using Booking.Shared.Domain;

namespace HotelCatalog.Domain;

public class Room : BaseEntity<int>
{
    public Room()
    {
    }
    public int HotelId { get; set; }
    public RoomType RoomType { get; set; }
    public int Capacity { get; set; }
    public decimal PricePerNight { get; set; }
    public bool IsAvailable { get; set; }

    public Room(int hotelId, RoomType type, int capacity, decimal pricePerNight, bool isAvailable)
    {
        HotelId = hotelId;
        RoomType = type;
        Capacity = capacity;
        PricePerNight = pricePerNight;
        IsAvailable = isAvailable;
    }
}