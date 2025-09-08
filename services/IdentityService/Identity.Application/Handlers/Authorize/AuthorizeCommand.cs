using Booking.Shared.Application;
using Booking.Shared.Identity;
using Identity.Client;

namespace Identity.Application.Handlers.Authorize;

public class AuthorizeCommand : ICommand<TokenResponse>
{
    public string Username { get; set; }
    public string Password { get; set; } 
}

public class AuthorizeCommandHandler : ICommandHandler<AuthorizeCommand, TokenResponse>
{
    private readonly IKeycloakAuthClient _keycloakAuthClient;

    public AuthorizeCommandHandler(IKeycloakAuthClient keycloakAuthClient)
    {
        _keycloakAuthClient = keycloakAuthClient;
    }

    public async Task<TokenResponse> Handle(AuthorizeCommand command, CancellationToken cancellationToken)
    {
        return await _keycloakAuthClient.AuthorizeAsync(command.Username, command.Password, cancellationToken);
    }
}