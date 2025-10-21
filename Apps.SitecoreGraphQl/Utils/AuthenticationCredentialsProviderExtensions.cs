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
}