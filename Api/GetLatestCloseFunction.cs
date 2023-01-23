using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

using BlazorApp.Shared;
using BlazorApp.Shared.CoreDto;
using static System.Net.WebRequestMethods;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.AccessControl;
using Newtonsoft.Json;

namespace BlazorApp.Api
{
    public static class GetLatestCloseFunction
    {
        [FunctionName("GetLatestClose")]
        public static IActionResult GetLatestClose([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log)
        {
            var accType = req.Query.Where(x => x.Key == "accType").FirstOrDefault().Value;
            var symbol = req.Query.Where(x => x.Key == "symbol").FirstOrDefault().Value;

            string sUrl = $"{ClsCommon.GetUrlServer()}/Server/GetLatestClose?accType={accType}&symbol={symbol}";

            decimal result = ClsCommon.ExecuteHttpGet<decimal>(sUrl);

            return new OkObjectResult(result);
        }
    }
}
