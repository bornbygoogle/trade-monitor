using BlazorApp.Api.Properties;
using BlazorApp.Shared;
using BlazorApp.Shared.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace BlazorApp.Api.Function
{
    public static class LogoutFunction
    {
        [FunctionName("Logout")]
        public static IActionResult Logout([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log)
        {
            var userName = req.Query.Where(x => x.Key == "userName").FirstOrDefault().Value;

            if (string.IsNullOrEmpty(userName))
                return new BadRequestObjectResult(string.Empty);

            Token token = null;
            string msgError = string.Empty;

            User user = new User();
            user.UserName = userName;
            user.ClientSecret = Resources.ApiSecret;
            user.GrantType = "log_out";

            var response = ClsCommon.ExecuteHttpPost<ResponseData>("http://cache-service.minhan-tran.fr/Auth/Gestion", user);
            if (response != null)
                return new OkObjectResult(response.Message);
            else
                return new BadRequestObjectResult(string.Empty);
        }
    }
}
