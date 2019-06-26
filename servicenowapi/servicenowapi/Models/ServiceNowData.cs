using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace servicenowapi.Models
{
    public class ServiceNowData
    {
        public JToken Data { get; set; }
        public string NextLink { get; set; }

        public ServiceNowData(JToken data, string nextLink)
        {
            Data = data;
            NextLink = nextLink;
        }
    }
}
