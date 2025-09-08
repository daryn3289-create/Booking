namespace Identity.Infrastructure.Options;

public class KeycloakOptions
{
    public const string SectionName = "Keycloak";
    public string BaseAddress { get; set; } = string.Empty;
    public string Authority { get; set; } = string.Empty;
    public string Realm { get; set; } = string.Empty;
    public string MetadataAddress { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientUuid { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string AdminClientId { get; set; } = string.Empty;
    public string AdminClientSecret { get; set; } = string.Empty;
}