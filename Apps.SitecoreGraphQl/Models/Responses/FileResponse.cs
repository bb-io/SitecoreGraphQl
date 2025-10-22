using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.SitecoreGraphQl.Models.Responses;

public class FileResponse : IDownloadContentOutput
{
    public FileReference Content { get; set; } = default!;
}