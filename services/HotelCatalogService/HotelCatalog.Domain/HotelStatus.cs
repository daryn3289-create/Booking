using System.ComponentModel;

namespace HotelCatalog.Domain;

public enum HotelStatus
{
    [Description("Open for bookings")]
    Open,
    [Description("Temporarily closed")]
    Closed,
    [Description("Under maintenance")]
    UnderMaintenance
}