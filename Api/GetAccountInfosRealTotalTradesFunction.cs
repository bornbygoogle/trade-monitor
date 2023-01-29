using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace BlazorApp.Api
{
    public static class GetAccountInfosRealTotalTradesFunction
    {
        [FunctionName("GetAccountInfosRealTotalTrades")]
        public static IActionResult GetAccountInfosRealTotalTrades([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log)
        {
            var accType = req.Query.Where(x => x.Key == "accType").FirstOrDefault().Value;
            var accHolder = req.Query.Where(x => x.Key == "accHolder").FirstOrDefault().Value;

            string sUrl = $"{ClsCommon.GetUrlServer()}/Server/GetAccountInfosRealTotalTrades?accType={accType}&accHolder={accHolder}";
            decimal accRealTotalTrades = ClsCommon.ExecuteHttpGet<decimal>(sUrl);

            return new OkObjectResult(accRealTotalTrades);
        }
    }
}
