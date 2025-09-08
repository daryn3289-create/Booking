using Booking.Shared.Domain;

namespace Profile.Domain;

public class User : BaseEntity<int>
{
    public User(Guid clientId, string username, string email, string firstName, string lastName)
    {
        ClientId = clientId;
        Username = username;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
    }
    public User()
    {
    }
    public Guid ClientId { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }

    private readonly List<int> _ownedHotels = [];
    public IReadOnlyCollection<int> OwnedHotels => _ownedHotels;
    
    public void AddOwnedHotel(int hotelId)
    {
        if (!_ownedHotels.Contains(hotelId))
            _ownedHotels.Add(hotelId);
    }
}