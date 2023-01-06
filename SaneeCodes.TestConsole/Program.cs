
using SaneeCodes.ServiceAdapter;
using SaneeCodes.Framework.Models.Transport;

Console.WriteLine("API Service Adapter Demo!");
//ServicAdapter usaage demo. Using this utility neither we need to create the POST request xml/json nor process the response xml/json. 
//We directly passing the object and directly receiving the object as response. All the intermediate processing is handled by ServiceAdapter library


//pass the WebAPI base url as the first param (read from config/db/provider fn)
IServiceAdapter servAdapter= new ServiceAdapter("http://localhost:5169");

//Service Adapter has 2 constructors and if we skip passing DataExchangeTyPeEnum, JSON will be the default.
//ServiceAdapter servAdapter= new ServiceAdapter("http://localhost:5169");

//Sample for GET transaction passing querystring
string querystring = "orgId=1";//add more items with & as separator
GetOrganizationRS getOrganizationRS = 
        servAdapter.Get<GetOrganizationRS>("OrgService", //controller name
                                           "GetOrganization",//acction method name
                                            querystring);//querystring if any
Console.WriteLine("Returned Organization: "+ getOrganizationRS.Organization.OrgId + "-"+getOrganizationRS.Organization.OrgName);

//Sample for GET transaction passing querystring - Using Enums in structured class to pass the action method name and controller name
//to avoid typo errors and also easy future update in names
string querystring2 = "orgId=2";//add more items with & as separator
GetOrganizationRS getOrganizationRS2 = 
        servAdapter.Get<GetOrganizationRS>(Controllers.OrgService.Name,//controller name - name can be passed as simple string too
                                            Controllers.OrgService.ActionMethods.GetOrganization.ToString(),//action method name - name can be passed as simple string too
                                            querystring2);
Console.WriteLine("Returned Organization: "+ getOrganizationRS2.Organization.OrgId + "-"+getOrganizationRS2.Organization.OrgName);

//Sample to initialize ServiceAdapter with DataExchangeType as param to constructor
servAdapter= new ServiceAdapter("http://localhost:5169",ServiceAdapter.DataExchangeTypeEnum.Xml);

//Sample for GET transaction passing querystring key/value paris as dictionary 
IDictionary<string,string> qParams= new Dictionary<string, string>();
qParams.Add("orgId","3");//add more items if needed
GetOrganizationRS getOrganizationRS3 = 
         servAdapter.Get<GetOrganizationRS>(Controllers.OrgService.Name,//controller
                                            Controllers.OrgService.ActionMethods.GetOrganization.ToString(),//action
                                            qParams);//querystring as dictionary
Console.WriteLine("Returned Organization: "+ getOrganizationRS3.Organization.OrgId + "-"+getOrganizationRS3.Organization.OrgName);

//Sample Post transaction passing object as param
GetOrganizationEmployeesRQ getOrganizationEmployeesRQ= new GetOrganizationEmployeesRQ();//model data to post to WebAPI
getOrganizationEmployeesRQ.OrgId=4;

GetOrganizationEmployeesRS getOrganizationEmployeesRS = 
        servAdapter.Post<GetOrganizationEmployeesRS,GetOrganizationEmployeesRQ>
                            (Controllers.OrgService.Name,//controller
                            Controllers.OrgService.ActionMethods.GetOrganizationEmployees.ToString(),//action method
                            getOrganizationEmployeesRQ,//posted model data
                            null);//querystring if any
Console.WriteLine("Returned Organization: "+ getOrganizationEmployeesRS.Organization.OrgId + "-"+getOrganizationEmployeesRS.Organization.OrgName 
                        + " with employee count="+(getOrganizationEmployeesRS.Organization.Employees?.Count().ToString()??"0"));

//DataExchange protocol can be switched in between
servAdapter.DataExchangeType=ServiceAdapter.DataExchangeTypeEnum.Json;

//Sample to use basic authentication -- We can extend ServiceAdapter to support any kind of authentication mechanism
servAdapter.SetCredentials("Test","Test123");

//Sample Post call with Json as DataExchangeType
getOrganizationEmployeesRQ= new GetOrganizationEmployeesRQ();//model data to post to WebAPI
getOrganizationEmployeesRQ.OrgId=5;

getOrganizationEmployeesRS = 
        servAdapter.Post<GetOrganizationEmployeesRS,GetOrganizationEmployeesRQ>
                            (Controllers.OrgService.Name,//controller
                            Controllers.OrgService.ActionMethods.GetOrganizationEmployees.ToString(),//action method
                            getOrganizationEmployeesRQ,//posted model data
                            "testkey=testvalue&key2=123");//querystring if any
Console.WriteLine("Returned Organization: "+ getOrganizationEmployeesRS.Organization.OrgId + "-"+getOrganizationEmployeesRS.Organization.OrgName 
                        + " with employee count="+(getOrganizationEmployeesRS.Organization.Employees?.Count().ToString()??"0"));



//-------------------------------------------

