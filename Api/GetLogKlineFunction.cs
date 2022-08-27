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

namespace BlazorApp.Api
{
    public static class GetLogKlineFunction
    {
        [FunctionName("GetLogKlines")]
        public static IActionResult GetLogKlines([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log)
        {
            var accType = req.Query.Where(x => x.Key == "accType").FirstOrDefault().Value;
            var accHolder = req.Query.Where(x => x.Key == "accHolder").FirstOrDefault().Value;
            var symbolName = req.Query.Where(x => x.Key == "symbolName").FirstOrDefault().Value;

            var symbols = GetLogKline(accType, accHolder, symbolName);

            return new OkObjectResult(symbols);
        }

        public static List<LogInfoItemDto> GetLogKline(string accType, string accHolder, string symbolName = null)
        {
            List<LogInfoItemDto> listKlines = new List<LogInfoItemDto>();

            try
            {
                string sUrl = $"{ClsCommon.URL_SERVER}/Logs/GetLogKline?accType={accType}&accHolder={accHolder}";

                if (!string.IsNullOrEmpty(symbolName))
                    sUrl += $"&symbolName={symbolName}";

                using (var httpClient = new HttpClient())
                {
                    listKlines = httpClient.GetFromJsonAsync<List<LogInfoItemDto>>(sUrl).Result;
                }
            }
            catch (Exception e) { }

            return listKlines;
        }
    }
}
