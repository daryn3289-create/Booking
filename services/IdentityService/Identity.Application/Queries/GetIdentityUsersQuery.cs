using Booking.Shared.Application;
using Booking.Shared.Common;
using Identity.Client;
using UserRepresentation = Booking.Shared.Identity.Generated.UserRepresentation;

namespace Identity.Application.Queries;

public record GetIdentityUsersQuery : IQuery<IEnumerable<object>>
{
    public KeycloakUserFilter Filter { get; set; } = new();
}

public class GetIdentityUsersQueryHandler : IQueryHandler<GetIdentityUsersQuery, IEnumerable<object>>
{
    private readonly IKeycloakAuthClient _keycloakAuthClient;

    public GetIdentityUsersQueryHandler(IKeycloakAuthClient keycloakAuthClient)
    {
        _keycloakAuthClient = keycloakAuthClient;
    }

    public async Task<IEnumerable<object>> Handle(GetIdentityUsersQuery query, CancellationToken cancellationToken)
    {
        var users = await _keycloakAuthClient.GetUsersAsync(query.Filter, cancellationToken);
        return users;
    }
}