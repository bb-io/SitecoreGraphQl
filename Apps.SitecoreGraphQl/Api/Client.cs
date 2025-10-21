using Apps.SitecoreGraphQl.Models.Dtos;
using Apps.SitecoreGraphQl.Utils;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Utils.RestSharp;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.SitecoreGraphQl.Api;

public class Client(List<AuthenticationCredentialsProvider> creds) : BlackBirdRestClient(new()
{
    BaseUrl = creds.GetBaseUrl(),
    ThrowOnAnyError = false
}) 
{
    public async Task<T> ExecuteGraphQlWithErrorHandling<T>(RestRequest request)
    {
        var response = await ExecuteWithErrorHandling(request);
        var graphQlResponse = JsonConvert.DeserializeObject<GraphQlResponseDto<T>>(response.Content!);
        return graphQlResponse!.Data!;
    }
    
    public override async Task<RestResponse> ExecuteWithErrorHandling(RestRequest request)
    {
        var response = await base.ExecuteWithErrorHandling(request);
        var graphQlResponse = JsonConvert.DeserializeObject<GraphQlResponseDto<object>>(response.Content!);
        if (graphQlResponse != null && graphQlResponse.Errors.Count > 0)
        {
            throw new PluginApplicationException(graphQlResponse.GetErrorMessages());
        }
        
        return response;
    }

    protected override Exception ConfigureErrorException(RestResponse response)
    {
        if (string.IsNullOrEmpty(response.Content))
        {
            if (string.IsNullOrEmpty(response.ErrorMessage))
            {
                return new PluginApplicationException($"Request failed with status code {response.StatusCode}");
            }
            
            return new PluginApplicationException(response.ErrorMessage);
        }

        var error = JsonConvert.DeserializeObject<RestErrorDto>(response.Content!);
        return error == null 
            ? new PluginApplicationException($"Request failed with status code {response.StatusCode}, content: {response.Content}") 
            : new PluginApplicationException($"{error.Error}: {error.ErrorDescription}");
    }
}