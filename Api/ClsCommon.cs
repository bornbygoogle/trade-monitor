using BlazorApp.Shared.CoreDto;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Threading;

namespace BlazorApp.Api
{
    public class ClsCommon
    {
        public static string URL_SERVER = "http://cache-service.minhan-tran.fr";
        //public static string URL_SERVER_2 = "https://logservice-app-20221012212833.redmushroom-aa658d30.northeurope.azurecontainerapps.io";
        //public static string URL_SERVER = "https://localhost:7132";

        private static HttpClient _httpClient;

        public static List<LogInfoItemDto> GetLogs(string sMethod, string accType, string accHolder, string symbol = null)
        {
            List<LogInfoItemDto> listSymbol = new List<LogInfoItemDto>();

            string sUrl = $"{ClsCommon.URL_SERVER}/Server/{sMethod}?accType={accType}&accHolder={accHolder}";

            if (!string.IsNullOrEmpty(symbol))
                sUrl += $"&symbol={symbol}";

            listSymbol = ExecuteHttpGet<List<LogInfoItemDto>>(sUrl);

            return listSymbol;
        }

        #region Http

        private static HttpClient GetHttpClient()
        {
            if (_httpClient == null)
            {
                _httpClient = new HttpClient();

                _httpClient.Timeout = TimeSpan.FromSeconds(5);
            }

            return _httpClient;
        }

        public static string ExecuteHttpGet(string sUrl)
        {
            var nbr = 0;
            var result = string.Empty;

            try
            {
                do
                {
                    try
                    {
                        var res = GetHttpClient().GetAsync(sUrl).Result;

                        res.EnsureSuccessStatusCode();

                        nbr = 11;

                        result = res.Content.ReadAsStringAsync().Result;
                    }
                    catch (Exception e)
                    {
                        if (nbr >= 10)
                            throw;
                    }

                    Thread.Sleep(1000);

                    nbr++;
                }
                while (nbr < 10);
            }
            catch 
            {
                //var res = GetHttpClient().GetAsync("https://black-bay-0e87ebf03.1.azurestaticapps.net/").Result; 
            }            

            return result;
        }

        public static T ExecuteHttpGet<T>(string sUrl)
        {
            var nbr = 0;
            T result = default(T);

            try
            {
                do
                {
                    try
                    {
                        var res = GetHttpClient().GetAsync(sUrl).Result;

                        res.EnsureSuccessStatusCode();

                        nbr = 11;

                        result = res.Content.ReadFromJsonAsync<T>().Result;
                    }
                    catch (Exception e)
                    {
                        if (nbr >= 10)
                            throw;
                    }

                    Thread.Sleep(1000);

                    nbr++;
                }
                while (nbr < 10);
            }
            catch 
            { 
                /*var res = GetHttpClient().GetAsync("https://black-bay-0e87ebf03.1.azurestaticapps.net/").Result;*/ 
            }          

            return result;
        }

        #endregion Http
    }
}
