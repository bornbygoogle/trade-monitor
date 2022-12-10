using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

using BlazorApp.Shared;
using BlazorApp.Shared.CoreDto;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BlazorApp.Api
{
    public static class GetBaseBoughtSoldFunction
    {
        [FunctionName("GetBaseBoughtSold")]
        public static IActionResult GetBaseBoughtSold([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log)
        {
            var accType = req.Query.Where(x => x.Key == "accType").FirstOrDefault().Value;
            var accHolder = req.Query.Where(x => x.Key == "accHolder").FirstOrDefault().Value;
            var symbol = req.Query.Where(x => x.Key == "symbol").FirstOrDefault().Value;

            string sUrl = $"{ClsCommon.GetUrlServer()}/Server/GetBaseBoughtSold?accType={accType}";

            List<LogInfoItemDto> logBaseBoughtSold = new List<LogInfoItemDto>();

            byte[] tmpResult = ClsCommon.ExecuteHttpGetByteArray(sUrl);

            if (tmpResult != null && tmpResult.Length > 0 && ClsUtil.ByteArrayToStringUnzipIfNedeed(tmpResult, System.Text.Encoding.UTF8, out string response, out string msgErr) && !string.IsNullOrEmpty(response) && string.IsNullOrEmpty(msgErr))
                logBaseBoughtSold = JsonConvert.DeserializeObject<List<LogInfoItemDto>>(response);

            return new OkObjectResult(logBaseBoughtSold);
        }
    }
}
