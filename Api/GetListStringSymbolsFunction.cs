using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace BlazorApp.Api
{
    public static class GetListStringSymbolsFunction
    {
        [FunctionName("GetListStringSymbols")]
        public static IActionResult GetListStringSymbols([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log)
        {
            var accType = req.Query.Where(x => x.Key == "accType").FirstOrDefault().Value;
            var accHolder = req.Query.Where(x => x.Key == "accHolder").FirstOrDefault().Value;

            string sUrl = $"{ClsCommon.GetUrlServer()}/Server/GetSymbols?accType={accType}&accHolder={accHolder}";
            List<string> listSymbol = ClsCommon.ExecuteHttpGet<List<string>>(sUrl);

            return new OkObjectResult(listSymbol);
        }
    }
}
