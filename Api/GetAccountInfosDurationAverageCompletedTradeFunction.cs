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
    public static class GetAccountInfosDurationAverageCompletedTradeFunction
    {
        private static bool _onWork = false;

        [FunctionName("GetAccountInfosDurationAverageCompletedTrade")]
        public static IActionResult GetAccountInfosDurationAverageCompletedTrade([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log)
        {
            List<DataItem> listDurationAverageCompletedTrade = null;

            if (!_onWork)
            {
                _onWork = true;

                var accType = req.Query.Where(x => x.Key == "accType").FirstOrDefault().Value;
                var accHolder = req.Query.Where(x => x.Key == "accHolder").FirstOrDefault().Value;
                var nbrDays = req.Query.Where(x => x.Key == "nbrDays").FirstOrDefault().Value;
                var real = req.Query.Where(x => x.Key == "real").FirstOrDefault().Value;

                string sUrl = $"{ClsCommon.GetUrlServer()}/Server/GetAccountInfos{(real == "1" ? "Real" : "Simulated")}DurationAverageCompletedTrade?accType={accType}&accHolder={accHolder}{(!string.IsNullOrEmpty(nbrDays) ? $"&nbrDays={nbrDays}" : string.Empty)}";
                listDurationAverageCompletedTrade = ClsCommon.ExecuteHttpGet<List<DataItem>>(sUrl);

                _onWork = false;
            }

            if (listDurationAverageCompletedTrade != null)
                return new OkObjectResult(listDurationAverageCompletedTrade);
            else
                return new OkObjectResult(null);

            
        }
    }
}
