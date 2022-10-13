using BlazorApp.Shared.CoreDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace BlazorApp.Api
{
    public class ClsCommon
    {
        public static string URL_SERVER = "https://logservice-app-20221012212833.redmushroom-aa658d30.northeurope.azurecontainerapps.io";
        //public static string URL_SERVER = "https://localhost:7132";

        public static List<LogInfoItemDto> GetLogs(string sMethod, string accType, string accHolder, string symbol = null)
        {
            List<LogInfoItemDto> listSymbol = new List<LogInfoItemDto>();

            try
            {
                string sUrl = $"{ClsCommon.URL_SERVER}/Server/{sMethod}?accType={accType}&accHolder={accHolder}";

                if (!string.IsNullOrEmpty(symbol))
                    sUrl += $"&symbol={symbol}";

                using (var httpClient = new HttpClient())
                {
                    listSymbol = httpClient.GetFromJsonAsync<List<LogInfoItemDto>>(sUrl).Result;
                }
            }
            catch (Exception e) { }

            return listSymbol;
        }
    }
}
