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
    public static class GetListSymbolsWithBaseQuoteFunction
    {
        [FunctionName("GetListSymbolsWithBaseQuote")]
        public static IActionResult GetListSymbolsWithBaseQuote([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
                                                                ILogger log)
        {
            var accType = req.Query.Where(x => x.Key == "accType").FirstOrDefault().Value;
            var accHolder = req.Query.Where(x => x.Key == "accHolder").FirstOrDefault().Value;
            var symbol = req.Query.Where(x => x.Key == "symbol").FirstOrDefault().Value;


            string sUrl = $"{ClsCommon.GetUrlServer()}/Server/GetListSymbolsWithBaseQuote?accType={accType}&accHolder={accHolder}";

            if (!string.IsNullOrEmpty(symbol))
                sUrl += $"&symbol={symbol}";

            List<SymbolItemDto> listSymbol = new List<SymbolItemDto>();

            byte[] tmpResult = ClsCommon.ExecuteHttpGetByteArray(sUrl);

            if (tmpResult != null && tmpResult.Length > 0 && ClsUtil.ByteArrayToStringUnzipIfNedeed(tmpResult, System.Text.Encoding.UTF8, out string response, out string msgErr) && !string.IsNullOrEmpty(response) && string.IsNullOrEmpty(msgErr))
                listSymbol = JsonConvert.DeserializeObject<List<SymbolItemDto>>(response);

            return new OkObjectResult(listSymbol);
        }
    }
}
