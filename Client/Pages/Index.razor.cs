using BlazorApp.Shared;
using BlazorApp.Shared.CoreDto;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
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
        private string _accountSimulatedFirstTradeDate = null;

        private bool _statSevenDays = true;
        private bool _statThirtyDays = false;
        private bool _statAllTimes = false;



        private bool _tdPotential = true;
        private bool _tdCombo = false;
        private bool _tdCountDown = false;

        private List<DataItem> _nbrTrades = null;
        private List<DataItem> _durationAverageTrades = null;
        private decimal? _totalTrades = 0;
        private decimal? _percentageSucceededTrades = 0;

        private List<DataItem> _boughtSimulated = null;
        private List<DataItem> _soldSimulated = null;
        private List<DataItem> _profitSimulated = null;

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

            _boughtSimulated = null;
            _soldSimulated = null;
            _profitSimulated = null;
        }

        protected override async Task OnInitializedAsync()
        {
            _textCurrentAccount = $"Account {_selectedAccount}";

            try
            {
                _cancelToken = new CancellationTokenSource();

                if (_boughtSimulated == null)
                    _boughtSimulated = new List<DataItem>();
                else
                    _boughtSimulated?.Clear();

                if (_soldSimulated == null)
                    _soldSimulated = new List<DataItem>();
                else
                    _soldSimulated?.Clear();

                if (_profitSimulated == null)
                    _profitSimulated = new List<DataItem>();
                else
                    _profitSimulated?.Clear();

                if (_nbrTrades == null)
                    _nbrTrades = new List<DataItem>();
                else
                    _nbrTrades?.Clear();

                if (_durationAverageTrades == null)
                    _durationAverageTrades = new List<DataItem>();
                else
                    _durationAverageTrades?.Clear();

                _logsPotential?.Clear();

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
            _boughtSimulated?.Clear();
            _soldSimulated?.Clear();
            _profitSimulated?.Clear();

            List<DataItem> newListItemBought = null;
            List<DataItem> newListItemSold = null;
            List<DataItem> newListItemProfit = null;

            if (_statSevenDays)
            {
                newListItemBought = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosBought?accType=Spot&accHolder=An&nbrDays=7&real=0", _cancelToken.Token);
                newListItemSold = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosSold?accType=Spot&accHolder=An&nbrDays=7&real=0", _cancelToken.Token);
                newListItemProfit = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosProfit?accType=Spot&accHolder=An&nbrDays=7&real=0", _cancelToken.Token);

                _accountSimulatedFirstTradeDate = await Http.GetStringAsync($"/api/GetAccountInfosFirstTradeDate?accType=Spot&accHolder=An&nbrDays=7&real=0", _cancelToken.Token);
            }
            else if (_statThirtyDays)
            {
                newListItemBought = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosBought?accType=Spot&accHolder=An&nbrDays=30&real=0", _cancelToken.Token);
                newListItemSold = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosSold?accType=Spot&accHolder=An&nbrDays=30&real=0", _cancelToken.Token);
                newListItemProfit = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosProfit?accType=Spot&accHolder=An&nbrDays=30&real=0", _cancelToken.Token);

                _accountSimulatedFirstTradeDate = await Http.GetStringAsync($"/api/GetAccountInfosFirstTradeDate?accType=Spot&accHolder=An&nbrDays=30&real=0", _cancelToken.Token);
            }
            else
            {
                newListItemBought = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosBought?accType=Spot&accHolder=An&real=0", _cancelToken.Token);
                newListItemSold = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosSold?accType=Spot&accHolder=An&real=0", _cancelToken.Token);
                newListItemProfit = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosProfit?accType=Spot&accHolder=An&real=0", _cancelToken.Token);

                _accountSimulatedFirstTradeDate = await Http.GetStringAsync($"/api/GetAccountInfosFirstTradeDate?accType=Spot&accHolder=An&real=0", _cancelToken.Token);
            }

            _accountSimulatedFirstTradeDate = $"First trade at {_accountSimulatedFirstTradeDate}";

            if (newListItemBought != null && newListItemBought != null)
                _boughtSimulated.AddRange(newListItemBought);

            if (newListItemSold != null && newListItemSold != null)
                _soldSimulated.AddRange(newListItemSold);

            if (newListItemProfit != null && _profitSimulated != null)
                _profitSimulated.AddRange(newListItemProfit);

            if (executionCount % 5 == 0)
            {
                GestionCompletedTrades();
                GestionDurationAverageTrade();
            }

            if (_tdPotential)
                _logsPotential = await Http.GetFromJsonAsync<List<LogInfoItemDto>>($"/api/GetLogKlinePotential?accType=Spot&accHolder=An", _cancelToken.Token);
            else if (_tdCombo)
                _logsPotential = await Http.GetFromJsonAsync<List<LogInfoItemDto>>($"/api/GetLogKlineTDCombo?accType=Spot&accHolder=An", _cancelToken.Token);
            else if (_tdCountDown)
                _logsPotential = await Http.GetFromJsonAsync<List<LogInfoItemDto>>($"/api/GetLogKlineTDCountDown?accType=Spot&accHolder=An", _cancelToken.Token);
        }

        private async void GestionCompletedTrades()
        {
            _nbrTrades?.Clear();

            List<DataItem> listNbrTrades = null;

            if (_statSevenDays)
            {
                listNbrTrades = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosCompletedTrade?accType=Spot&accHolder=An&nbrDays=7&real=0", _cancelToken.Token);
            }
            else if (_statThirtyDays)
            {
                listNbrTrades = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosCompletedTrade?accType=Spot&accHolder=An&nbrDays=30&real=0", _cancelToken.Token);
            }
            else
            {
                listNbrTrades = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosCompletedTrade?accType=Spot&accHolder=An&real=0", _cancelToken.Token);
            }

            if (listNbrTrades != null && listNbrTrades.Count > 0) 
                _nbrTrades.AddRange(listNbrTrades);
        }

        private async void GestionDurationAverageTrade()
        {
            _durationAverageTrades?.Clear();

            List<DataItem> tmpListDataItem = null;

            if (_statSevenDays)
                tmpListDataItem = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosDurationAverageCompletedTrade?accType=Spot&accHolder=An&nbrDays=7&real=0", _cancelToken.Token);
            else if (_statThirtyDays)
                tmpListDataItem = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosDurationAverageCompletedTrade?accType=Spot&accHolder=An&nbrDays=30&real=0", _cancelToken.Token);
            else
                tmpListDataItem = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosDurationAverageCompletedTrade?accType=Spot&accHolder=An&real=0", _cancelToken.Token);

            if (tmpListDataItem != null)
                _durationAverageTrades = tmpListDataItem.ToList();
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
