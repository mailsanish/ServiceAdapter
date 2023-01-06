using SaneeCodes.Framework.Models;

namespace SaneeCodes.Framework.Models.Transport;

    public class GetOrganizationEmployeesRQ:TransportRequestBase
    {
        public int? OrgId {get; set;}
    }
    
    public class GetOrganizationEmployeesRS:TransportResponseBase
    {
        public Organization? Organization {get; set;}
    }
