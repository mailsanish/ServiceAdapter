namespace SaneeCodes.Framework.Models.Transport;

public static class Controllers
{
    
    public class OrgService
    {    
        public static string Name
        {
            get{
                return  System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name??"";
            }
        }
        public enum ActionMethods {
                GetOrganization=1,
                GetOrganizationEmployees=2
        }
    }
    //define separate class/namespace per controller here
}
