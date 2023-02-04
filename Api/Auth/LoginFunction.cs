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

namespace BlazorApp.Api.Auth
{
    public static class LoginFunction
    {
        [FunctionName("Login")]
        public static IActionResult Login([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log)
        {
            var userName = req.Query.Where(x => x.Key == "userName").FirstOrDefault().Value;
            var password = req.Query.Where(x => x.Key == "password").FirstOrDefault().Value;

            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                return new OkObjectResult("ERR|Missing credentials !");

            Token token = null;
            string msgError = string.Empty;

            User user = new User();
            user.UserName = userName;
            user.Password = password;
            user.ClientSecret = Resources.ApiSecret;
            user.GrantType = "password";

            var response = ClsCommon.ExecuteHttpPost<ResponseData>($"{ClsCommon.GetUrlServer()}/Auth/Gestion", user);
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
