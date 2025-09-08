using Booking.Shared.Domain;

namespace HotelCatalog.Domain;

public class HotelPhoto : BaseEntity<int>
{
    public int HotelId { get; set; }
    public string Url { get; set; }

    public HotelPhoto(int  hotelId, string url)
    {
        HotelId = hotelId;
        Url = url;
    }
}