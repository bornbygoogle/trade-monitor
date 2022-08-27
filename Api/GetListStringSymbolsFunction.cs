using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;

namespace BlazorApp.Api
{
    public static class GetListStringSymbolsFunction
    {
        [FunctionName("GetListStringSymbols")]
        public static IActionResult GetListStringSymbols([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log)
        {
            var accType = req.Query.Where(x => x.Key == "accType").FirstOrDefault().Value;
            var accHolder = req.Query.Where(x => x.Key == "accHolder").FirstOrDefault().Value;

            var symbols = GetListSymbols(accType, accHolder);

            return new OkObjectResult(symbols);
        }

        public static List<string> GetListSymbols(string accType, string accHolder)
        {
            List<string> listSymbol = new List<string>();

            try
            {
                using (var httpClient = new HttpClient())
                {
                    listSymbol = httpClient.GetFromJsonAsync<List<string>>($"{ClsCommon.URL_SERVER}/Logs/GetSymbols?accType={accType}&accHolder={accHolder}").Result;
                }
            }
            catch (Exception e) { }

            return listSymbol;
        }
    }
}
