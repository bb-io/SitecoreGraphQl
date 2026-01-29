using Apps.SitecoreGraphQl.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;

namespace Apps.SitecoreGraphQl.Utils;

public static class AuthenticationCredentialsProviderExtensions
{
    public static Uri GetBaseUrl(this IEnumerable<AuthenticationCredentialsProvider> providers)
    {
        var baseUrl = providers.Get(CredNames.BaseUrl);
        if(string.IsNullOrEmpty(baseUrl.Value))
        {
            throw new Exception("Base URL is not provided in the connection settings.");
        }
        
        return new Uri(baseUrl.Value);
    }
    
    public static string GetConnectionType(this IEnumerable<AuthenticationCredentialsProvider> providers)
    {
        var connectionType = providers.Get(CredNames.ConnectionType);
        if(string.IsNullOrEmpty(connectionType.Value))
        {
            throw new Exception("Connection type is not provided in the connection settings.");
        }
        
        return connectionType.Value;
    }
    
    public static Uri GetIdentityServerUrl(this IEnumerable<AuthenticationCredentialsProvider> providers)
    {
        var identityServerUrl = providers.Get(CredNames.IdentityServerUrl);
        if(string.IsNullOrEmpty(identityServerUrl.Value))
        {
            throw new Exception("Identity Server URL is not provided in the connection settings.");
        }
        
        return new Uri(identityServerUrl.Value);
    }
    
    public static string? GetScope(this IEnumerable<AuthenticationCredentialsProvider> providers)
    {
        var scope = providers.FirstOrDefault(p => p.KeyName == CredNames.Scope);
        return scope?.Value;
    }
}