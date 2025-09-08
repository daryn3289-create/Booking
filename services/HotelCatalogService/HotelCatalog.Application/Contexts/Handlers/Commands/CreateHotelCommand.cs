using System.Text.Json;
using Booking.Shared.Application;
using Booking.Shared.Infrastructure.Messaging;
using FluentValidation;
using HotelCatalog.Domain;
using HotelCatalog.Domain.ValueObjects;
using HotelCatalog.Infrastructure;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace HotelCatalog.Application.Contexts.Handlers.Commands;

public class CreateHotelCommand : ICommand
{
    public int OwnerId { get; set; } // Assuming hotels are owned by users
    public string Name { get; set; }
    public string Description { get; set; }
    public string Country { get; set; }
    public string City { get; set; }
    public string Street { get; set; }

    public int Rating { get; set; }
    public HotelStatus Status { get; set; }
}

public class CreateHotelCommandValidator : AbstractValidator<CreateHotelCommand>
{
    public CreateHotelCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название обязательно")
            .MaximumLength(100).WithMessage("Название не должно превышать 100 символов");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Описание не должно превышать 500 символов");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Страна обязательна")
            .MaximumLength(50).WithMessage("Страна не должна превышать 50 символов");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("Город обязателен")
            .MaximumLength(50).WithMessage("Город не должен превышать 50 символов");

        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Улица обязательна")
            .MaximumLength(100).WithMessage("Улица не должна превышать 100 символов");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Рейтинг должен быть от 1 до 5");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Некорректный статус отеля");
    }
}

public class CreateHotelCommandHandler : ICommandHandler<CreateHotelCommand>
{
    private readonly IRabbitMqFactory _rabbitMqFactory;
    private readonly HotelCatalogDbContext _context;
    private readonly ILogger<CreateHotelCommandHandler> _logger;

    public CreateHotelCommandHandler(HotelCatalogDbContext context, ILogger<CreateHotelCommandHandler> logger, IRabbitMqFactory rabbitMqFactory)
    {
        _context = context;
        _logger = logger;
        _rabbitMqFactory = rabbitMqFactory;
    }

    public async Task Handle(CreateHotelCommand command, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var address = Address.Create(command.Street, command.City, command.Country);
            var hotel = Hotel.Create(command.OwnerId, command.Name, command.Description, address, command.Rating, command.Status);

            _context.Hotels.Add(hotel);
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            
            var hotelCreatedEvent = new HotelCreatedEvent(
                hotel.Id, 
                hotel.OwnerId, 
                hotel.Name, 
                hotel.Description, 
                hotel.Address, 
                hotel.Rating, 
                hotel.Status);
            
            await using var channel = await _rabbitMqFactory.CreateChannelAsync(cancellationToken);
        
            var body = JsonSerializer.SerializeToUtf8Bytes(hotelCreatedEvent);

            await channel.BasicPublishAsync(
                exchange: "", 
                routingKey: "hotel.created",
                mandatory: false,
                basicProperties: new BasicProperties { Persistent = true },
                body: body,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating hotel");
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

}


public class RoomResponse
{
    public RoomType RoomType { get; set; }
    public int Capacity { get; set; } 
    public decimal PricePerNight { get; set; } 
    public bool IsAvailable { get; set; }
}