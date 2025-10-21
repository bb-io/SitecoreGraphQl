using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.SitecoreGraphQl.Models.Requests;

public class ItemRequest
{
    [Display("Item ID")]
    public string ItemId { get; set; } = string.Empty;

    public string GetItemId()
    {
        if(string.IsNullOrEmpty(ItemId))
        {
            throw new PluginMisconfigurationException("Item ID is null or empty, please provide a valid Item ID.");
        }
        
        return ItemId;
    }
}