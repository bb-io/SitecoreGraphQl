using Apps.SitecoreGraphQl.Constants;
using Apps.SitecoreGraphQl.Models.Dtos;
using Apps.SitecoreGraphQl.Utils;
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
        var connectionType = authenticationCredentialsProviders.GetConnectionType();
        
        string accessToken = connectionType switch
        {
            ConnectionTypes.SitecoreXmCloud => GetXmCloudToken(authenticationCredentialsProviders),
            ConnectionTypes.SitecoreXp => GetXpToken(authenticationCredentialsProviders),
            _ => throw new Exception($"Unsupported connection type: {connectionType}")
        };
        
        this.AddHeader("Authorization", $"Bearer {accessToken}");
    }
    
    private string GetXmCloudToken(AuthenticationCredentialsProvider[] creds)
    {
        var authRequest = new RestRequest("/oauth/token", Method.Post)
            .AddJsonBody(new
            {
                audience = "https://api.sitecorecloud.io",
                grant_type = "client_credentials",
                client_id = creds.Get(CredNames.ClientId).Value,
                client_secret = creds.Get(CredNames.ClientSecret).Value
            });
        
        var client = new RestClient("https://auth.sitecorecloud.io");
        var response = client.Execute(authRequest);
        
        if (!response.IsSuccessful)
        {
            var errorMessage = string.IsNullOrEmpty(response.Content)
                ? response.ErrorMessage ?? "Unknown error"
                : response.Content;
            throw new Exception($"Error obtaining XM Cloud access token: {response.StatusCode} - {errorMessage}");
        }
        
        var tokenResponse = JsonConvert.DeserializeObject<AuthTokenDto>(response.Content!)
            ?? throw new Exception("Failed to deserialize XM Cloud authentication token response.");
        
        return tokenResponse.AccessToken;
    }
    
    private string GetXpToken(AuthenticationCredentialsProvider[] creds)
    {
        var identityServerUrl = creds.GetIdentityServerUrl();
        var clientId = creds.Get(CredNames.ClientId).Value;
        var clientSecret = creds.Get(CredNames.ClientSecret).Value;
        var scope = creds.GetScope();
        
        var authRequest = new RestRequest("/connect/token", Method.Post);
        authRequest.AddHeader("Content-Type", "application/x-www-form-urlencoded");
        authRequest.AddParameter("grant_type", "client_credentials");
        authRequest.AddParameter("client_id", clientId);
        authRequest.AddParameter("client_secret", clientSecret);
        
        if (!string.IsNullOrEmpty(scope))
        {
            authRequest.AddParameter("scope", scope);
        }
        
        var client = new RestClient(identityServerUrl);
        var response = client.Execute(authRequest);
        
        if (!response.IsSuccessful)
        {
            var errorMessage = string.IsNullOrEmpty(response.Content)
                ? response.ErrorMessage ?? "Unknown error"
                : response.Content;
            throw new Exception($"Error obtaining Sitecore XP access token from Identity Server: {response.StatusCode} - {errorMessage}");
        }
        
        var tokenResponse = JsonConvert.DeserializeObject<AuthTokenDto>(response.Content!)
            ?? throw new Exception("Failed to deserialize Sitecore XP authentication token response.");
        
        return tokenResponse.AccessToken;
    }
}