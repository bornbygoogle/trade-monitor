using BlazorApp.Shared;
using BlazorApp.Shared.CoreDto;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Radzen;
using System.Globalization;
using System.Net.Http.Json;
using System.Timers;

namespace BlazorApp.Client.Pages
{
    public partial class Index : IDisposable
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

        private string _selectedAccount = "An";

        private CancellationTokenSource _cancelToken = null;

        private static System.Timers.Timer refreshTimer;
        private static int executionCount = 0;

        private string _textCurrentAccount = string.Empty;
        private string _stringAccount = null;
        private AccountDto _account = null;

        private decimal? _accountSimulatedTotalTrades = null;
        private decimal? _accountSimulatedTotalPositiveTrades = null;

        private bool _statSevenDays = true;
        private bool _statThirtyDays = false;
        private bool _statAllTimes = false;



        private bool _tdPotential = true;
        private bool _tdCombo = false;
        private bool _tdCountDown = false;

        private List<DataItem> _nbrTrades = new List<DataItem>();
        private List<DataItem> _durationAverageTrades = new List<DataItem>();
        private decimal? _totalTrades = 0;
        private decimal? _percentageSucceededTrades = 0;
        private bool _realPercentage = false;

        private List<string> _listSymbols = new List<string>() { "USDT", "BTC", "ETH" };

        private List<DataItem> _profitSimulated = new List<DataItem>();
        private List<DataItem> _profitReal = null;

        private List<LogInfoItemDto> _logsPotential = null;

        private bool panelPotentialCollapsed = true;

        bool showDataLabels = true;

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

            _profitSimulated = null;
        }

        protected override async Task OnInitializedAsync()
        {
            _textCurrentAccount = $"Account {_selectedAccount}";

            try
            {
                if (_cancelToken == null)
                    _cancelToken = new CancellationTokenSource();

                if (_profitSimulated == null)
                    _profitSimulated = new List<DataItem>();

                if (refreshTimer == null)
                {
                    refreshTimer = new System.Timers.Timer(TimeSpan.FromSeconds(ClsUtilCommon.TIMER_DURATION).TotalMilliseconds);
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
            if (_cancelToken.Token.IsCancellationRequested)
                return;

            bool hasNewResult = false;

            try
            {
                refreshTimer.Stop();

                GestionTimer();

                hasNewResult = true;

                if (executionCount == 60)
                    Interlocked.Exchange(ref executionCount, 0);
            }
            catch
            {
                hasNewResult = false;
            }
            finally
            {
                if (hasNewResult)
                    InvokeAsync(StateHasChanged);

                Interlocked.Increment(ref executionCount);

                refreshTimer.Start();
            }
        }

        private async void GestionTimer()
        {
            List<DataItem> newListItem = null;

            if (_statSevenDays)
                newListItem = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosProfit?accType=Spot&accHolder=An&nbrDays=7&real=0", _cancelToken.Token);
            else if (_statThirtyDays)
                newListItem = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosProfit?accType=Spot&accHolder=An&nbrDays=30&real=0", _cancelToken.Token);
            else
                newListItem = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosProfit?accType=Spot&accHolder=An&real=0", _cancelToken.Token);

            if (newListItem != null && _profitSimulated != null)
            {
                _profitSimulated.Clear();
                _profitSimulated.AddRange(newListItem);
            }

            if (executionCount % 5 == 0)
            {
                GestionPercentageTrade();
                GestionDurationAverageTrade();
            }

            if (_tdPotential)
                _logsPotential = await Http.GetFromJsonAsync<List<LogInfoItemDto>>($"/api/GetLogKlinePotential?accType=Spot&accHolder=An", _cancelToken.Token);
            else if (_tdCombo)
                _logsPotential = await Http.GetFromJsonAsync<List<LogInfoItemDto>>($"/api/GetLogKlineTDCombo?accType=Spot&accHolder=An", _cancelToken.Token);
            else if (_tdCountDown)
                _logsPotential = await Http.GetFromJsonAsync<List<LogInfoItemDto>>($"/api/GetLogKlineTDCountDown?accType=Spot&accHolder=An", _cancelToken.Token);
        }

        private async void GestionPercentageTrade()
        {
            _nbrTrades.Clear();

            _accountSimulatedTotalTrades = await Http.GetFromJsonAsync<decimal>($"/api/GetAccountInfosTotalTrades?accType=Spot&accHolder=An&real=0", _cancelToken.Token);
            _accountSimulatedTotalPositiveTrades = await Http.GetFromJsonAsync<decimal>($"/api/GetAccountInfosTotalPositiveTrades?accType=Spot&accHolder=An&real=0", _cancelToken.Token);

            if (_accountSimulatedTotalPositiveTrades.HasValue && _accountSimulatedTotalTrades.HasValue)
            {
                DataItem nbrTradesSimulatedSucceeded = new DataItem();
                nbrTradesSimulatedSucceeded.Base = "Succeeded trade";
                nbrTradesSimulatedSucceeded.Profit = (double)(_accountSimulatedTotalPositiveTrades ?? 0);

                _nbrTrades.Add(nbrTradesSimulatedSucceeded);

                DataItem nbrTradesSimulatedNotSucceeded = new DataItem();
                nbrTradesSimulatedNotSucceeded.Base = "Failed trade";
                nbrTradesSimulatedNotSucceeded.Profit = (double)((_accountSimulatedTotalTrades - _accountSimulatedTotalPositiveTrades) ?? 0);

                _totalTrades = _accountSimulatedTotalTrades ?? 0;
                _percentageSucceededTrades = Math.Round(((_accountSimulatedTotalPositiveTrades ?? 0) * 100) / (_accountSimulatedTotalTrades ?? 1), 2);

                _nbrTrades.Add(nbrTradesSimulatedNotSucceeded);
            }
        }

        private async void GestionDurationAverageTrade()
        {
            List<DataItem> tmpListDataItem = null;

            if (_statSevenDays)
                tmpListDataItem = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosDurationAverageCompletedTrade?accType=Spot&accHolder=An&nbrDays=7&real={(_realPercentage ? "1" : "0")}", _cancelToken.Token);
            else if (_statThirtyDays)
                tmpListDataItem = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosDurationAverageCompletedTrade?accType=Spot&accHolder=An&nbrDays=30&real={(_realPercentage ? "1" : "0")}", _cancelToken.Token);
            else
                tmpListDataItem = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosDurationAverageCompletedTrade?accType=Spot&accHolder=An&real={(_realPercentage ? "1" : "0")}", _cancelToken.Token);

            if (tmpListDataItem != null)
            {
                _durationAverageTrades.Clear();
                _durationAverageTrades = tmpListDataItem.ToList();
            }
        }

        protected void ButtonStatSevenDaysClick(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            _statSevenDays = true;
            _statThirtyDays = false;
            _statAllTimes = false;
        }

        protected void ButtonStatThirtyDaysClick(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            _statSevenDays = false;
            _statThirtyDays = true;
            _statAllTimes = false;
        }

        protected void ButtonStatAllTimesClick(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            _statSevenDays = false;
            _statThirtyDays = false;
            _statAllTimes = true;
        }

        protected void ButtonTDPotentialClick(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            _tdPotential = true;
            _tdCombo = false;
            _tdCountDown = false;
        }

        protected void ButtonTDComboClick(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            _tdPotential = false;
            _tdCombo = true;
            _tdCountDown = false;
        }

        protected void ButtonTDCountDownClick(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            _tdPotential = false;
            _tdCombo = false;
            _tdCountDown = true;
        }

    }
}
