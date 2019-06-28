using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using servicenowapi.Models;
using System.IO;

namespace servicenowapi.Handlers
{
    public class HttpHandler
    {

        public HttpClient ServiceNowHttpClient { get; set; }

        public HttpHandler(string username, string password)
        {
            ServiceNowHttpClient = new HttpClient();
            ServiceNowHttpClient.DefaultRequestHeaders.Authorization = GetAuthenticationHeaderValue(username, password);
        }

        private string GetNextLink(string headervalue, string hostname)
        {
            try
            {
                var linkArray = headervalue.Split("<").ToList();
                string nextLink = linkArray.Where(a => a.Contains("next")).Single()
                    .Replace(">;rel=\"next\",", "")
                    .Replace("equinor.service-now.com/api/now/", $"{hostname}/api/data/")
                    .Replace("sysparm_display_value=true&sysparm_exclude_reference_link=true&", "");
                return nextLink;
            }
            catch
            {
                return null;
            }
        }

        private AuthenticationHeaderValue GetAuthenticationHeaderValue(string username, string password)
        {
            var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");
            var token = Convert.ToBase64String(byteArray);
            var authHeader = new AuthenticationHeaderValue("Basic", token);
            return authHeader;
        }

        private JToken FormatData(JToken values)
        {
            foreach (var row in values)
            {
                foreach (dynamic column in row)
                {
                    string name = column.Name;
                    string item = column.Value.ToString();
                    if (string.IsNullOrWhiteSpace(item))
                        row[name] = null;
                }
            }
            return values;

        }

        public async Task<ServiceNowData> GetDataAsync(string uri, string hostname)
        {
            string nextLink;
            var response = await ServiceNowHttpClient.GetAsync(uri);
            string content = await response.Content.ReadAsStringAsync();

            if(response.Headers.TryGetValues("Link", out IEnumerable<string> headers))
            {
                string nextLinkHeader = headers.Single();
                nextLink = GetNextLink(nextLinkHeader, hostname);
            }
            else
            {
                nextLink = null;
            }
            var values = FormatData(JsonConvert.DeserializeObject<JToken>(content)["result"]);
              
            ServiceNowData result = new ServiceNowData(values, nextLink);
            return result;
        }


    }
}
