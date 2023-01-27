using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace BlazorApp.Api
{
    public static class GetSimulatedInfosFunction
    {
        [FunctionName("GetSimulatedInfos")]
        public static IActionResult GetGetSimulatedInfos([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log)
        {
            var accType = req.Query.Where(x => x.Key == "accType").FirstOrDefault().Value;

            string sUrl = $"{ClsCommon.GetUrlServer()}/Server/GetSimulatedInfos?accType={accType}&accHolder=An";
            string sAccountInfo = ClsCommon.ExecuteHttpGet(sUrl);

            return new OkObjectResult(sAccountInfo);
        }
    }
}
