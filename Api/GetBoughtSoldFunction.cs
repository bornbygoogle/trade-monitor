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
    public static class GetBoughtSoldFunction
    {
        [FunctionName("GetBoughtSold")]
        public static IActionResult GetBoughtSold([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log)
        {
            var accType = req.Query.Where(x => x.Key == "accType").FirstOrDefault().Value;
            var accHolder = req.Query.Where(x => x.Key == "accHolder").FirstOrDefault().Value;
            var symbol = req.Query.Where(x => x.Key == "symbol").FirstOrDefault().Value;

            List<LogInfoItemDto> logBaseBoughtSold = new List<LogInfoItemDto>();

            try
            {
                string sUrl = $"{ClsCommon.URL_SERVER}/Server/GetBoughtSold?accType={accType}&accHolder={accHolder}";

                if (!string.IsNullOrEmpty(symbol))
                    sUrl += $"&symbol={symbol}";

                using (var httpClient = new HttpClient())
                {
                    logBaseBoughtSold = httpClient.GetFromJsonAsync<List<LogInfoItemDto>>(sUrl).Result;
                }
            }
            catch (Exception e) { }

            return new OkObjectResult(logBaseBoughtSold);
        }
    }
}
