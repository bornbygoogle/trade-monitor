using BlazorApp.Shared.CoreDto;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;

namespace BlazorApp.Api
{
    public class ClsCommon
    {
        public static string URL_SERVER = "https://logservice-app-20221111153833.thankfuldesert-bb5e8557.northeurope.azurecontainerapps.io";
        public static string URL_SERVER_2 = "https://logservice-app-20221012212833.redmushroom-aa658d30.northeurope.azurecontainerapps.io";
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
