using Apps.SitecoreGraphQl.Api;
using Apps.SitecoreGraphQl.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;
using RestSharp;

namespace Apps.SitecoreGraphQl.Connections;

public class ConnectionValidator: IConnectionValidator
{
    public async ValueTask<ConnectionValidationResponse> ValidateConnection(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        CancellationToken cancellationToken)
    {
        return new()
        {
            // Temporary solution to validate connection
            IsValid = true
        };
    }
}
