using SaneeCodes.Framework.Models.Transport;

namespace SaneeCodes.WebAPI.Respository;

public interface IRepository
{
    GetOrganizationRS GetOrganization(int orgId);

    GetOrganizationEmployeesRS GetOrganizationEmployees(GetOrganizationEmployeesRQ getOrganizationEmployees);
}