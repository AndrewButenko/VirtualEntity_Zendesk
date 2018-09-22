using System;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json.Linq;

namespace D365Saturday.VirtualEntities
{
    public class ZendeskClient
    {
        private readonly string baseUrl;
        private readonly string preparedToken;

        public ZendeskClient(string instanceName, string userName, string token)
        {
            baseUrl = $"https://{instanceName}.zendesk.com";
            preparedToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userName}/token:{token}"));
        }

        private HttpWebRequest GetRequest(string requestUrl, string method)
        {
            var request = WebRequest.CreateHttp($"{baseUrl}{requestUrl}");
            request.Method = method;
            request.Accept = "application/json";
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", $"Basic {preparedToken}");

            return request;
        }

        public JObject ExecuteRequest(string query)
        {
            var requestUrl = "/api/v2/" + query;
            //var requestUrl = "/api/v2/tickets/2.json; -- get single ticket
            //var requestUrl = "/api/v2/search.json?query=" + Uri.EscapeDataString(query); -- search

            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            var request = GetRequest(requestUrl, "GET");

            using (var streamReader = new StreamReader(request.GetResponse().GetResponseStream()))
            {
                return JObject.Parse(streamReader.ReadToEnd());
            }
        }
    }
}
