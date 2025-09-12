namespace Booking.Shared.Common;

public record Filter
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public record KeycloakUserFilter : Filter
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    
    public string? Search { get; set; }
    
    public bool? EmailVerified { get; set; }
    public bool? Enabled { get; set; }
}
public record MultiSelectFilter : Filter
{
    public Dictionary<string, string> SearchTerms { get; set; } = new();
}