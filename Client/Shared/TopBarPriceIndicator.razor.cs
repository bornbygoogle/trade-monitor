using BlazorApp.Shared.CoreDto;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using System.Net.Http.Json;
using System.Timers;
using static System.Net.WebRequestMethods;

namespace BlazorApp.Client.Shared
{
    public partial class TopBarPriceIndicator
    {
        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        protected TooltipService TooltipService { get; set; }

        [Inject]
        protected ContextMenuService ContextMenuService { get; set; }

        [Inject]
        protected NotificationService NotificationService { get; set; }

        [Inject]
        protected HttpClient Http { get; set; }

        private string _priceBTC = "BTC : 16000";
        private string _priceBNB = "BTC : 300";
        private string _priceETH = "BTC : 1600";

        private static System.Timers.Timer refreshTimer;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                if (refreshTimer == null)
                {
                    refreshTimer = new System.Timers.Timer(5000);
                    refreshTimer.Elapsed += RefreshTimer;
                    refreshTimer.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public async void RefreshTimer(Object source, ElapsedEventArgs e)
        {
            bool hasNewResult = false;

            decimal newPriceBTC;
            decimal newPriceBNB;
            decimal newPriceETH;

            try
            {
                newPriceBTC = await Http.GetFromJsonAsync<decimal>($"/api/GetLatestClose?accType=Spot&symbol=BTCUSDT");
                if (newPriceBTC > 0)
                    _priceBTC = $"BTC : {Math.Round(newPriceBTC, 2)}";
                
                newPriceBNB = await Http.GetFromJsonAsync<decimal>($"/api/GetLatestClose?accType=Spot&symbol=BNBUSDT");
                if (newPriceBNB > 0)
                    _priceBNB = $"BNB : {Math.Round(newPriceBNB, 2)}";

                newPriceETH = await Http.GetFromJsonAsync<decimal>($"/api/GetLatestClose?accType=Spot&symbol=ETHUSDT");
                if (newPriceETH > 0)
                    _priceETH = $"ETH : {Math.Round(newPriceETH, 2)}";

                hasNewResult = true;
            }
            catch
            {
                hasNewResult = false;
            }

            if (hasNewResult)
                _ = InvokeAsync(StateHasChanged);
        }

    }
}
