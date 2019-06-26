using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using servicenowapi.Handlers;

namespace servicenowapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class DataController : ControllerBase
    {
        private readonly string _username;
        private readonly string _password;
        public DataController(IConfiguration configuration)
        {
            _username = configuration["http-servicenowprod-username"];
            _password = configuration["http-servicenowprod-password"];

        }

        [HttpGet("table/{table}")]
        public async Task<IActionResult> GetDataAsync(string table, string sysparm_fields, int sysparm_limit, int sysparm_offset=0,string sysparm_query=null)
        {
            string hostname = HttpContext.Request.Host.Value;
            HttpHandler handler = new HttpHandler(_username, _password);
            string uri = $"https://equinor.service-now.com/api/now/table/{table}?sysparm_display_value=true&sysparm_exclude_reference_link=true&sysparm_fields={sysparm_fields}&sysparm_limit={sysparm_limit}&sysparm_offset={sysparm_offset}";
            if (!string.IsNullOrEmpty(sysparm_query))
                uri += $"&sysparm_query={sysparm_query}";
            var result = await handler.GetDataAsync(uri, hostname);
            Response.Headers.TryAdd("NextLink", result.NextLink);
            return Ok(result.Data);

        }
    }
}