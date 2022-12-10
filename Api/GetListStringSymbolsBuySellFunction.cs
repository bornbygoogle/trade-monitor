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
    public static class GetListStringSymbolsBuySellFunction
    {
        [FunctionName("GetListStringSymbolsBuySell")]
        public static IActionResult GetListStringSymbolsBuySell([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log)
        {
            var accType = req.Query.Where(x => x.Key == "accType").FirstOrDefault().Value;
            var accHolder = req.Query.Where(x => x.Key == "accHolder").FirstOrDefault().Value;

            var symbols = GetListSymbolsBuySell(accType, accHolder);

            return new OkObjectResult(symbols);
        }

        public static List<string> GetListSymbolsBuySell(string accType, string accHolder)
        {
            string sUrl = $"{ClsCommon.GetUrlServer()}/Server/GetSymbolsBuySell?accType={accType}";
            List<string> listSymbol = ClsCommon.ExecuteHttpGet<List<string>>(sUrl);

            return listSymbol;
        }
    }
}
