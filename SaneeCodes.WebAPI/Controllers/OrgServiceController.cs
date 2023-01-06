using Microsoft.AspNetCore.Mvc;
using SaneeCodes.Framework.Models.Transport;
using SaneeCodes.WebAPI.Respository;

namespace SaneeCodes.WebAPI.Controllers;

//Manage all the routing config properly in respective files. Omitted all those here for brevity
[ApiController]
[Route("[controller]/[action]")]
public class OrgServiceController : ControllerBase
{
    IRepository _repository;
    public OrgServiceController()//(IRepository repository)
    {

       //Handle your dependecy injection here.  I have omitted all those for brevity
       // _repository=repository;

       _repository= new DemoRepository();
    }
    [HttpGet]
    public string GetOrganizationData(int orgId)
    {
        return "OrgDat"+orgId;
    }

    [HttpGet]
    public GetOrganizationRS GetOrganization(int orgId)
    {
        GetOrganizationRS getOrganizationRS= new GetOrganizationRS();
        try{
            getOrganizationRS= _repository.GetOrganization(orgId);
         }
         catch//(Exception Ex)
         {
                // TODO- Handling exception and updating base level Transaction status objects
         }
         return getOrganizationRS;
    }

    [HttpPost]
    public GetOrganizationEmployeesRS GetOrganizationEmployees([FromBody]GetOrganizationEmployeesRQ getOrganizationEmployeesRQ)
    {        
        GetOrganizationEmployeesRS getOrganizationEmployeesRS= new GetOrganizationEmployeesRS();
        try{
            getOrganizationEmployeesRS= _repository.GetOrganizationEmployees(getOrganizationEmployeesRQ);
         }
         catch//(Exception Ex)
         {
                // TODO- Handling exception and updating base level Transaction status objects
         }
         return getOrganizationEmployeesRS;
    }
}