namespace OpenPlanningPoker.UserManagement.Api.Extensions;

public static class KeyCloakExtensions
{
    private const string AdminSection = "KeycloakAdmin";
    private const string AdminClient = "admin";
    private const string ProtectionClient = "protection";

    public static IServiceCollection AddKeyCloak(this IServiceCollection services, IConfigurationManager configuration)
    {
        services.AddKeycloakWebApiAuthentication(configuration);

        services
            .AddAuthorization()
            .AddKeycloakAuthorization(configuration)
            .AddAuthorizationServer(configuration);

        var options = configuration.GetKeycloakOptions<KeycloakAdminClientOptions>(configSectionName: AdminSection)!;
        services
            .AddClientCredentialsTokenManagement()
            .AddClient(
                AdminSection,
                client =>
                {
                    client.ClientId = options.Resource;
                    client.ClientSecret = options.Credentials.Secret;
                    client.TokenEndpoint = options.KeycloakTokenEndpoint;
                }
            );

        services
            .AddKeycloakAdminHttpClient(configuration, keycloakClientSectionName: AdminSection)
            .AddClientCredentialsTokenHandler(AdminSection);

        services.AddAccessTokenManagement(
            configuration
        );

        return services;
    }

    public static IServiceCollection AddAccessTokenManagement(this IServiceCollection services, IConfigurationManager configuration)
    {
        var adminClientOptions = configuration.GetKeycloakOptions<KeycloakAdminClientOptions>(AdminSection)!;
        var protectionClientOptions = configuration.GetKeycloakOptions<KeycloakProtectionClientOptions>()!;

        services.AddSingleton(adminClientOptions);
        services.AddSingleton(protectionClientOptions);

        services.AddDistributedMemoryCache();
        services
            .AddClientCredentialsTokenManagement()
            .AddClient(
                AdminClient,
                client =>
                {
                    client.ClientId = adminClientOptions.Resource;
                    client.ClientSecret = adminClientOptions.Credentials.Secret;
                    client.TokenEndpoint = adminClientOptions.KeycloakTokenEndpoint;
                }
            )
            .AddClient(
                ProtectionClient,
                client =>
                {
                    client.ClientId = protectionClientOptions.Resource;
                    client.ClientSecret = protectionClientOptions.Credentials.Secret;
                    client.TokenEndpoint = protectionClientOptions.KeycloakTokenEndpoint;
                }
            );

        return services;
    }
}