using Apps.SitecoreGraphQl.Constants;
using Apps.SitecoreGraphQl.Models.Dtos;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;
using Blackbird.Applications.Sdk.Utils.RestSharp;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.SitecoreGraphQl.Api;

public class Request(IEnumerable<AuthenticationCredentialsProvider> creds)
    : BlackBirdRestRequest("/sitecore/api/authoring/graphql/v1", Method.Post, creds)
{
    protected override void AddAuth(IEnumerable<AuthenticationCredentialsProvider> creds)
    {
        var authenticationCredentialsProviders = creds as AuthenticationCredentialsProvider[] ?? creds.ToArray();
        
        var authRequest = new RestRequest("/oauth/token", Method.Post)
            .AddJsonBody(new
            {
                audience = "https://api.sitecorecloud.io",
                grant_type = "client_credentials",
                client_id = authenticationCredentialsProviders.Get(CredNames.ClientId).Value,
                client_secret = authenticationCredentialsProviders.Get(CredNames.ClientSecret).Value
            });
        
        var client = new RestClient("https://auth.sitecorecloud.io");
        var response = client.Execute(authRequest);
        if (!response.IsSuccessful)
        {
            throw new Exception($"Error obtaining access token: {response.StatusCode} - {response.Content}");
        }
        
        var tokenResponse = JsonConvert.DeserializeObject<AuthTokenDto>(response.Content!)
            ?? throw new Exception("Failed to deserialize authentication token response.");
        
        this.AddHeader("Authorization", $"Bearer {tokenResponse.AccessToken}");
    }
}