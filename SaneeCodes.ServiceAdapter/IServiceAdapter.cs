namespace SaneeCodes.ServiceAdapter;

public interface IServiceAdapter
{
    ServiceAdapter.DataExchangeTypeEnum DataExchangeType { get; set; }
    string ServiceUrl { get; set; }

    void SetCredentials(string userName, string password);
    void ClearCredentials();
   
    T Get<T>(string controllerName, string actionName, string queryString = null);
    T Get<T>(string controllerName, string actionName, IDictionary<string, string> queryStringDict);
    T Post<T, U>(string controllerName, string actionName, U transportDataModel, string queryString = null);
    T Post<T>(string controllerName, string actionName, string transportDataModel, string queryString = null);
    T Put<T, U>(string controllerName, string actionName, U transportDataModel, string queryString = null);
    T Put<T>(string controllerName, string actionName, string transportDataModel, string queryString = null);    
    T Delete<T>(string controllerName, string actionName, string queryString = null);
}






