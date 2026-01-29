using Apps.SitecoreGraphQl.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;

namespace Apps.SitecoreGraphQl.Connections;

public class ConnectionDefinition : IConnectionDefinition
{
    public IEnumerable<ConnectionPropertyGroup> ConnectionPropertyGroups => new List<ConnectionPropertyGroup>
    {
        new()
        {
            Name = ConnectionTypes.SitecoreXmCloud,
            DisplayName = "Sitecore XM Cloud",
            AuthenticationType = ConnectionAuthenticationType.Undefined,
            ConnectionProperties = new List<ConnectionProperty>
            {
                new(CredNames.BaseUrl)
                {
                    DisplayName = "Base URL",
                    Description = "Your XM Cloud instance base URL (e.g., https://xmc-yourorg-yourinstance.sitecorecloud.io)",
                    Sensitive = false
                },
                new(CredNames.ClientId)
                {
                    DisplayName = "Client ID",
                    Description = "OAuth2 Client ID from XM Cloud",
                    Sensitive = false
                },
                new(CredNames.ClientSecret)
                {
                    DisplayName = "Client Secret",
                    Description = "OAuth2 Client Secret from XM Cloud",
                    Sensitive = true
                }
            }
        },
        new()
        {
            Name = ConnectionTypes.SitecoreXp,
            DisplayName = "Sitecore XP",
            AuthenticationType = ConnectionAuthenticationType.Undefined,
            ConnectionProperties = new List<ConnectionProperty>
            {
                new(CredNames.BaseUrl)
                {
                    DisplayName = "Base URL",
                    Description = "Your Sitecore CM base URL (e.g., https://cm.yourdomain.com)",
                    Sensitive = false
                },
                new(CredNames.IdentityServerUrl)
                {
                    DisplayName = "Identity Server URL",
                    Description = "Sitecore Identity Server URL (e.g., https://identity.yourdomain.com)",
                    Sensitive = false
                },
                new(CredNames.ClientId)
                {
                    DisplayName = "Client ID",
                    Description = "OAuth2 Client ID configured in Identity Server",
                    Sensitive = false
                },
                new(CredNames.ClientSecret)
                {
                    DisplayName = "Client Secret",
                    Description = "OAuth2 Client Secret configured in Identity Server",
                    Sensitive = true
                }
            }
        }
    };

    public IEnumerable<AuthenticationCredentialsProvider> CreateAuthorizationCredentialsProviders(Dictionary<string, string> values)
    {
        var connectionType = values.Keys.Contains(CredNames.IdentityServerUrl) 
            ? ConnectionTypes.SitecoreXp 
            : ConnectionTypes.SitecoreXmCloud;
        
        return values
            .Select(x => new AuthenticationCredentialsProvider(x.Key, x.Value))
            .Append(new AuthenticationCredentialsProvider(CredNames.ConnectionType, connectionType))
            .ToList();
    }
}