using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using Microsoft.Extensions.Configuration;

namespace Tests.SitecoreGraphQl.Base;
public class TestBase
{
    protected IEnumerable<AuthenticationCredentialsProvider> CredentialsProviders { get; set; }

    protected InvocationContext InvocationContext { get; set; }

    public FileManager FileManager { get; set; }

    protected TestBase()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        
        CredentialsProviders = config.GetSection("ConnectionDefinition").GetChildren()
            .Select(x => new AuthenticationCredentialsProvider(x.Key, x.Value!)).ToList();
        InvocationContext = new InvocationContext
        {
            AuthenticationCredentialsProviders = CredentialsProviders,
        };

        FileManager = new FileManager();
    }
    
    protected void PrintObject<T>(T obj)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(obj, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });
        
        Console.WriteLine(json);
    }
}
