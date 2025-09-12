using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Booking.Shared.Common;
using Booking.Shared.Identity;
using Booking.Shared.Identity.Generated;
using Booking.Shared.Identity.Options;
using Booking.Shared.Infrastructure.Messaging;
using Booking.Shared.Infrastructure.Messaging.Contracts;
using Duende.IdentityModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Identity.Client;

public interface IKeycloakAuthClient
{
   Task<TokenResponse> AuthorizeAsync(string username, string password, CancellationToken cancellationToken);
   Task CreateUserAsync(string username, string email, string password, string firstName, string lastName, CancellationToken cancellationToken);

   Task<IEnumerable<object>> GetUsersAsync(KeycloakUserFilter filter,
      CancellationToken cancellationToken);
}

public class KeycloakAuthClient : IKeycloakAuthClient
{
   private readonly IKeycloakGeneratedExternalApiClient _generatedExternalApiClient;
   private readonly HttpClient _httpClient;
   private readonly KeycloakOptions _keycloakOptions;
   private readonly ILogger<KeycloakAuthClient> _logger;
   private readonly IRabbitMqFactory _factory;
   
   public KeycloakAuthClient(
      HttpClient httpClient, 
      IOptions<KeycloakOptions> keycloakOptions, 
      ILogger<KeycloakAuthClient> logger, 
      IKeycloakGeneratedExternalApiClient generatedExternalApiClient, 
      IRabbitMqFactory factory)
   {
      _httpClient = httpClient;
      _keycloakOptions = keycloakOptions.Value;
      _logger = logger;
      _generatedExternalApiClient = generatedExternalApiClient;
      _factory = factory;
   }
   
   public async Task<TokenResponse> AuthorizeAsync(string username, string password, CancellationToken cancellationToken)
   {
      var request = new HttpRequestMessage
         (HttpMethod.Post, 
         $"{_keycloakOptions.Authority}/protocol/openid-connect/token");
      
      request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
      {
         { "client_id", _keycloakOptions.ClientId },
         { "client_secret", _keycloakOptions.ClientSecret },
         { "grant_type", "password" },
         { "scope", "openid" },
         { "username", username },
         { "password", password }
      });

      try
      {
         var response = await _httpClient.SendAsync(request, cancellationToken);

         if (!response.IsSuccessStatusCode)
         {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Failed to authorize by email. Status code: {StatusCode}, Error: {ErrorContent}", 
               response.StatusCode, errorContent);
            
            throw new HttpRequestException($"Authorization failed with status code {response.StatusCode}: {errorContent}");
         }
         
         return await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken) 
                ?? throw new InvalidOperationException("Failed to deserialize token response.");
      }
      catch (Exception ex)
      {
         _logger.LogError("Error authorizing by email {Message}, {InnerException}", ex.Message, ex.InnerException);
         throw;
      }
   }

   public async Task CreateUserAsync(
      string username, 
      string email,
      string password,
      string firstName,
      string lastName, 
      CancellationToken cancellationToken)
   {
      try
      {
         var user = new UserRepresentation
         {
            Username = username,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Enabled = true,
            EmailVerified = false,
            Credentials =
            [
               new CredentialRepresentation(
                  type: OidcConstants.GrantTypes.Password,
                  value: password,
                  temporary: false)
            ]
         };
      
         var url = $"{_keycloakOptions.BaseAddress}/admin/realms/{_keycloakOptions.Realm}/users";
         var response = await _httpClient.PostAsJsonAsync(url, user, cancellationToken: cancellationToken);

         var location = response.Headers.Location?.ToString();
         
         var userId = location?.Split('/').Last();
         
         if (!response.IsSuccessStatusCode || string.IsNullOrEmpty(userId))
         {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Failed to register user. Status code: {StatusCode}, Error: {ErrorContent}", 
               response.StatusCode, errorContent);
            
            await DeleteUserAsync(userId, cancellationToken);
            throw new HttpRequestException($"User registration failed with status code {response.StatusCode}: {errorContent}");
         }
         
         await SendVerifyEmailAsync(userId, cancellationToken);

         await SetClientRoleAsync(userId, cancellationToken);
         
         await PublishUserCreatedEventAsync(userId, username, email, firstName, lastName, cancellationToken);
      }
      catch (Exception ex) 
      {
         _logger.LogError(ex, "Error while register user");
         throw;
      }
   }
   
   public async Task VerifyEmailAsync(string userId, CancellationToken cancellationToken)
   {
      var user = await _generatedExternalApiClient.UsersGET2Async(null, _keycloakOptions.Realm, userId, cancellationToken: cancellationToken);

      if(user is null)
         throw new InvalidOperationException($"User with ID '{userId}' not found.");

      user.EmailVerified = true;

      await _generatedExternalApiClient.UsersPUTAsync(user, _keycloakOptions.Realm, userId, cancellationToken);
   }
   
   private async Task SendVerifyEmailAsync(string? userId, CancellationToken cancellationToken)
   {
      if (string.IsNullOrEmpty(userId))
         return;
      
      var user = await _generatedExternalApiClient.UsersGET2Async(null, _keycloakOptions.Realm, userId, cancellationToken: cancellationToken);

      if(user is null)
         throw new InvalidOperationException($"User with ID '{userId}' not found.");
      
      await _generatedExternalApiClient.SendVerifyEmailAsync(
         _keycloakOptions.ClientId,
         3, 
         null,
         _keycloakOptions.Realm,
         userId, 
         cancellationToken);
   }
   
   public async Task ChangePasswordAsync(string userId, string currentPassword, string newPassword, CancellationToken cancellationToken)
   {
      var tokenResponse = await AuthorizeAsync(userId, currentPassword, cancellationToken);
      
      if(tokenResponse is null || string.IsNullOrEmpty(tokenResponse.AccessToken))
         throw new UnauthorizedAccessException("Current password is incorrect.");

      await ResetPasswordAsync(userId, newPassword, cancellationToken);
   }

   public async Task<IEnumerable<object>> GetUsersAsync(KeycloakUserFilter filter, CancellationToken cancellationToken)
   {
      var first = (filter.Page - 1) * filter.PageSize;
      var max = filter.PageSize;

      var users = await _generatedExternalApiClient.UsersAll3Async(
         briefRepresentation: null,
         email: filter.Email,
         emailVerified: filter.EmailVerified,
         enabled: filter.Enabled,
         exact: null,
         first: first,
         firstName: filter.FirstName,
         idpAlias: null,
         idpUserId: null,
         lastName: filter.LastName,
         max: max,
         q: null,
         search: filter.Search,
         username: null,
         realm: _keycloakOptions.Realm,
         cancellationToken: cancellationToken
      );

      var result = users.Select(x => new
      {
         x.Username,
         x.Email,
         x.FirstName,
         x.LastName,
         x.EmailVerified,
         x.Enabled,
         x.Attributes,
         x.Self
      });
      
      return result;
   }
   
   public async Task<Booking.Shared.Identity.Generated.UserRepresentation?> GetUserByIdAsync(string userId, CancellationToken cancellationToken)
   {
      return await _generatedExternalApiClient.UsersGET2Async(null, _keycloakOptions.Realm, userId, cancellationToken: cancellationToken);
   }
   
   public async Task<RoleRepresentation> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken)
   {
      var role = await _generatedExternalApiClient.RolesGET2Async(_keycloakOptions.Realm, roleName, cancellationToken);
      
      if(role is null)
         throw new InvalidOperationException($"Role with name '{roleName}' not found.");
      
      return role;
   }
   
   public async Task<ICollection<RoleRepresentation>> GetRolesAsync(CancellationToken cancellationToken)
   {
      return await _generatedExternalApiClient.RolesAllAsync(null, null, 10, null, _keycloakOptions.Realm, _keycloakOptions.ClientUuid, cancellationToken);
   }

   private async Task DeleteUserAsync(string? userId, CancellationToken cancellationToken)
   {
      if (string.IsNullOrEmpty(userId))
         return;
      
      var user = await _generatedExternalApiClient.UsersGET2Async(null, _keycloakOptions.Realm, userId, cancellationToken: cancellationToken);

      if(user is null)
         throw new InvalidOperationException($"User with ID '{userId}' not found.");

      await _generatedExternalApiClient.UsersDELETE3Async(_keycloakOptions.Realm, userId, cancellationToken);
   }
   private async Task ResetPasswordAsync(string userId, string newPassword, CancellationToken cancellationToken)
   {
      var user = await _generatedExternalApiClient.UsersGET2Async(null, _keycloakOptions.Realm, userId, cancellationToken: cancellationToken);

      if(user is null)
         throw new InvalidOperationException($"User with ID '{userId}' not found.");

      var credentialRepresentation = new Booking.Shared.Identity.Generated.CredentialRepresentation
      {
         Type = "password",
         Value = newPassword,
         Temporary = false
      };

      await _generatedExternalApiClient.ResetPasswordAsync(
         credentialRepresentation, 
         _keycloakOptions.Realm, 
         userId, 
         cancellationToken);
   }
   
   private async Task SetClientRoleAsync(string? userId, CancellationToken cancellationToken)
   {
      if(string.IsNullOrEmpty(userId))
         return;
      
      var user = await _generatedExternalApiClient.UsersGET2Async(null, _keycloakOptions.Realm, userId, cancellationToken: cancellationToken);

      if(user is null)
         return;
      
      var userIdParam = user.Id;

      var userRoles = await _generatedExternalApiClient.ClientsAll9Async(_keycloakOptions.Realm, userIdParam,
            _keycloakOptions.ClientUuid, cancellationToken);

      if(userRoles.Count != 0)
         return;
      
      var availableRole = await _generatedExternalApiClient.Available9Async(_keycloakOptions.Realm, userIdParam, _keycloakOptions.ClientUuid, cancellationToken);

      var defaultRole = availableRole.FirstOrDefault(x => x.Name == "Client");
      
      if(defaultRole is null)
         return;

      await _generatedExternalApiClient.ClientsPOST6Async([defaultRole], _keycloakOptions.Realm, userIdParam,
         _keycloakOptions.ClientUuid, cancellationToken);
   }
   
   private async Task PublishUserCreatedEventAsync(
      string? userId,
      string username,
      string email,
      string firstName,
      string lastName,
      CancellationToken cancellationToken)
   {
      if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var parsedUserId))
      {
         _logger.LogError("Invalid userId format: {UserId}", userId);
         throw new FormatException($"UserId '{userId}' is not a valid GUID or is null/empty.");
      }

      var userCreatedEvent = new UserCreatedEvent
      {
         ClientId = parsedUserId,
         Username = username,
         Email = email,
         FirstName = firstName,
         LastName = lastName
      };

      var message = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(userCreatedEvent));

      await using var channel = await _factory.CreateChannelAsync(cancellationToken);

      await channel.QueueDeclareAsync(
         queue: "users",
         durable: true,
         exclusive: false,
         autoDelete: false,
         cancellationToken: cancellationToken);

      await channel.BasicPublishAsync(
         exchange: "",
         routingKey: "users",
         mandatory: false,
         basicProperties: new BasicProperties { Persistent = true },
         body: message,
         cancellationToken: cancellationToken);
   }
}


public record UserRepresentation
{
   [JsonPropertyName("username")]
   public string Username { get; set; }
   [JsonPropertyName("email")]
   public string Email { get; set; }
   [JsonPropertyName("firstName")]
   public string FirstName { get; set; }
   [JsonPropertyName("lastName")]
   public string LastName { get; set; }
   [JsonPropertyName("enabled")]
   public bool Enabled { get; set; }
   [JsonPropertyName("emailVerified")]
   public bool EmailVerified { get; set; }
   [JsonPropertyName("credentials")]
   public List<CredentialRepresentation> Credentials { get; set; }
}

public record CredentialRepresentation
{
   public CredentialRepresentation(string type, string value, bool temporary)
   {
      Type = type;
      Value = value;
      Temporary = temporary;
   }

   [JsonPropertyName("type")]
   public string Type { get; set; }
   [JsonPropertyName("value")]
   public string Value { get; set; }
   [JsonPropertyName("temporary")]
   public bool Temporary { get; set; }
}