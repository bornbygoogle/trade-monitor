using BlazorApp.Api.Properties;
using BlazorApp.Shared;
using BlazorApp.Shared.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;

namespace BlazorApp.Api.Function
{
    public static class RefreshTokenFunction
    {
        [FunctionName("RefreshToken")]
        public static IActionResult RefreshToken([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log)
        {
            var userName = req.Query.Where(x => x.Key == "userName").FirstOrDefault().Value;
            var refreshToken = req.Query.Where(x => x.Key == "refreshToken").FirstOrDefault().Value;

            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(refreshToken))
                return new OkObjectResult("ERR|Missing credentials !");

            Token token = null;
            string msgError = string.Empty;

            User user = new User();
            user.UserName = userName;
            user.RefreshToken = refreshToken;
            user.ClientSecret = Resources.ApiSecret;
            user.GrantType = "refresh_token";

            var response = ClsCommon.ExecuteHttpPost<ResponseData>("http://cache-service.minhan-tran.fr/Auth/Gestion", user);
            if (response != null)
            {
                if (response.Code == "900")
                {
                    token = JsonConvert.DeserializeObject<Token>(JsonConvert.SerializeObject(response.Data));
                    msgError = string.Empty;

                    //return new OkObjectResult($"{ClsCommon.Encrypt(token.AccessToken)}||REF||{ClsCommon.Encrypt(token.RefreshToken)}");
                    return new OkObjectResult($"{token.AccessToken}||REF||{token.RefreshToken}");
                }
                else
                {
                    token = null;
                    return new OkObjectResult($"ERR|{response.Message}");
                }
            }
            else
                return new BadRequestObjectResult(string.Empty);
        }
    }
}
