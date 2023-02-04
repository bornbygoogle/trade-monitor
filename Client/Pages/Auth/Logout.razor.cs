namespace BlazorApp.Client.Pages.Auth
{
    public partial class Logout : IDisposable
    {
        public void Dispose()
        {
            
        }

        protected override async Task OnInitializedAsync()
        {
            var authState = await ((ApiAuthenticationStateProvider)authenticationStateProvider).GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity != null)
            {
                if (user.Identity.IsAuthenticated)
                {
                    var sUrl = $"/api/Logout?userName={user.Identity.Name}";

                    var result = await Http.GetStringAsync(sUrl);

                    if (result == "logged out")
                    {
                        await localStorage.RemoveItemAsync($"accessToken");
                        await localStorage.RemoveItemAsync($"refreshToken");
                        ((ApiAuthenticationStateProvider)authenticationStateProvider).MarkUserAsLoggedOut();

                        NavigationManager.NavigateTo("/");
                    }
                }
            }
        }
    }
}
