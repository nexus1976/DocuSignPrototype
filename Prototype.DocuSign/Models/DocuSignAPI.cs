using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using Prototype.DocuSign.DAL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Linq;
using System.Net;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace Prototype.DocuSign.Models
{
    public class DocuSignAPI : IDocuSignAPI
    {
        public enum ResponseTypes
        {
            StringData, BinaryData
        }

        public virtual string DocuSignURL { get; set; }
        public virtual string DocuSignUserName { get; set; }
        public virtual string DocuSignPWD { get; set; }
        public virtual string DocuSignIntegratorKey { get; set; }
        public virtual string DocuSignAccountId { get; set; }
        public virtual string SenderViewURI { get; set; }

        public DocuSignAPI()
        { }
        public DocuSignAPI(string docuSignURL, string docuSignUserName, string docuSignPWD, string docuSignIntegratorKey, string docuSignAccountId)
        {
            this.DocuSignURL = docuSignURL;
            this.DocuSignUserName = docuSignUserName;
            this.DocuSignPWD = docuSignPWD;
            this.DocuSignIntegratorKey = docuSignIntegratorKey;
            this.DocuSignAccountId = docuSignAccountId;
        }
        public DocuSignAPI(System.Collections.Specialized.NameValueCollection config)
        {
            if (config.AllKeys.Contains("DocuSign.URL"))
                this.DocuSignURL = config["DocuSign.URL"];
            if (config.AllKeys.Contains("DocuSign.UserName"))
                this.DocuSignUserName = config["DocuSign.UserName"];
            if (config.AllKeys.Contains("DocuSign.Password"))
                this.DocuSignPWD = config["DocuSign.Password"];
            if (config.AllKeys.Contains("DocuSign.IntegratorKey"))
                this.DocuSignIntegratorKey = config["DocuSign.IntegratorKey"];
            if (config.AllKeys.Contains("DocuSign.AccountId"))
                this.DocuSignAccountId = config["DocuSign.AccountId"];
        }

        public virtual Uri Login()
        {
            Uri response = null;
            dynamic jsonResponse = this.GetJSONRestResponse(this.DocuSignURL + "login_information");
            response = new Uri(jsonResponse.loginAccounts[0].baseUrl.Value);
            return response;
        }
        public virtual string SendSingleDocument(string emailBody, string emailSubject, string documentName, byte[] document, string senderEmail, string senderName, decimal X, decimal Y, int pageNumber)
        {
            Uri baseURL = this.Login();
            string url = baseURL.ToString() + "/envelopes";
            HttpWebRequest request = initializeRequest(url, "POST", null);

            string xmlBody =
                    "<envelopeDefinition xmlns=\"http://www.docusign.com/restapi\">" +
                    "<emailSubject>" + emailSubject + "</emailSubject>" +
                    "<status>sent</status>" +
                    "<documents>" +
                    "<document>" +
                    "<documentId>1</documentId>" +
                    "<name>" + documentName + "</name>" +
                    "</document>" +
                    "</documents>" +
                            // add recipient(s)
                    "<recipients>" +
                    "<signers>" +
                    "<signer>" +
                    "<recipientId>1</recipientId>" +
                    "<email>" + senderEmail + "</email>" +
                    "<name>" + senderName + "</name>" +
                    "<tabs>" +
                    "<signHereTabs>" +
                    "<signHere>" +
                    "<xPosition>" + X.ToString() + "</xPosition>" + // default unit is pixels
                    "<yPosition>" + Y.ToString() + "</yPosition>" + // default unit is pixels
                    "<documentId>1</documentId>" +
                    "<pageNumber>" + pageNumber.ToString() + "</pageNumber>" +
                    "</signHere>" +
                    "</signHereTabs>" +
                    "</tabs>" +
                    "</signer>" +
                    "</signers>" +
                    "</recipients>" +
                    "</envelopeDefinition>";

            configureMultiPartFormDataRequest(request, xmlBody, documentName, document);
            string response = getResponseBody(request);
            string envelopeId = parseDataFromResponse(response, "envelopeId");
            return envelopeId;
        }
        public virtual string SendSingleDocumentSenderView(string emailBody, string emailSubject, string documentName, byte[] document, string senderEmail, string senderName)
        {
            string url = this.DocuSignURL + "login_information";
            HttpWebRequest request = initializeRequest(url, "GET", null);
            string response = getResponseBody(request);
            string baseURL = parseDataFromResponse(response, "baseUrl");
            url = baseURL + "/envelopes";
            request = initializeRequest(url, "POST", null);

            string xmlBody =
                    "<envelopeDefinition xmlns=\"http://www.docusign.com/restapi\">" +
                    "<emailSubject>" + emailSubject + "</emailSubject>" +
                    "<status>created</status>" + 	
                    "<documents>" +
                    "<document>" +
                    "<documentId>1</documentId>" +
                    "<name>" + documentName + "</name>" +
                    "</document>" +
                    "</documents>" +
                    "<recipients>" +
                    "<signers>" +
                    "<signer>" +
                    "<recipientId>1</recipientId>" +
                    "<email>" + senderEmail + "</email>" +
                    "<name>" + senderName + "</name>" +
                    "</signer>" +
                    "</signers>" +
                    "</recipients>" +
                    "</envelopeDefinition>";

            configureMultiPartFormDataRequest(request, xmlBody, documentName, document);
            response = getResponseBody(request);

            string envelopeId = parseDataFromResponse(response, "envelopeId");
            return envelopeId;
        }

        public virtual string GetSenderView(string envelopeId)
        {
            Uri baseURL = this.Login();
            string senderURL = string.Empty;
            string url = baseURL.ToString() + string.Format("/envelopes/{0}/views/sender", envelopeId);
            dynamic jsonResponse = this.PostJSONRestResponse(url);
            senderURL = jsonResponse.url;
            return senderURL;
        }
 
        public virtual IEnumerable<Document> GetStatusUpdate(IEnumerable<Document> listOfDocumentsToCheck)
        {
            if(listOfDocumentsToCheck != null && listOfDocumentsToCheck.Any())
            {
                listOfDocumentsToCheck = this.getStatusUpdate(listOfDocumentsToCheck, "SENT");
                listOfDocumentsToCheck = this.getStatusUpdate(listOfDocumentsToCheck, "CREATED");
            }
            return listOfDocumentsToCheck;
        }
        private IEnumerable<Document> getStatusUpdate(IEnumerable<Document> listOfDocumentsToCheck, string statusToCheck)
        {
            List<Document> documents = listOfDocumentsToCheck.Where(d => d.Status.ToUpper() == statusToCheck.ToUpper()).ToList();
            if (documents != null && documents.Any())
            {
                DateTime minDateTime = documents.Min(d => d.RequestedDate);
                string formattedDate = HttpUtility.UrlEncode(minDateTime.ToString("MM/dd/yyyy")).Replace("+", "%20");
                string url = this.DocuSignURL + string.Format("accounts/{0}/envelopes?from_date={1}&from_to_status={2}", this.DocuSignAccountId, formattedDate, statusToCheck.ToLower());
                dynamic jsonResponse = this.GetJSONRestResponse(url);

                using (var context = DNAContextFactory.CreateDNAContext())
                {
                    foreach (var item in jsonResponse.envelopes)
                    {
                        string envelopeId = item.envelopeId.ToString();
                        var doc = listOfDocumentsToCheck.Where(d => d.DocuSignEnvelopId == envelopeId).FirstOrDefault();
                        if (doc != null)
                        {
                            if (doc.Status.ToUpper() != item.status.ToString().ToUpper())
                            {
                                doc.Status = item.status.ToString().ToUpper();
                                var dbDoc = context.Documents.Where(d => d.DocuSignEnvelopId == envelopeId).FirstOrDefault();
                                if (dbDoc != null)
                                {
                                    dbDoc.Status = item.status.ToString().ToUpper();
                                    dbDoc.DocumentPDF = this.GetDocument(envelopeId);
                                    doc.DocumentPDF = dbDoc.DocumentPDF;
                                }
                            }
                        }
                    }
                    context.SaveChanges();
                }
            }
            return listOfDocumentsToCheck;
        }
        public virtual byte[] GetDocument(string envelopeId)
        {
            byte[] doc = new byte[]{};

            if(!string.IsNullOrWhiteSpace(envelopeId))
            {
                string url = this.DocuSignURL + string.Format("accounts/{0}/envelopes/{1}/documents/combined?show_changes=&watermark=&certificate=", this.DocuSignAccountId, envelopeId);
                doc = this.GetJSONRestResponse(url, ResponseTypes.BinaryData);
            }

            return doc;
        }

        #region DocuSign JSON API Helpers
        private dynamic PostJSONRestResponse(string url)
        {
            string stringResponse = string.Empty;
            dynamic jsonResponse = null;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
            using (WebClient client = new WebClient())
            {
                this.AddHeaders<WebClient>(client);
                stringResponse = client.UploadString(url, "{}");
                jsonResponse = JsonConvert.DeserializeObject(stringResponse);
            }
            return jsonResponse;
        }
        private dynamic GetJSONRestResponse(string url)
        {
            return this.GetJSONRestResponse(url, ResponseTypes.StringData);
        }
        private dynamic GetJSONRestResponse(string url, ResponseTypes responseType)
        {
            string stringResponse = string.Empty;
            byte[] byteResponse = new byte[] { };
            dynamic jsonResponse = null;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
            using (WebClient client = new WebClient())
            {
                this.AddHeaders<WebClient>(client);
                switch (responseType)
                {
                    case ResponseTypes.BinaryData:
                        byteResponse = client.DownloadData(url);
                        jsonResponse = byteResponse;
                        break;
                    default:
                        stringResponse = client.DownloadString(url);
                        jsonResponse = JsonConvert.DeserializeObject(stringResponse);
                        break;
                }
            }
            
            return jsonResponse;
        }
        private void AddHeaders<T>(T docuSignClient)
        {
            if(docuSignClient is WebClient)
            {
                var client = docuSignClient as WebClient;
                client.Headers.Add("Content-Type", "application/json");
                client.Headers.Add("Accept", "application/json");
                client.Headers.Add("X-DocuSign-Authentication",
                    string.Format("{{ \"Username\": \"{0}\", \"Password\": \"{1}\", \"IntegratorKey\": \"{2}\" }}", this.DocuSignUserName, this.DocuSignPWD, this.DocuSignIntegratorKey));
            }
        }
        #endregion

        #region DocuSign XML API Helpers
        private HttpWebRequest initializeRequest(string url, string method, string body)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;
            addRequestHeaders(request);
            if (body != null)
                addRequestBody(request, body);
            return request;
        }
        private void addRequestHeaders(HttpWebRequest request)
        {
            // authentication header can be in JSON or XML format.  XML used for this walkthrough:
            string authenticateStr =
                "<DocuSignCredentials>" +
                    "<Username>" + this.DocuSignUserName + "</Username>" +
                    "<Password>" + this.DocuSignPWD + "</Password>" +
                    "<IntegratorKey>" + this.DocuSignIntegratorKey + "</IntegratorKey>" + // global (not passed)
                    "</DocuSignCredentials>";
            request.Headers.Add("X-DocuSign-Authentication", authenticateStr);
            request.Accept = "application/xml";
            request.ContentType = "application/xml";
        }
        private void addRequestBody(HttpWebRequest request, string requestBody)
        {
            // create byte array out of request body and add to the request object
            byte[] body = System.Text.Encoding.UTF8.GetBytes(requestBody);
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(body, 0, requestBody.Length);
            dataStream.Close();
        }
        private void configureMultiPartFormDataRequest(HttpWebRequest request, string xmlBody, string docName, byte[] document)
        {
            // overwrite the default content-type header and set a boundary marker
            request.ContentType = "multipart/form-data; boundary=BOUNDARY";
            string fileContentType = GetFileContentType(docName);
            // start building the multipart request body
            string requestBodyStart = "\r\n\r\n--BOUNDARY\r\n" +
                "Content-Type: application/xml\r\n" +
                "Content-Disposition: form-data\r\n" +
                "\r\n" +
                xmlBody + "\r\n\r\n--BOUNDARY\r\n" + 	// our xml formatted envelopeDefinition
                "Content-Type: " + fileContentType + "\r\n" +
                "Content-Disposition: file; filename=\"" + docName + "\"; documentId=1\r\n" +
                "\r\n";
            string requestBodyEnd = "\r\n--BOUNDARY--\r\n\r\n";
            byte[] bodyStart = System.Text.Encoding.UTF8.GetBytes(requestBodyStart.ToString());
            byte[] bodyEnd = System.Text.Encoding.UTF8.GetBytes(requestBodyEnd.ToString());
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(bodyStart, 0, requestBodyStart.ToString().Length);
            dataStream.Write(document, 0, document.Length);
            dataStream.Write(bodyEnd, 0, requestBodyEnd.ToString().Length);
            dataStream.Close();
        }
        private string getResponseBody(HttpWebRequest request)
        {
            // read the response stream into a local string
            string responseText = null;
            try
            {
                HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse();
                StreamReader sr = new StreamReader(webResponse.GetResponseStream());
                responseText = sr.ReadToEnd();
                return responseText;
            }
            catch (WebException ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(ex.Message);

                try
                {
                    using (WebResponse response = ex.Response)
                    {
                        HttpWebResponse httpResponse = (HttpWebResponse)response;
                        using (Stream data = response.GetResponseStream())
                        {
                            using (var reader = new StreamReader(data))
                            {
                                string xml = reader.ReadToEnd();
                                using (XmlReader xmlReader = XmlReader.Create(new StringReader(xml)))
                                {
                                    xmlReader.ReadToFollowing("message");
                                    string text = xmlReader.ReadElementContentAsString();
                                    sb.Append(text);
                                }
                            }
                        }
                    }
                }
                catch { }

                Exception e = new Exception(sb.ToString());
                e.Source = ex.Source;
                throw e;
            }
        }
        private string parseDataFromResponse(string response, string searchToken)
        {
            // look for "searchToken" in the response body and parse its value
            using (XmlReader reader = XmlReader.Create(new StringReader(response)))
            {
                while (reader.Read())
                {
                    if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == searchToken))
                        return reader.ReadString();
                }
            }
            return null;
        }
        private string prettyPrintXml(string xml)
        {
            // print nicely formatted xml
            try
            {
                XDocument doc = XDocument.Parse(xml);
                return doc.ToString();
            }
            catch (Exception)
            {
                return xml;
            }
        }
        private string GetFileContentType(string docName)
        {
            string extension = string.IsNullOrWhiteSpace(docName) ? null : Path.GetExtension(docName);
            if (string.IsNullOrWhiteSpace(extension))
                return @"application/octet-stream";

            string contentType = null;
            switch (extension.Trim().ToLower())
            {
                case ".gif":
                    contentType = @"image/gif";
                    break;
                case ".tiff":
                    contentType = @"image/tiff";
                    break;
                case ".png":
                    contentType = @"image/png";
                    break;
                case ".jpg":
                case ".jpeg":
                    contentType = @"image/jpeg";
                    break;
                case ".pdf":
                    contentType = @"application/pdf";
                    break;
                default:
                    contentType = @"application/octet-stream";
                    break;
            }

            return contentType;
        }
        #endregion
    }

    public class DocuSignAPIFactory
    {
        public static IDocuSignAPI CreateDocuSignAPI()
        {
            return new DocuSignAPI(ConfigurationManager.AppSettings);
        }
    }
}