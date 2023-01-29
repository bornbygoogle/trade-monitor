using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace BlazorApp.Api
{
    public static class GetAccountInfosSimulatedProfitQuoteFunction
    {
        [FunctionName("GetAccountInfosSimulatedProfitQuote")]
        public static IActionResult GetAccountInfosSimulatedProfitQuote([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log)
        {
            var accType = req.Query.Where(x => x.Key == "accType").FirstOrDefault().Value;
            var accHolder = req.Query.Where(x => x.Key == "accHolder").FirstOrDefault().Value;
            var quote = req.Query.Where(x => x.Key == "quote").FirstOrDefault().Value;
            var nbrDays = req.Query.Where(x => x.Key == "nbrDays").FirstOrDefault().Value;

            string sUrl = $"{ClsCommon.GetUrlServer()}/Server/GetAccountInfosSimulatedProfitQuote?accType={accType}&accHolder={accHolder}&quote={quote}{(!string.IsNullOrEmpty(nbrDays) ? nbrDays : string.Empty)}";
            double accSimulatedProfit = ClsCommon.ExecuteHttpGet<double>(sUrl);

            return new OkObjectResult(accSimulatedProfit);
        }
    }
}
