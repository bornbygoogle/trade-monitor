using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using BlazorApp.Shared.CoreDto;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;

namespace BlazorApp.Api
{
    public static class GetLogKlinePotentialFunction
    {
        [FunctionName("GetLogKlinePotential")]
        public static IActionResult GetLogKlinePotential([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log)
        {
            var accType = req.Query.Where(x => x.Key == "accType").FirstOrDefault().Value;
            var accHolder = req.Query.Where(x => x.Key == "accHolder").FirstOrDefault().Value;
            var symbol = req.Query.Where(x => x.Key == "symbol").FirstOrDefault().Value;


            string sUrl = $"{ClsCommon.URL_SERVER}/Server/GetLogKlinePotential?accType={accType}&accHolder={accHolder}";

            if (!string.IsNullOrEmpty(symbol))
                sUrl += $"&symbol={symbol}";

            List<LogInfoItemDto> listKlines = ClsCommon.ExecuteHttpGet<List<LogInfoItemDto>>(sUrl);

            return new OkObjectResult(listKlines);
        }
    }
}
