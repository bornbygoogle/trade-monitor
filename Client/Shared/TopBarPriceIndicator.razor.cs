using BlazorApp.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using System.Net.Http.Json;
using System.Timers;

namespace BlazorApp.Client.Shared
{
    public partial class TopBarPriceIndicator : IDisposable
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
        private string _priceBNB = "BNB : 300";
        private string _priceETH = "ETH : 1600";

        private static System.Timers.Timer refreshTimer;

        private CancellationTokenSource _cancelToken = null;

        public void Dispose()
        {
            if (refreshTimer != null)
            {
                refreshTimer.Stop();
                refreshTimer.Elapsed -= RefreshTimer;
                refreshTimer.Dispose();

                refreshTimer = null;
            }

            if (_cancelToken == null)
            {
                _cancelToken?.Cancel();
                _cancelToken?.Dispose();
                _cancelToken = null;
            }
        }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                if (refreshTimer == null)
                {
                    refreshTimer = new System.Timers.Timer(TimeSpan.FromSeconds(ClsUtilCommon.TIMER_DURATION).TotalMilliseconds);
                    refreshTimer.Elapsed += RefreshTimer;
                    refreshTimer.Enabled = true;
                }

                _cancelToken = new CancellationTokenSource();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public async void RefreshTimer(Object source, ElapsedEventArgs e)
        {
            if (_cancelToken.Token.IsCancellationRequested)
                return;

            bool hasNewResult = false;

            decimal newPriceBTC;
            decimal newPriceBNB;
            decimal newPriceETH;

            try
            {
                refreshTimer.Stop();

                newPriceBTC = await Http.GetFromJsonAsync<decimal>($"/api/GetLatestClose?accType=Spot&symbol=BTCUSDT", _cancelToken.Token);
                if (newPriceBTC > 0)
                    _priceBTC = $"BTC : {Math.Round(newPriceBTC, 2)}";

                newPriceBNB = await Http.GetFromJsonAsync<decimal>($"/api/GetLatestClose?accType=Spot&symbol=BNBUSDT", _cancelToken.Token);
                if (newPriceBNB > 0)
                    _priceBNB = $"BNB : {Math.Round(newPriceBNB, 2)}";

                newPriceETH = await Http.GetFromJsonAsync<decimal>($"/api/GetLatestClose?accType=Spot&symbol=ETHUSDT", _cancelToken.Token);
                if (newPriceETH > 0)
                    _priceETH = $"ETH : {Math.Round(newPriceETH, 2)}";

                hasNewResult = true;
            }
            catch
            {
                hasNewResult = false;
            }
            finally
            {
                if (hasNewResult)
                    InvokeAsync(StateHasChanged);

                refreshTimer.Start();
            }
        }

    }
}
