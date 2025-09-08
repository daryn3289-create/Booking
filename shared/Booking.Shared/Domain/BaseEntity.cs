using System.ComponentModel.DataAnnotations;

namespace Booking.Shared.Domain;

public class BaseEntity<TId> where TId : struct
{
    [Key]
    public TId Id { get; set; }
    public DateTime Created { get; set; } = DateTime.UtcNow;
}