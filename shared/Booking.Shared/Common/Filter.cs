namespace Booking.Shared.Common;

public record Filter
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public record MultiSelectFilter : Filter
{
    public Dictionary<string, string> SearchTerms { get; set; } = new();
}