using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace BlazorApp.Api
{
    public static class GetAccountInfosFirstTradeDateFunction
    {
        private static bool _onWork = false;

        [FunctionName("GetAccountInfosFirstTradeDate")]
        public static IActionResult GetAccountInfosFirstTradeDate([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log)
        {
            string firstTradeDate = string.Empty;

            if (!_onWork)
            {
                _onWork = true;

                var accType = req.Query.Where(x => x.Key == "accType").FirstOrDefault().Value;
                var accHolder = req.Query.Where(x => x.Key == "accHolder").FirstOrDefault().Value;
                var nbrDays = req.Query.Where(x => x.Key == "nbrDays").FirstOrDefault().Value;
                var real = req.Query.Where(x => x.Key == "real").FirstOrDefault().Value;

                string sUrl = $"{ClsCommon.GetUrlServer()}/Server/GetAccountInfos{(real == "1" ? "Real" : "Simulated")}FirstTradeDate?accType={accType}&accHolder={accHolder}{(!string.IsNullOrEmpty(nbrDays) ? $"&nbrDays={nbrDays}" : string.Empty)}";
                firstTradeDate = ClsCommon.ExecuteHttpGet(sUrl);

                _onWork = false;
            }

            return new OkObjectResult(firstTradeDate);
        }
    }
}
