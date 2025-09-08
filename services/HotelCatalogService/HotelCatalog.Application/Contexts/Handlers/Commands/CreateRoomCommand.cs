using Booking.Shared.Application;
using FluentValidation;
using HotelCatalog.Domain;
using HotelCatalog.Infrastructure;
using Microsoft.Extensions.Logging;

namespace HotelCatalog.Application.Contexts.Handlers.Commands;

public class CreateRoomCommand : ICommand
{
    public int HotelId { get; set; }
    public RoomType RoomType { get; set; }
    public int Capacity { get; set; }
    public decimal PricePerNight { get; set; }
    public bool IsAvailable { get; set; }
}

public class CreateRoomCommandValidator : AbstractValidator<CreateRoomCommand>
{
    public CreateRoomCommandValidator()
    {
        RuleFor(x => x.HotelId)
            .GreaterThan(0).WithMessage("ID отеля должен быть больше 0");

        RuleFor(x => x.RoomType)
            .IsInEnum().WithMessage("Некорректный тип комнаты");

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Вместимость должна быть больше 0");

        RuleFor(x => x.PricePerNight)
            .GreaterThan(0).WithMessage("Цена за ночь должна быть больше 0");
    }
}
public class CreateRoomCommandHandler : ICommandHandler<CreateRoomCommand>
{
    private readonly HotelCatalogDbContext _context;
    private readonly ILogger<CreateRoomCommandHandler> _logger;

    public CreateRoomCommandHandler(HotelCatalogDbContext context, ILogger<CreateRoomCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(CreateRoomCommand command, CancellationToken cancellationToken)
    {
        var room = new Room(command.HotelId, command.RoomType, command.Capacity, command.PricePerNight, command.IsAvailable);
        
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Комната с ID {RoomId} создана для отеля с ID {HotelId}", room.Id, room.HotelId);
    }
}

