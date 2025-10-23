using Newtonsoft.Json;

namespace Apps.SitecoreGraphQl.Models.Dtos;

public class AuthTokenDto
{
    [JsonProperty("access_token")]
    public string AccessToken { get; set; } = string.Empty;
    
    [JsonProperty("expires_in")]
    public int ExpiresIn { get; set; }
    
    [JsonProperty("token_type")]
    public string TokenType { get; set; } = string.Empty;
    
    [JsonProperty("scope")]
    public string Scope { get; set; } = string.Empty;
}