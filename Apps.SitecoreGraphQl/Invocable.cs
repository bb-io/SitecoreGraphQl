using Apps.SitecoreGraphQl.Api;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.SitecoreGraphQl;

public class Invocable : BaseInvocable
{
    protected AuthenticationCredentialsProvider[] CredentialsProviders =>
        InvocationContext.AuthenticationCredentialsProviders.ToArray();

    protected Client Client { get; }
    
    public Invocable(InvocationContext invocationContext) : base(invocationContext)
    {
        Client = new(CredentialsProviders.ToList());
    }
}
