using BlazorApp.Shared;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace BlazorApp.Client
{
    public class ApiAuthenticationStateProvider : AuthenticationStateProvider
    {
        //private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;

        public ApiAuthenticationStateProvider(HttpClient httpClient, ILocalStorageService localStorage)
        {
            //_httpClient = httpClient;
            _localStorage = localStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var sToken = await _localStorage.GetItemAsync<string>("accessToken");

                if (sToken == null)
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

                if (!string.IsNullOrEmpty(sToken) && !ClsCommon.CheckTokenIsNotExpired(sToken))
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

                //_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", sToken);

                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(ClsCommon.ParseClaimsFromJwt(sToken), "password")));
            }
            catch (Exception)
            {
                await _localStorage.RemoveItemAsync("accessToken");
            }
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        public void MarkUserAsAuthenticated(string username)
        {
            var newClaimName = new Claim(ClaimTypes.Name, username);
            var newClaimIdentity = new ClaimsIdentity(new[] { newClaimName }, "password");
            var authenticatedUser = new ClaimsPrincipal(newClaimIdentity);
            var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
            NotifyAuthenticationStateChanged(authState);
        }

        public void MarkUserAsLoggedOut()
        {
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymousUser));
            NotifyAuthenticationStateChanged(authState);
        }
    }
}
