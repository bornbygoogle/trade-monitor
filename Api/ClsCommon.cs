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
        public static string URL_SERVER = "https://cacheservice-app.mangocoast-e15fdab0.northeurope.azurecontainerapps.io";

        public static List<LogInfoItemDto> GetLogs(string sMethod, string accType, string accHolder, string symbolName = null)
        {
            List<LogInfoItemDto> listSymbol = new List<LogInfoItemDto>();

            try
            {
                string sUrl = $"{ClsCommon.URL_SERVER}/Server/{sMethod}?accType={accType}&accHolder={accHolder}";

                if (!string.IsNullOrEmpty(symbolName))
                    sUrl += $"&symbolName={symbolName}";

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
