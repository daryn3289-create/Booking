using Identity.Infrastructure.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Api;

public static class Extension
{
    public static void AddKeycloakAuthentication(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<KeycloakOptions>(builder.Configuration.GetSection(KeycloakOptions.SectionName));
        
        var keycloakOptions = builder.Configuration.GetSection(KeycloakOptions.SectionName).Get<KeycloakOptions>()
            ?? throw new InvalidOperationException("Keycloak options are not configured properly.");

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Authority = keycloakOptions.Authority;
                options.RequireHttpsMetadata = false;
                options.MetadataAddress = keycloakOptions.MetadataAddress;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    RoleClaimType = "roles",
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateLifetime = false
                };
                
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last();
                        if (!string.IsNullOrEmpty(accessToken))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });
        
        builder.Services.AddAuthorization();
    }
}