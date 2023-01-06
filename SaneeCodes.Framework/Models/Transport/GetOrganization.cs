using SaneeCodes.Framework.Models;
namespace SaneeCodes.Framework.Models.Transport;
public class GetOrganizationRS:TransportResponseBase
{
    public Organization? Organization {get; set;}    
}