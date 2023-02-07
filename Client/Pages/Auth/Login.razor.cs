using BlazorApp.Shared.Auth;
using System.Globalization;

namespace BlazorApp.Client.Pages.Auth
{
    public partial class Login : IDisposable
    {
        private User user = null;
        private bool ShowErrors;
        private string Error = "";

        protected override async Task OnInitializedAsync()
        {
            var authState = await ((ApiAuthenticationStateProvider)authenticationStateProvider).GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity != null && user.Identity.IsAuthenticated)
                NavigationManager.NavigateTo("/gestion");
        }

        private async void HandleLogin()
        {
            ShowErrors = false;

            var result = await Http.GetStringAsync($"/api/Login?userName={user.UserName}&password={user.Password}");

            if (result.StartsWith("ERR|"))
            {
                Error = result.Replace("ERR|", string.Empty);
                ShowErrors = true;

                // Update the UI
                StateHasChanged();
            }
            else
            {
                var res = result.Split("||REF||");
                if (res != null && res.Count() == 2)
                {
                    await localStorage.SetItemAsync($"accountName", CultureInfo.CurrentCulture.TextInfo.ToTitleCase(user.UserName) );
                    await localStorage.SetItemAsync($"accessToken", res[0]);
                    await localStorage.SetItemAsync($"refreshToken", res[1]);

                    ((ApiAuthenticationStateProvider)authenticationStateProvider).MarkUserAsAuthenticated(user.UserName);

                    //redirect
                    NavigationManager.NavigateTo($"/{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(user.UserName)}", true);
                }
            }
        }


        public void Dispose()
        {

        }
    }
}
