using Booking.Shared.Domain;

namespace HotelCatalog.Domain.ValueObjects;

public sealed class Address : IComparable<Address>, IEquatable<Address>
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

    public int CompareTo(object? obj)
    {
        if (obj is null) return 1;
        if (ReferenceEquals(this, obj)) return 0;
        return obj is Address other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(Address)}");
    }

    public bool Equals(Address? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Country == other.Country && City == other.City && Street == other.Street;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Address)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Country, City, Street);
    }

    public static bool operator ==(Address? left, Address? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Address? left, Address? right)
    {
        return !Equals(left, right);
    }

    public int CompareTo(Address? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        var countryComparison = string.Compare(Country, other.Country, StringComparison.Ordinal);
        if (countryComparison != 0) return countryComparison;
        var cityComparison = string.Compare(City, other.City, StringComparison.Ordinal);
        if (cityComparison != 0) return cityComparison;
        return string.Compare(Street, other.Street, StringComparison.Ordinal);
    }
}