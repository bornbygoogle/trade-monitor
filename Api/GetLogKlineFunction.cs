using BlazorApp.Shared;
using BlazorApp.Shared.CoreDto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace BlazorApp.Api
{
    public static class GetLogKlineFunction
    {
        [FunctionName("GetLogKlines")]
        public static IActionResult GetLogKlines([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log)
        {
            var accType = req.Query.Where(x => x.Key == "accType").FirstOrDefault().Value;
            var accHolder = req.Query.Where(x => x.Key == "accHolder").FirstOrDefault().Value;
            var symbol = req.Query.Where(x => x.Key == "symbol").FirstOrDefault().Value;

            string sUrl = $"{ClsCommon.GetUrlServer()}/Server/GetLogKline?accType={accType}&accHolder={accHolder}";

            if (!string.IsNullOrEmpty(symbol))
                sUrl += $"&symbol={symbol}";

            List<LogInfoItemDto> listKlines = new List<LogInfoItemDto>();

            byte[] tmpResult = ClsCommon.ExecuteHttpGetByteArray(sUrl);

            if (tmpResult != null && tmpResult.Length > 0 && ClsUtil.ByteArrayToStringUnzipIfNedeed(tmpResult, System.Text.Encoding.UTF8, out string response, out string msgErr) && !string.IsNullOrEmpty(response) && string.IsNullOrEmpty(msgErr))
                listKlines = JsonConvert.DeserializeObject<List<LogInfoItemDto>>(response);

            return new OkObjectResult(listKlines);
        }
    }
}
