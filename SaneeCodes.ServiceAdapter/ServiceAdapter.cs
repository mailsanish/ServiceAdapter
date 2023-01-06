using System.Net.Http;
using System.Net.Http.Headers;
using System.Linq;

namespace SaneeCodes.ServiceAdapter;
public class ServiceAdapter:IServiceAdapter
    {
        #region Fields
        
        string _userName, _password;
        public enum VerbType { Get, Post, Put, Delete }

        public enum DataExchangeTypeEnum
        {
            Xml,
            Json
        }

        private string _serviceUrl;
        public string ServiceUrl
        {
            get { return _serviceUrl; }
            set
            {

                _serviceUrl = value;

                if (!_serviceUrl.EndsWith("/"))
                    _serviceUrl = _serviceUrl + "/";
            }
        }

        DataExchangeTypeEnum _dataExchangeType=DataExchangeTypeEnum.Json;
        public DataExchangeTypeEnum DataExchangeType
        {
            get { return _dataExchangeType; }
            set { _dataExchangeType = value; }
        }
        #endregion

        #region Ctor
        public ServiceAdapter(string serviceUrl) : this(serviceUrl, DataExchangeTypeEnum.Json)
        {
        }
        public ServiceAdapter(string serviceUrl, DataExchangeTypeEnum dataExchangeType)
        {
            ServiceUrl = serviceUrl;
            _dataExchangeType = dataExchangeType;
        }

        #endregion

        #region Methods

        public void SetCredentials(string userName, string password)
        {
            _userName=userName;
            _password=password;
        }
        public void ClearCredentials()
        {
            _userName=null;
            _password=null;
        }

                
        public T Get<T>(string controllerName, string actionName, string queryString=null)
        {           
            return SendRequest<T>(controllerName,actionName, VerbType.Get, queryString,null);
        }

        
        public T Get<T>(string controllerName, string actionName, IDictionary<string,string> queryStringDict)
        {
            
            if(queryStringDict!=null)
            {
                string qsParams= string.Join("&",queryStringDict
                .Where(q=>q.Key!=null)
                .Select(q=>$"{q.Key}={q.Value}")
                .ToArray());
                return SendRequest<T>(controllerName,actionName, VerbType.Get, qsParams,null);
            }
            else
                return SendRequest<T>(controllerName,actionName, VerbType.Get, null,null);
        }


        
        public T Post<T, U>(string controllerName, string actionName, U transportDataModel, string queryString=null)
        {            
            return SendRequest<T, U>(controllerName,actionName, VerbType.Post, queryString,transportDataModel);
        }
        
        
        public T Post<T>(string controllerName, string actionName,string transportDataModel, string queryString=null)
        {
            return SendRequest<T>(controllerName,actionName, VerbType.Post, queryString, transportDataModel);
        }
        

        public T Put<T, U>(string controllerName, string actionName, U transportDataModel, string queryString=null)
        {            
            return SendRequest<T,U>(controllerName,actionName, VerbType.Put,queryString, transportDataModel);
        }
       
        
        public T Put<T>(string controllerName, string actionName, string transportDataModel, string queryString=null)
        {            
            return SendRequest<T>(controllerName,actionName, VerbType.Put, queryString, transportDataModel);
        }

        public T Delete<T>(string controllerName, string actionName, string queryString=null)
        {
            return SendRequest<T>(controllerName,actionName, VerbType.Delete, queryString, null);
        }

        

        #region Private Methods
        private T SendRequest<T>(string controllerName, string actionName, VerbType verbType, string queryString, string transportDataModel)
        {
            T returnObject = default(T);
            HttpResponseMessage httpResponse = null;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_serviceUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/" + _dataExchangeType.ToString().ToLower()));

                if(_userName!=null && _password!=null)
                {
                    string authInfo = Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(_userName + ":" + _password));                
                    client.DefaultRequestHeaders.Add("Authorization", $"Basic {authInfo}");
                }
                else if(_userName!=null)
                {
                    string authInfo = Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(_userName));                
                    client.DefaultRequestHeaders.Add("Authorization", $"{authInfo}");
                }

                string urlString = CreateRequestUrl(controllerName,actionName, queryString);
               

                switch (verbType)
                {

                    case VerbType.Get:
                        {
                            httpResponse = client.GetAsync(urlString).Result;
                            break;
                        }
                    case VerbType.Post:
                        {
                            if (transportDataModel == null)
                                transportDataModel = "";
                            var content = new StringContent(transportDataModel);
                            httpResponse = client.PostAsync(urlString, content).Result;
                            break;
                        }
                    case VerbType.Put:
                        {
                            if (transportDataModel == null)
                                transportDataModel = "";
                            var content = new StringContent(transportDataModel);
                            httpResponse = client.PutAsync(urlString, content).Result;
                            break;
                        }
                    case VerbType.Delete:
                        {
                            httpResponse = client.DeleteAsync(urlString).Result;
                            break;
                        }
                }
                if (httpResponse?.IsSuccessStatusCode??false)
                {
                    try
                    {
                        try{
                            //returnObject = SerializerHelper.XmlDeserialize<T>(httpResponse.Content.ReadAsStringAsync().Result);
                            returnObject = httpResponse.Content.ReadAsAsync<T>().Result;
                        }
                        catch{
                            string responseString = httpResponse.Content.ReadAsStringAsync().Result;
                        
                            if(_dataExchangeType==DataExchangeTypeEnum.Xml)
                                returnObject = SerializerHelper.XmlDeserialize<T>(responseString);
                            else
                                returnObject=SerializerHelper.JsonDeserialize<T>(responseString);
                        }
                    }
                    catch (Exception Ex)
                    {
                        throw new Exception("Error on serializing service returned data." + Ex.Message);
                    }
                }
                else
                {
                    throw new Exception($"Unsuccessful on connecting to Web API: {httpResponse?.StatusCode} | {httpResponse?.ReasonPhrase}");
                }

            }

            return returnObject;
        }

        private T SendRequest<T,U>(string controllerName, string actionName, VerbType verbType, string queryString, U transportDataModel)
        {
            T returnObject = default(T);
            HttpResponseMessage httpResponse = null;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_serviceUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/" + _dataExchangeType.ToString().ToLower()));
                
                if(_userName!=null && _password!=null)
                {
                    string authInfo = Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(_userName + ":" + _password));                
                    client.DefaultRequestHeaders.Add("Authorization", $"Basic {authInfo}");
                }
                else if(_userName!=null)
                {
                    string authInfo = Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(_userName));                
                    client.DefaultRequestHeaders.Add("Authorization", $"{authInfo}");
                }

                string urlString = CreateRequestUrl(controllerName,actionName, queryString);
              
                switch (verbType)
                {

                    case VerbType.Get:
                        {
                            httpResponse = client.GetAsync(urlString).Result;
                            break;
                        }
                    case VerbType.Post:
                        {
                            System.Net.Http.Formatting.MediaTypeFormatter formatter=null;
                            if (_dataExchangeType == DataExchangeTypeEnum.Xml)
                            {
                                formatter = new System.Net.Http.Formatting.XmlMediaTypeFormatter();                                
                            }
                            else
                            {
                                formatter = new System.Net.Http.Formatting.JsonMediaTypeFormatter();                                
                            }

                            httpResponse = client.PostAsync<U>(urlString, transportDataModel, formatter).Result;

                            if (!(httpResponse?.IsSuccessStatusCode??false))
                            {                                
                                if (_dataExchangeType == DataExchangeTypeEnum.Xml)
                                {
                                    var xml = SerializerHelper.XmlSerialize(transportDataModel); 
                                    var stringContent = new StringContent(xml, System.Text.UnicodeEncoding.UTF8, "application/xml");
                                    httpResponse=client.PostAsync(urlString, stringContent).Result; 
                                }
                                else
                                {
                                    var json = SerializerHelper.JsonSerialize(transportDataModel); 
                                    var stringContent = new StringContent(json, System.Text.UnicodeEncoding.UTF8, "application/json");
                                    httpResponse=client.PostAsync(urlString, stringContent).Result;
                                }
                                
                            }

                            
                            break;
                        }
                    case VerbType.Put:
                        {
                            System.Net.Http.Formatting.MediaTypeFormatter formatter = null;
                            if (_dataExchangeType == DataExchangeTypeEnum.Xml)
                                formatter = new System.Net.Http.Formatting.XmlMediaTypeFormatter();
                            else
                                formatter = new System.Net.Http.Formatting.JsonMediaTypeFormatter();
                            httpResponse = client.PutAsync(urlString, transportDataModel, formatter).Result;
                            if (!(httpResponse?.IsSuccessStatusCode??false))
                            {                                
                                if (_dataExchangeType == DataExchangeTypeEnum.Xml)
                                {
                                    var xml = SerializerHelper.XmlSerialize(transportDataModel); 
                                    var stringContent = new StringContent(xml, System.Text.UnicodeEncoding.UTF8, "application/xml");
                                    httpResponse=client.PutAsync(urlString, stringContent).Result; 
                                }
                                else
                                {
                                    var json = SerializerHelper.JsonSerialize(transportDataModel); 
                                    var stringContent = new StringContent(json, System.Text.UnicodeEncoding.UTF8, "application/json");
                                    httpResponse=client.PutAsync(urlString, stringContent).Result;
                                }
                                
                            }
                            break;
                        }
                    case VerbType.Delete:
                        {
                            httpResponse = client.DeleteAsync(urlString).Result;
                            break;
                        }
                }
                if (httpResponse?.IsSuccessStatusCode??false)
                {
                    try
                    {
                        try{
                            returnObject = httpResponse.Content.ReadAsAsync<T>().Result;
                        }
                        catch{
                            string responseString = httpResponse.Content.ReadAsStringAsync().Result;
                        
                            if(_dataExchangeType==DataExchangeTypeEnum.Xml)
                                returnObject = SerializerHelper.XmlDeserialize<T>(responseString);
                            else
                                returnObject=SerializerHelper.JsonDeserialize<T>(responseString);
                        }
                    }
                    catch (Exception Ex)
                    {
                        throw new Exception("Error on serializing service returned data." + Ex.Message);
                    }
                }
                else
                {
                    throw new Exception($"Unsuccessful on connecting to Web API: {httpResponse?.StatusCode} | {httpResponse?.ReasonPhrase}");
                }

            }

            return returnObject;
        }

        private T SendRequest<T>(VerbType verbType, string controllerName, string queryString, string transportDataModel)
        {
            T returnObject = default(T);
            HttpResponseMessage httpResponse = null;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_serviceUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/" + _dataExchangeType.ToString().ToLower()));

                if(_userName!=null && _password!=null)
                {
                    string authInfo = Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(_userName + ":" + _password));                
                    client.DefaultRequestHeaders.Add("Authorization", $"Basic {authInfo}");
                }
                else if(_userName!=null)
                {
                    string authInfo = Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(_userName));                
                    client.DefaultRequestHeaders.Add("Authorization", $"{authInfo}");
                }

                string urlString = CreateRequestUrl(controllerName, "", queryString);


                switch (verbType)
                {

                    case VerbType.Get:
                        {
                            httpResponse = client.GetAsync(urlString).Result;
                            break;
                        }
                    case VerbType.Post:
                        {
                            if (transportDataModel == null)
                                transportDataModel = "";
                            var content = new StringContent(transportDataModel);
                            httpResponse = client.PostAsync(urlString, content).Result;
                            break;
                        }
                    case VerbType.Put:
                        {
                            if (transportDataModel == null)
                                transportDataModel = "";
                            var content = new StringContent(transportDataModel);
                            httpResponse = client.PutAsync(urlString, content).Result;
                            break;
                        }
                    case VerbType.Delete:
                        {
                            httpResponse = client.DeleteAsync(urlString).Result;
                            break;
                        }
                }
                

                if (httpResponse?.IsSuccessStatusCode??false)
                {
                    try
                    {
                        try{
                            returnObject = httpResponse.Content.ReadAsAsync<T>().Result;
                        }
                        catch{
                            string responseString = httpResponse.Content.ReadAsStringAsync().Result;
                        
                            if(_dataExchangeType==DataExchangeTypeEnum.Xml)
                                returnObject = SerializerHelper.XmlDeserialize<T>(responseString);
                            else
                                returnObject=SerializerHelper.JsonDeserialize<T>(responseString);
                        }

                        
                        
                    }
                    catch (Exception Ex)
                    {
                        throw new Exception("Error on serializing API response." + Ex.Message);
                    }
                }
                // else if(httpResponse.Content!=null)
                // {
                //     try
                //     {
                //         if (_dataExchangeType == DataExchangeTypeEnum.Xml)
                //             returnObject = SerializerHelper.XmlDeserialize<T>(httpResponse.Content.ReadAsStringAsync().Result);
                //         else
                //             returnObject = JsonManager.JsonDeserialize<T>(httpResponse.Content.ReadAsStringAsync().Result);
                        
                //     }
                //     catch //(Exception Ex)
                //     {
                //         throw new Exception($"Unknown content. Status: {(int)httpResponse.StatusCode} | {httpResponse.ReasonPhrase}");
                //     }
                // }
                // else
                // {

                //     throw new Exception($"Missing content. Status: {(int)httpResponse.StatusCode} | {httpResponse.ReasonPhrase}");
                // }
                else
                {
                    throw new Exception($"Unsuccessful on connecting to Web API: {httpResponse?.StatusCode} | {httpResponse?.ReasonPhrase}");
                }

            }

            return returnObject;
        }

        private T SendRequest<T,U>(VerbType verbType, string controllerName, string queryString, U transportDataModel)
        {
            T returnObject = default(T);
            HttpResponseMessage httpResponse = null;
            using (var client = new System.Net.Http.HttpClient())
            {
                client.BaseAddress = new Uri(_serviceUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/" + _dataExchangeType.ToString().ToLower()));

                if(_userName!=null && _password!=null)
                {
                    string authInfo = Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(_userName + ":" + _password));                
                    client.DefaultRequestHeaders.Add("Authorization", $"Basic {authInfo}");
                }
                else if(_userName!=null)
                {
                    string authInfo = Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(_userName));                
                    client.DefaultRequestHeaders.Add("Authorization", $"{authInfo}");
                }

                string urlString = CreateRequestUrl(controllerName, "", queryString);


                switch (verbType)
                {

                    case VerbType.Get:
                        {
                            httpResponse = client.GetAsync(urlString).Result;
                            break;
                        }
                    case VerbType.Post:
                        {
                            System.Net.Http.Formatting.MediaTypeFormatter formatter=null;
                            if (_dataExchangeType == DataExchangeTypeEnum.Xml)
                                formatter = new System.Net.Http.Formatting.XmlMediaTypeFormatter();
                            else
                                formatter = new System.Net.Http.Formatting.JsonMediaTypeFormatter();

                            httpResponse = client.PostAsync<U>(urlString, transportDataModel, formatter).Result;
                            if (!(httpResponse?.IsSuccessStatusCode??false))
                            {                                
                                if (_dataExchangeType == DataExchangeTypeEnum.Xml)
                                {
                                    var xml = SerializerHelper.XmlSerialize(transportDataModel); 
                                    var stringContent = new StringContent(xml, System.Text.UnicodeEncoding.UTF8, "application/xml");
                                    httpResponse=client.PostAsync(urlString, stringContent).Result; 
                                }
                                else
                                {
                                    var json = SerializerHelper.JsonSerialize(transportDataModel); 
                                    var stringContent = new StringContent(json, System.Text.UnicodeEncoding.UTF8, "application/json");
                                    httpResponse=client.PostAsync(urlString, stringContent).Result;
                                }
                                
                            }
                            break;
                        }
                    case VerbType.Put:
                        {
                            System.Net.Http.Formatting.MediaTypeFormatter formatter = null;
                            if (_dataExchangeType == DataExchangeTypeEnum.Xml)
                                formatter = new System.Net.Http.Formatting.XmlMediaTypeFormatter();
                            else
                                formatter = new System.Net.Http.Formatting.JsonMediaTypeFormatter();
                            httpResponse = client.PutAsync(urlString, transportDataModel, formatter).Result;
                            if (!(httpResponse?.IsSuccessStatusCode??false))
                            {                                
                                if (_dataExchangeType == DataExchangeTypeEnum.Xml)
                                {
                                    var xml = SerializerHelper.XmlSerialize(transportDataModel); 
                                    var stringContent = new StringContent(xml, System.Text.UnicodeEncoding.UTF8, "application/xml");
                                    httpResponse=client.PutAsync(urlString, stringContent).Result; 
                                }
                                else
                                {
                                    var json = SerializerHelper.JsonSerialize(transportDataModel); 
                                    var stringContent = new StringContent(json, System.Text.UnicodeEncoding.UTF8, "application/json");
                                    httpResponse=client.PutAsync(urlString, stringContent).Result;
                                }
                                
                            }
                            break;
                        }
                    case VerbType.Delete:
                        {
                            httpResponse = client.DeleteAsync(urlString).Result;
                            break;
                        }
                }
                if (httpResponse?.IsSuccessStatusCode??false)
                {
                    try{
                            returnObject = httpResponse.Content.ReadAsAsync<T>().Result;
                        }
                        catch{
                            string responseString = httpResponse.Content.ReadAsStringAsync().Result;
                        
                            if(_dataExchangeType==DataExchangeTypeEnum.Xml)
                                returnObject = SerializerHelper.XmlDeserialize<T>(responseString);
                            else
                                returnObject=SerializerHelper.JsonDeserialize<T>(responseString);
                        }
                }
                else
                {
                    throw new Exception($"Unsuccessful on connecting to Web API: {httpResponse?.StatusCode} | {httpResponse?.ReasonPhrase}");
                }
            }

            return returnObject;
        }

       
        private string CreateRequestUrl(string controller, string action, string queryString)
        {
            string url;

            if(string.IsNullOrWhiteSpace(queryString))
            {
                url = $"{_serviceUrl}{controller}/{action}";
            }
            else
            {
                queryString = queryString.TrimStart().StartsWith("?") ? queryString.Trim().Remove(0, 1) : queryString.Trim();

                url = $"{_serviceUrl}{controller}/{action}?{queryString}";
            }
            

            return url;
        }


        #endregion

        #endregion
    }
