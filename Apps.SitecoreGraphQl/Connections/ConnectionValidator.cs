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
        try
        {
            var credentialsProviders = authenticationCredentialsProviders as List<AuthenticationCredentialsProvider> ?? authenticationCredentialsProviders.ToList();
            
            var client = new Client(credentialsProviders.ToList());
            var request = new Request(credentialsProviders)
                .AddJsonBody(new
                {
                    query = GraphQlQueries.GetLanguagesQuery(),
                });
            
            await client.ExecuteWithErrorHandling(request);
            return new()
            {
                IsValid = true
            };
        } 
        catch(Exception ex)
        {
            return new()
            {
                IsValid = false,
                Message = ex.Message
            };
        }
    }
}
