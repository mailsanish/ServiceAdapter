using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Text.Json;

namespace SaneeCodes.ServiceAdapter;
public class SerializerHelper
    {
        public static String XmlSerialize(Object objectToSerialize)
        {
            return XmlSerialize(objectToSerialize, null);
        }
        public static String XmlSerialize(Object objectToSerialize, Type[] ExtraTypes)
        {
            System.Text.StringBuilder responseString = new System.Text.StringBuilder();
            StringWriter strWriter = new StringWriter();
            XmlSerializer objXmlSerializer = null;

            try
            {

                if (ExtraTypes != null && ExtraTypes.Length > 0)
                {
                    //objXmlSerializer = new XmlSerializer(objectToSerialize.GetType(), ExtraTypes);
                    XmlSerializerPool.XmlSerializerToken xsToken = XmlSerializerPool.GetXmlSerializerToken(objectToSerialize.GetType(), ExtraTypes);

                    using (XmlTextWriter xmlTextWriter = new XmlTextWriter(strWriter))
                    {
                        //objXmlSerializer.Serialize(xmlTextWriter, objectToSerialize);
                        xsToken.XmlSerializerInstance.Serialize(xmlTextWriter, objectToSerialize);
                        responseString.Append(strWriter.ToString());

                        //change the encoding
                        responseString.Replace(Encoding.Unicode.WebName, Encoding.UTF8.WebName, 0, 56);
                    }

                    XmlSerializerPool.ReleaseXmlSerializerToken(xsToken);
                }
                else
                {

                    objXmlSerializer = new XmlSerializer(objectToSerialize.GetType());
                    using (XmlTextWriter xmlTextWriter = new XmlTextWriter(strWriter))
                    {
                        objXmlSerializer.Serialize(xmlTextWriter, objectToSerialize);
                        responseString.Append(strWriter.ToString());

                        //change the encoding
                        responseString.Replace(Encoding.Unicode.WebName, Encoding.UTF8.WebName, 0, 56);
                    }
                }


            }
            catch
            {
                throw;
            }
            finally
            {
                if (strWriter != null)
                    strWriter.Close();
            }

            return responseString.ToString();
        }

        public static T XmlDeserialize<T>(String xmlString)
        {
            Object returnObject = null;
            Type returnType = typeof(T);

            try
            {
                XmlDocument objDoc = new XmlDocument();
                objDoc.LoadXml(xmlString);
                if (!String.IsNullOrEmpty(objDoc.DocumentElement.NamespaceURI))
                {
                    objDoc.LoadXml(objDoc.OuterXml.Replace(objDoc.DocumentElement.NamespaceURI, String.Empty));
                }
                XmlRootAttribute xRoot = new XmlRootAttribute();
                xRoot.ElementName = objDoc.DocumentElement.Name;
                xRoot.IsNullable = true;
                //XmlSerializer xmlSer = new System.Xml.Serialization.XmlSerializer(returnType, xRoot);
                XmlSerializerPool.XmlSerializerToken xsToken = XmlSerializerPool.GetXmlSerializerToken(returnType, xRoot);
                using (StringReader strReader = new StringReader(objDoc.OuterXml))
                {
                    //returnObject = xmlSer.Deserialize(strReader);
                    returnObject = xsToken.XmlSerializerInstance.Deserialize(strReader);
                }
                XmlSerializerPool.ReleaseXmlSerializerToken(xsToken);
            }
            catch
            {
                throw;
            }

            return (T)returnObject;
        }

        public static String JsonSerialize(Object objectToSerialize)
        {
           return JsonSerializer.Serialize(objectToSerialize);
        }
        public static T JsonDeserialize<T>(String jsonString)
        {
            return JsonSerializer.Deserialize<T>(jsonString);
        }
    }

    #region XmlSerializerPool

     class XmlSerializerPool
     {
        #region Fields
        static int poolIdCount = 0;
        static int MaxCountPerType = 5;

        internal enum InstanceType { Normal = 0, TypeArray = 1, RootAttrib = 2 }
        static List<XmlSerializerToken> XmlSerializerTokenList = new List<XmlSerializerToken>(); 
        #endregion

        #region XmlSerializerToken
        internal class XmlSerializerToken
        {

            internal int PoolId;
            internal string XmlCode = "";
            private XmlSerializer _xmlSerializerInstance;
            internal bool IsLocked;
            internal DateTime CreatedTime;
            internal bool IsError = false;
            internal string ErrorMessage;
            internal InstanceType InstanceType = InstanceType.Normal;
                        
            internal XmlSerializerToken()
            {
                //PoolId=++poolIdCount;
                CreatedTime = DateTime.Now;
                IsLocked = false;
            }

            internal XmlSerializer XmlSerializerInstance
            {
                get
                {

                    if (_xmlSerializerInstance == null && IsError == true)
                    {
                        throw new Exception("Error on creating  XmlSerializer instance(within NCL Plugin) for type=" + XmlCode + " Error: " + ErrorMessage);
                        //return null;
                    }
                    else
                        return _xmlSerializerInstance;

                }

                set
                {
                    _xmlSerializerInstance = value;
                }
            }

            internal XmlSerializerToken Clone()
            {
                return (XmlSerializerToken)this.MemberwiseClone();
            }
        }
        #endregion

        #region Methods

        internal static XmlSerializerToken GetXmlSerializerToken(Type mainType, Type[] ExtraTypes)
        {
            if (mainType == null)
            {
                XmlSerializerToken retXmlSerializerToken = new XmlSerializerToken();
                retXmlSerializerToken.CreatedTime = DateTime.Now;
                retXmlSerializerToken.IsError = true;
                retXmlSerializerToken.ErrorMessage = "Type passed to XmlSerializer is null";
                return retXmlSerializerToken.Clone();
            }

            if (ExtraTypes == null)
            {
                XmlSerializerToken retXmlSerializerToken = new XmlSerializerToken();
                retXmlSerializerToken.CreatedTime = DateTime.Now;
                retXmlSerializerToken.IsError = true;
                retXmlSerializerToken.ErrorMessage = "ExtraTypes passed to XmlSerializer is null";
                return retXmlSerializerToken.Clone();
            }
            else
            {


                string xmlCode = mainType.FullName + "&" + string.Concat(ExtraTypes.Select(t => t.FullName));

                XmlSerializerToken retXmlSerializerToken = GetXmlSerializerToken(xmlCode, mainType, ExtraTypes, null);

                if (retXmlSerializerToken == null)
                {
                    retXmlSerializerToken = new XmlSerializerToken();
                    retXmlSerializerToken.CreatedTime = DateTime.Now;
                    retXmlSerializerToken.XmlCode = xmlCode;
                    retXmlSerializerToken.IsError = true;
                    retXmlSerializerToken.ErrorMessage = "Error on creating XmlSerializer for " + xmlCode;
                    return retXmlSerializerToken.Clone();
                }
                else
                    return retXmlSerializerToken;
            }
        }

        internal static XmlSerializerToken GetXmlSerializerToken(Type mainType, XmlRootAttribute xRoot)
        {
            if (mainType == null)
            {
                XmlSerializerToken retXmlSerializerToken = new XmlSerializerToken();
                retXmlSerializerToken.CreatedTime = DateTime.Now;
                retXmlSerializerToken.IsError = true;
                retXmlSerializerToken.ErrorMessage = "Type passed to XmlSerializer is null";
                return retXmlSerializerToken.Clone();
            }

            if (xRoot == null)
            {
                XmlSerializerToken retXmlSerializerToken = new XmlSerializerToken();
                retXmlSerializerToken.CreatedTime = DateTime.Now;
                retXmlSerializerToken.IsError = true;
                retXmlSerializerToken.ErrorMessage = "XmlRootAttribute passed to XmlSerializer is null";
                return retXmlSerializerToken.Clone();
            }
            else
            {


                string xmlCode = mainType.FullName + "&" + (!string.IsNullOrWhiteSpace(xRoot.ElementName) ? xRoot.ElementName : (!string.IsNullOrWhiteSpace(xRoot.Namespace) ? xRoot.Namespace : ""));

                XmlSerializerToken retXmlSerializerToken = GetXmlSerializerToken(xmlCode, mainType, null, xRoot);

                if (retXmlSerializerToken == null)
                {
                    retXmlSerializerToken = new XmlSerializerToken();
                    retXmlSerializerToken.CreatedTime = DateTime.Now;
                    retXmlSerializerToken.XmlCode = xmlCode;
                    retXmlSerializerToken.IsError = true;
                    retXmlSerializerToken.ErrorMessage = "Error on creating XmlSerializer for " + xmlCode;
                    return retXmlSerializerToken.Clone();
                }
                else
                    return retXmlSerializerToken;
            }
        }

        private static XmlSerializerToken GetXmlSerializerToken(string xmlCode, Type mainType, Type[] ExtraTypes, XmlRootAttribute xRoot)
        {
            XmlSerializerToken retXmlSerializerToken = null;
            int checkCnt = 0;

            if (XmlSerializerTokenList.Count != 0)
            {
                while (checkCnt < 3 && retXmlSerializerToken == null)
                {
                    lock (XmlSerializerTokenList)
                    {
                        retXmlSerializerToken = XmlSerializerTokenList.FirstOrDefault(t => t.XmlCode == xmlCode && t.IsLocked == false);
                    }

                    if (retXmlSerializerToken == null)
                        System.Threading.Thread.Sleep(500);
                    else
                        retXmlSerializerToken.IsLocked = true;

                    checkCnt++;
                }
            }

            if (retXmlSerializerToken == null)
            {

                lock (XmlSerializerTokenList)
                {
                    retXmlSerializerToken = XmlSerializerTokenList.FirstOrDefault(t => t.XmlCode == xmlCode && t.IsLocked == true && DateTime.Now.Subtract(t.CreatedTime) > TimeSpan.FromMinutes(15));
                }

                if (retXmlSerializerToken != null)//returns expire dtoken
                {
                    retXmlSerializerToken.CreatedTime = DateTime.Now;
                    retXmlSerializerToken.PoolId = ++poolIdCount;
                    retXmlSerializerToken.IsLocked = true;

                    return retXmlSerializerToken;
                }

                int maxCount = 0;
                lock (XmlSerializerTokenList)
                {
                    maxCount = XmlSerializerTokenList.FindAll(t => t.XmlCode == xmlCode).Count;
                }

                if (retXmlSerializerToken == null && maxCount <= MaxCountPerType)
                {
                    retXmlSerializerToken = new XmlSerializerToken();
                    retXmlSerializerToken.XmlCode = xmlCode;

                    try
                    {
                        if (ExtraTypes != null)
                        {
                            retXmlSerializerToken.XmlSerializerInstance = new XmlSerializer(mainType, ExtraTypes);
                            retXmlSerializerToken.InstanceType = InstanceType.TypeArray;
                        }
                        else if (xRoot != null)
                        {
                            retXmlSerializerToken.XmlSerializerInstance = new XmlSerializer(mainType, xRoot);
                            retXmlSerializerToken.InstanceType = InstanceType.RootAttrib;
                        }

                        if (retXmlSerializerToken.XmlSerializerInstance != null)
                        {
                            retXmlSerializerToken.IsLocked = true;
                            retXmlSerializerToken.PoolId = ++poolIdCount;
                            retXmlSerializerToken.CreatedTime = DateTime.Now;

                            lock (XmlSerializerTokenList)
                            {
                                XmlSerializerTokenList.Add(retXmlSerializerToken);
                            }
                        }
                        else
                        {
                            retXmlSerializerToken.IsError = true;
                            retXmlSerializerToken.CreatedTime = DateTime.Now;
                            retXmlSerializerToken.XmlCode = xmlCode;
                            retXmlSerializerToken.ErrorMessage = "Error on creating XmlSerializer for " + xmlCode;
                        }
                    }
                    catch (Exception Ex)
                    {
                        retXmlSerializerToken.IsError = true;
                        retXmlSerializerToken.XmlCode = xmlCode;
                        retXmlSerializerToken.CreatedTime = DateTime.Now;
                        retXmlSerializerToken.ErrorMessage = Ex.Message;
                    }

                }
                else
                {
                    retXmlSerializerToken = new XmlSerializerToken();
                    retXmlSerializerToken.CreatedTime = DateTime.Now;
                    retXmlSerializerToken.XmlCode = xmlCode;
                    retXmlSerializerToken.IsError = true;
                    retXmlSerializerToken.ErrorMessage = "XmlSerializer instance creation limit reached maximum limit of " + MaxCountPerType;
                }
            }

            //if (retXmlSerializerToken != null)
            //{
            //    retXmlSerializerToken.IsLocked = true;
            //}
            return retXmlSerializerToken;
        }

        internal static bool ReleaseXmlSerializerToken(XmlSerializerToken xmlSerializerToken)
        {
            try
            {
                if (xmlSerializerToken != null)
                {
                    lock (XmlSerializerTokenList)
                    {
                        if (XmlSerializerTokenList.Any(t => t.PoolId == xmlSerializerToken.PoolId))
                            XmlSerializerTokenList.Find(t => t.PoolId == xmlSerializerToken.PoolId).IsLocked = false;
                    }
                    return true;
                }

                return false;
            }
            catch { return false; }
        } 
        #endregion

    }

    #endregion