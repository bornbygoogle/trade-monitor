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
    public static class LogsFunction
    {
        private static string URL_SERVER = "https://cacheserver-app.victoriousocean-9c8156ce.northeurope.azurecontainerapps.io";

        private static string GetSummary(int temp)
        {
            var summary = "Mild";

            if (temp >= 32)
            {
                summary = "Hot";
            }
            else if (temp <= 16 && temp > 0)
            {
                summary = "Cold";
            }
            else if (temp <= 0)
            {
                summary = "Freezing!";
            }

            return summary;
        }

        [FunctionName("GetListSymbolsWithBaseQuote")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var accType = req.Query.Where(x => x.Key == "accType").FirstOrDefault().Value;
            var accHolder = req.Query.Where(x => x.Key == "accHolder").FirstOrDefault().Value;

            var symbols = GetListSymbols(accType, accHolder);

            //var randomNumber = new Random();
            //var temp = 0;

            //var result = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            //{
            //    Date = DateTime.Now.AddDays(index),
            //    TemperatureC = temp = randomNumber.Next(-20, 55),
            //    Summary = GetSummary(temp)
            //}).ToArray();

            return new OkObjectResult(symbols);
        }

        public static List<SymbolItemDto> GetListSymbols(string accType, string accHolder)
        {
            List<SymbolItemDto> listSymbol = new List<SymbolItemDto>();

            try
            {
                using (var httpClient = new HttpClient())
                {
                    listSymbol = httpClient.GetFromJsonAsync<List<SymbolItemDto>>($"{URL_SERVER}/Logs/GetListSymbolsWithBaseQuote?accType={accType}&accHolder={accHolder}").Result;
                }
            }
            catch (Exception e) { }

            return listSymbol;
        }
    }
}
