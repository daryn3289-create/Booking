using Booking.Shared.Domain;

namespace HotelCatalog.Domain.ValueObjects;

public sealed record Address 
{
    public string Country { get; }
    public string City { get; }
    public string Street { get; }

    private Address(string street, string city, string country)
    {
        Street = street;
        City = city;
        Country = country;
    }

    public static Address Create(string street, string city, string country)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new DomainException("Street cannot be empty.");
        if (string.IsNullOrWhiteSpace(city))
            throw new DomainException("City cannot be empty.");
        if (string.IsNullOrWhiteSpace(country))
            throw new DomainException("Country cannot be empty.");

        return new Address(street, city, country);
    }
}