using Booking.Shared.Application;
using Identity.Client;

namespace Identity.Application.Handlers.Register;


public record RegisterCommand : ICommand
{
    public string Username { get; set; }
    public string FistName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}

public class RegisterCommandHandler : ICommandHandler<RegisterCommand>
{
    private readonly IKeycloakAuthClient _keycloakAuthClient;

    public RegisterCommandHandler(IKeycloakAuthClient keycloakAuthClient)
    {
        _keycloakAuthClient = keycloakAuthClient;
    }

    public async Task Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        await _keycloakAuthClient.CreateUserAsync(command.Username, command.Email, command.Password, command.FistName, command.LastName, cancellationToken);
    }
}