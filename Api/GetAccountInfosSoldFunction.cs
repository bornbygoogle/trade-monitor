using BlazorApp.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace BlazorApp.Api
{
    public static class GetAccountInfosSoldFunction
    {
        private static bool _onWork = false;

        [FunctionName("GetAccountInfosSold")]
        public static IActionResult GetAccountInfosSold([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log)
        {
            List<DataItem> accRealSold = null;

            if (!_onWork)
            {
                _onWork = true;

                var accType = req.Query.Where(x => x.Key == "accType").FirstOrDefault().Value;
                var accHolder = req.Query.Where(x => x.Key == "accHolder").FirstOrDefault().Value;
                var nbrDays = req.Query.Where(x => x.Key == "nbrDays").FirstOrDefault().Value;
                var real = req.Query.Where(x => x.Key == "real").FirstOrDefault().Value;

                string sUrl = $"{ClsCommon.GetUrlServer()}/Server/GetAccountInfos{(real == "1" ? "Real" : "Simulated")}Sold?accType={accType}&accHolder={accHolder}{(!string.IsNullOrEmpty(nbrDays) ? $"&nbrDays={nbrDays}" : string.Empty)}";
                accRealSold = ClsCommon.ExecuteHttpGet<List<DataItem>>(sUrl);

                _onWork = false;
            }

            if (accRealSold != null)
                return new OkObjectResult(accRealSold);
            else
                return new OkObjectResult(null);
        }
    }
}
