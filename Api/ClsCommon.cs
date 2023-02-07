using BlazorApp.Api.Function;
using BlazorApp.Shared;
using BlazorApp.Shared.CoreDto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        private static HttpClient GetHttpClient(string accessToken = null)
        {
            if (_httpClient == null)
            {
                _httpClient = new HttpClient();

                _httpClient.Timeout = TimeSpan.FromSeconds(5);
            }

            _httpClient.DefaultRequestHeaders.Authorization = null;

            if (!string.IsNullOrEmpty(accessToken))
                _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

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

                        Thread.Sleep(1000);
                    }

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

        public static T ExecuteHttpPost<T>(string sUrl, object oObject, string accessToken = null)
        {
            var nbr = 0;
            T result = default(T);

            try
            {
                do
                {
                    try
                    {
                        var httpClient = GetHttpClient(accessToken);

                        var res = httpClient.PostAsync(sUrl, new StringContent(Serialize(oObject), Encoding.UTF8, "application/json")).Result;

                        res.EnsureSuccessStatusCode();

                        nbr = 11;

                        var resultString = res.Content.ReadAsStringAsync().Result;

                        if (!string.IsNullOrEmpty(resultString))
                            result = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(resultString);
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

        public static string ExecuteHttpPost(string sUrl, object oObject, string accessToken = null)
        {
            var nbr = 0;
            string result = string.Empty;


            try
            {
                do
                {
                    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

                    try
                    {
                        var httpClient = GetHttpClient(accessToken);

                        result = PostStreamAsync(httpClient, sUrl, oObject, cancellationTokenSource).Result;

                        nbr = 11;
                    }
                    catch (Exception e)
                    {
                        cancellationTokenSource.Cancel();

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

        private static async Task<string> PostStreamAsync(HttpClient client, string url, object content, CancellationTokenSource cancellationTokenSource)
        {
            string result = string.Empty;

            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            using (var httpContent = CreateHttpContent(content))
            {
                request.Content = httpContent;

                var response = client.SendAsync(request, cancellationTokenSource.Token).Result;

                response.EnsureSuccessStatusCode();

                result = response.Content.ReadAsStringAsync().Result;
            }

            return result;
        }

        private static HttpContent CreateHttpContent(object content)
        {
            HttpContent httpContent = null;

            if (content != null)
            {
                var ms = new MemoryStream();
                SerializeJsonIntoStream(content, ms);
                ms.Seek(0, SeekOrigin.Begin);
                httpContent = new StreamContent(ms);

                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                //httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                //httpContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
            }

            return httpContent;
        }

        public static void SerializeJsonIntoStream(object value, Stream stream)
        {
            if (value != null)
            {
                var sData = JsonConvert.SerializeObject(value);

                sData = sData.ToBrotliAsync(System.IO.Compression.CompressionLevel.Optimal).Result.Result.Value;

                ClsUtil.StringToByteArrayZip(sData, System.Text.Encoding.UTF8, out byte[] bData, out string msgErr);

                if (bData != null && bData.Length > 0 && string.IsNullOrEmpty(msgErr))
                {
                    var sContent = Convert.ToBase64String(bData);

                    Console.WriteLine(sContent.Substring(0, 5));

                    using (var sw = new StreamWriter(stream, new UTF8Encoding(false), 1024, true))
                    using (var jtw = new JsonTextWriter(sw) { Formatting = Formatting.None })
                    {
                        var js = new JsonSerializer();
                        js.Serialize(jtw, sContent);
                        jtw.Flush();
                    }
                }
            }
        }

        public static string Serialize(object oObject, bool ignoreNullOrDefault = true)
        {
            if (ignoreNullOrDefault)
                return Newtonsoft.Json.JsonConvert.SerializeObject(oObject, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            else
                return Newtonsoft.Json.JsonConvert.SerializeObject(oObject);
        }


        #endregion Http
    }
}
