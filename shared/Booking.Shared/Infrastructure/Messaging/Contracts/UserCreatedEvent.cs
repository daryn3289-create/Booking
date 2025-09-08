﻿namespace Booking.Shared.Infrastructure.Messaging.Contracts;

public class UserCreatedEvent
{
    public Guid ClientId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}