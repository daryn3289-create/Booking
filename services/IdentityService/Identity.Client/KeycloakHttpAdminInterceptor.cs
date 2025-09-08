using System.Net.Http.Headers;
using System.Net.Http.Json;
using Booking.Shared.Identity;
using Identity.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Identity.Client;

public class KeycloakHttpAdminInterceptor : DelegatingHandler
{
    private readonly IKeycloakAdminTokenService  _keycloakAdminTokenService;

    public KeycloakHttpAdminInterceptor(IKeycloakAdminTokenService keycloakAdminTokenService)
    {
        _keycloakAdminTokenService = keycloakAdminTokenService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _keycloakAdminTokenService.GetAdminTokenAsync();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, cancellationToken);
    }
}

public interface IKeycloakAdminTokenService
{
    Task<string> GetAdminTokenAsync();
}

public class KeycloakAdminTokenService : IKeycloakAdminTokenService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly KeycloakOptions _options;
    private readonly ILogger<KeycloakAdminTokenService> _logger;

    public KeycloakAdminTokenService(
        IHttpClientFactory httpClientFactory, 
        IOptions<KeycloakOptions> options,
        ILogger<KeycloakAdminTokenService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<string> GetAdminTokenAsync()
    {
        var httpClient = _httpClientFactory.CreateClient();
        var tokenEndpoint = $"{_options.BaseAddress}/realms/master/protocol/openid-connect/token";

        var content = new FormUrlEncodedContent(new Dictionary<string, string>()
        {
            { "client_id", _options.AdminClientId },
            { "client_secret", _options.AdminClientSecret },
            { "grant_type", "client_credentials" }
        });

        try
        {
            var response = await httpClient.PostAsync(tokenEndpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get admin token, Error: {Message}, {StatusCode}",
                    response.ReasonPhrase, response.StatusCode);
                
                throw new HttpRequestException($"Failed to get admin token, Error: {response.ReasonPhrase}");
            }
        
            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>() 
                                ?? throw new Exception("Cannot deserialize token response");

            return tokenResponse.AccessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch admin token");
            throw;
        }
    }
}