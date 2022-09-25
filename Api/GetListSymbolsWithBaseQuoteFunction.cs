using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

using BlazorApp.Shared;
using BlazorApp.Shared.CoreDto;
using static System.Net.WebRequestMethods;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.AccessControl;

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

            var symbols = GetListSymbols(accType, accHolder, symbol);

            return new OkObjectResult(symbols);
        }

        public static List<SymbolItemDto> GetListSymbols(string accType, string accHolder, string symbol)
        {
            List<SymbolItemDto> listSymbol = new List<SymbolItemDto>();

            try
            {
                string sUrl = $"{ClsCommon.URL_SERVER}/Server/GetListSymbolsWithBaseQuote?accType={accType}&accHolder={accHolder}";

                if (!string.IsNullOrEmpty(symbol))
                    sUrl += $"&symbol={symbol}";

                using (var httpClient = new HttpClient())
                {
                    listSymbol = httpClient.GetFromJsonAsync<List<SymbolItemDto>>(sUrl).Result;
                }
            }
            catch (Exception e) { }

            return listSymbol;
        }

    }
}
