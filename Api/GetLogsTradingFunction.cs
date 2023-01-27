using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace BlazorApp.Api
{
    public static class GetLogsTradingFunction
    {
        [FunctionName("GetLogsTrading")]
        public static IActionResult GetLogsTrading([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log)
        {
            var accType = req.Query.Where(x => x.Key == "accType").FirstOrDefault().Value;
            var accHolder = req.Query.Where(x => x.Key == "accHolder").FirstOrDefault().Value;
            var symbol = req.Query.Where(x => x.Key == "symbol").FirstOrDefault().Value;

            var symbols = ClsCommon.GetLogs("GetLogsTrading", accType, accHolder, symbol);

            return new OkObjectResult(symbols);
        }
    }
}
