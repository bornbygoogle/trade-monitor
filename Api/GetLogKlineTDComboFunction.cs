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
    public static class GetLogKlineTDComboFunction
    {
        [FunctionName("GetLogKlineTDCombo")]
        public static IActionResult GetLogKlineTDCombo([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log)
        {
            var accType = req.Query.Where(x => x.Key == "accType").FirstOrDefault().Value;
            var accHolder = req.Query.Where(x => x.Key == "accHolder").FirstOrDefault().Value;
            var symbolName = req.Query.Where(x => x.Key == "symbolName").FirstOrDefault().Value;

            List<LogInfoItemDto> listKlines = new List<LogInfoItemDto>();

            try
            {
                string sUrl = $"{ClsCommon.URL_SERVER}/Logs/GetLogKlineTDCombo?accType={accType}&accHolder={accHolder}";

                if (!string.IsNullOrEmpty(symbolName))
                    sUrl += $"&symbolName={symbolName}";

                using (var httpClient = new HttpClient())
                {
                    listKlines = httpClient.GetFromJsonAsync<List<LogInfoItemDto>>(sUrl).Result;
                }
            }
            catch (Exception e) { }

            return new OkObjectResult(listKlines);
        }
    }
}
