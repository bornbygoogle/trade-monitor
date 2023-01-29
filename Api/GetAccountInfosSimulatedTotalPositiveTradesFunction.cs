using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace BlazorApp.Api
{
    public static class GetAccountInfosSimulatedTotalPositiveTradesFunction
    {
        [FunctionName("GetAccountInfosSimulatedTotalPositiveTrades")]
        public static IActionResult GetAccountInfosSimulatedTotalPositiveTrades([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log)
        {
            var accType = req.Query.Where(x => x.Key == "accType").FirstOrDefault().Value;
            var accHolder = req.Query.Where(x => x.Key == "accHolder").FirstOrDefault().Value;

            string sUrl = $"{ClsCommon.GetUrlServer()}/Server/GetAccountInfosSimulatedTotalPositiveTrades?accType={accType}&accHolder={accHolder}";
            decimal accSimulatedTotalPositivesTrades = ClsCommon.ExecuteHttpGet<decimal>(sUrl);

            return new OkObjectResult(accSimulatedTotalPositivesTrades);
        }
    }
}
