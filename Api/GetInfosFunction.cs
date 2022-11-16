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
    public static class GetInfosFunction
    {
        [FunctionName("GetInfos")]
        public static IActionResult GetInfos([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log)
        {
            var accType = req.Query.Where(x => x.Key == "accType").FirstOrDefault().Value;
            var accHolder = req.Query.Where(x => x.Key == "accHolder").FirstOrDefault().Value;

            string sUrl = $"{ClsCommon.URL_SERVER}/Server/GetInfos?accType={accType}&accHolder={accHolder}";
            string sAccountInfo = ClsCommon.ExecuteHttpGet(sUrl);

            return new OkObjectResult(sAccountInfo);
        }
    }
}
