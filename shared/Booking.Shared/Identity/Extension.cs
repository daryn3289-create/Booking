using System.Security.Claims;
using System.Text.Json;
using Booking.Shared.Identity.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Booking.Shared.Identity;

public static class Extension
{
    public static void AddKeycloakAuthentication(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Services.Configure<KeycloakOptions>(configuration.GetSection(KeycloakOptions.SectionName));
        
        var keycloakOptions = configuration.GetSection(KeycloakOptions.SectionName).Get<KeycloakOptions>()
            ?? throw new InvalidOperationException("Keycloak options are not configured properly.");

        builder.Services.AddAuthentication()
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.Authority = keycloakOptions.Authority;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKeyResolver = (token, securityToken, kid, validationParameters) =>
                    {
                        using var http = new HttpClient();
                        var jwks = http.GetStringAsync("http://keycloak:8080/realms/booking/protocol/openid-connect/certs")
                            .GetAwaiter().GetResult();

                        var keys = new JsonWebKeySet(jwks);
                        
                        return keys.Keys;
                    }
                };
                
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.TryGetValue("access_token", out var token))
                        {
                            context.Token = token;
                        }
                        return Task.CompletedTask;
                    },
                    
                    OnTokenValidated = context =>
                    {
                        var identity = context.Principal?.Identity as ClaimsIdentity;
                        
                        var resourceAccessClaim = context.Principal?.FindFirst("resource_access")?.Value;
                        if (!string.IsNullOrEmpty(resourceAccessClaim))
                        {
                            var doc = JsonDocument.Parse(resourceAccessClaim);
                            
                            if (doc.RootElement.TryGetProperty("booking", out var bookingRoles) &&
                                bookingRoles.TryGetProperty("roles", out var roles))
                            {
                                foreach (var roleName in roles
                                             .EnumerateArray()
                                             .Select(role => role.GetString())
                                             .Where(roleName => !string.IsNullOrEmpty(roleName)))
                                {
                                    identity?.AddClaim(new Claim(ClaimTypes.Role, roleName));
                                }
                            }
                        }
                        return Task.CompletedTask;
                    },
                    
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine("Authentication failed: " + context.Exception.Message);
                        return Task.CompletedTask;
                    },
                    
                    OnChallenge = context =>
                    {
                        Console.WriteLine("OnChallenge: " + context.ErrorDescription);
                        return Task.CompletedTask;
                    },
                    
                    OnForbidden = context =>
                    {
                        Console.WriteLine("OnForbidden: " + context.Response);
                        return Task.CompletedTask;
                    }
                };
            });

        builder.Services.AddAuthorizationBuilder()
            .AddPolicy(AuthorizationConstants.AdminPolicy, policyBuilder =>
            {
                policyBuilder.RequireRole(AuthorizationConstants.AdminRole);
                policyBuilder.RequireClaim(AuthorizationConstants.EmailVerifiedClaim, "true");
            })
            .AddPolicy(AuthorizationConstants.ClientPolicy, policyBuilder =>
            {
                policyBuilder.RequireRole(AuthorizationConstants.ClientRole);
                policyBuilder.RequireClaim(AuthorizationConstants.EmailVerifiedClaim, "true");
            });

        builder.Services.AddAuthorization();
    }
}

public static class AuthorizationConstants
{
    public const string AdminPolicy = "AdminPolicy";
    public const string ClientPolicy = "ClientPolicy";
    
    public const string AdminRole = "Admin";
    public const string ClientRole = "Client";
    
    public const string EmailVerifiedClaim = "email_verified";
}