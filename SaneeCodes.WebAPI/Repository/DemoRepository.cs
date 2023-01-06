using SaneeCodes.Framework.Models;
using SaneeCodes.Framework.Models.Transport;

namespace SaneeCodes.WebAPI.Respository;

//Replace with original DB repository
public class DemoRepository : IRepository
{
    public GetOrganizationRS GetOrganization(int orgId)
    {
        GetOrganizationRS GetOrganizationRS= new GetOrganizationRS();
        GetOrganizationRS.Organization= new Organization { OrgId=orgId, OrgName="Org "+orgId.ToString(), IsActive=true, OrgDescription="Desc of "+orgId.ToString()  } ;
        return GetOrganizationRS;
    }

    public GetOrganizationEmployeesRS GetOrganizationEmployees(GetOrganizationEmployeesRQ getOrganizationEmployees)
    {
        GetOrganizationEmployeesRS GetOrganizationEmployeesRS= new GetOrganizationEmployeesRS();
        GetOrganizationEmployeesRS.Organization = new Organization { OrgId=getOrganizationEmployees?.OrgId, OrgName="Org "+getOrganizationEmployees?.OrgId?.ToString(), IsActive=true, OrgDescription="Desc of "+getOrganizationEmployees?.OrgId?.ToString() } ;
        
        GetOrganizationEmployeesRS.Organization.Employees =new List<Person>();
        GetOrganizationEmployeesRS.Organization.Employees.Add(new Person{PersonId=1, FirstName="Sanish", LastName="A", IsActive=true});
        GetOrganizationEmployeesRS.Organization.Employees.Add(new Person{PersonId=2, FirstName="Tom", LastName="Hanks", IsActive=true});
        GetOrganizationEmployeesRS.Organization.Employees.Add(new Person{PersonId=2, FirstName="John", LastName="Smith", IsActive=false});

        return GetOrganizationEmployeesRS;
    }
}