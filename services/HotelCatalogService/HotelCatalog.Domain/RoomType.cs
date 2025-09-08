using System.ComponentModel;

namespace HotelCatalog.Domain;

public enum RoomType
{
    [Description("Single Room for one person")]
    Single,
    [Description("Double Room for couples or friends")]
    Double,
    [Description("Suite Room for luxury stay")]
    Suite,
    [Description("Family Room for larger groups")]
    Family
}