using BlazorApp.Shared;
using BlazorApp.Shared.CoreDto;
using CryptoExchange.Net.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
        public static string URL_DEV = "https://localhost:7132";
        public const bool USE_LOCAL_SERVER = false;

        private static HttpClient _httpClient;

        public static List<LogInfoItemDto> GetLogs(string sMethod, string accType, string accHolder, string symbol = null)
        {
            List<LogInfoItemDto> listSymbol = new List<LogInfoItemDto>();

            string sUrl = $"{ClsCommon.GetUrlServer()}/Server/{sMethod}?accType={accType}&accHolder={accHolder}";

            if (!string.IsNullOrEmpty(symbol))
                sUrl += $"&symbol={symbol}";

            byte[] tmpResult = ClsCommon.ExecuteHttpGetByteArray(sUrl);

            if (tmpResult != null && tmpResult.Length > 0 && ClsUtil.ByteArrayToStringUnzipIfNedeed(tmpResult, System.Text.Encoding.UTF8, out string response, out string msgErr) && !string.IsNullOrEmpty(response) && string.IsNullOrEmpty(msgErr))
                listSymbol = JsonConvert.DeserializeObject<List<LogInfoItemDto>>(response);

            return listSymbol;
        }

        #region Http

        public static string GetUrlServer()
        {
            if (USE_LOCAL_SERVER)
                return URL_DEV.EndsWith("/") ? URL_DEV.TrimEnd('/') : URL_DEV;
            else
                return URL_SERVER.EndsWith("/") ? URL_SERVER.TrimEnd('/') : URL_SERVER;
        }

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

        public static byte[] ExecuteHttpGetByteArray(string sUrl)
        {
            var nbr = 0;
            byte[] result = null;

            try
            {
                do
                {
                    try
                    {
                        var res = GetHttpClient().GetAsync(sUrl).Result;

                        res.EnsureSuccessStatusCode();

                        nbr = 11;

                        result = res.Content.ReadAsAsync<byte[]>().Result;
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
