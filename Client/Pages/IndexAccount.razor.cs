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
    public partial class IndexAccount : IDisposable
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

        [Parameter] public string selectedAccount { get; set; }

        private static bool _onWork = false;

        private CancellationTokenSource _cancelToken = null;

        private static System.Timers.Timer refreshTimer;
        private static int executionCount = 0;

        private string _stringAccount = null;
        private AccountDto _account = null;

        private decimal? _accountRealTotalTrades = null;
        private decimal? _accountRealTotalPositiveTrades = null;
        private string _accountRealFirstTradeDate = null;

        private bool _statSevenDays = true;
        private bool _statThirtyDays = false;
        private bool _statAllTimes = false;

        private List<DataItem> _nbrTrades = null;
        private List<DataItem> _durationAverageTrades = null;
        private decimal? _totalTrades = 0;
        private decimal? _percentageSucceededTrades = 0;

        private List<DataItem> _boughtReal = null;
        private List<DataItem> _soldReal = null;
        private List<DataItem> _profitReal = null;

        private List<LogInfoItemDto> _logsBoughtSold = null;

        private string _labelLogsBoughtSoldHistory = null;
        private string _labelLogsBoughtSoldPositive = null;
        private string _labelLogsBoughtSoldNegative = null;

        private List<LogInfoItemDto> _logsBoughtSoldHistory = null;
        private List<LogInfoItemDto> _logsBoughtSoldPositive = null;
        private List<LogInfoItemDto> _logsBoughtSoldNegative = null;

        private DateTime? _logsClicked = null;

        private bool panelBoughtSoldPositiveCollapsed = false;
        private bool panelBoughtSoldNegativeCollapsed = true;
        private bool panelBoughtSoldHistoryCollapsed = true;


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
        }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                _cancelToken = new CancellationTokenSource();

                if (_boughtReal == null)
                    _boughtReal = new List<DataItem>();
                else
                    _boughtReal?.Clear();

                if (_soldReal == null)
                    _soldReal = new List<DataItem>();
                else
                    _soldReal?.Clear();

                if (_profitReal == null)
                    _profitReal = new List<DataItem>();
                else
                    _profitReal?.Clear();

                if (_nbrTrades == null)
                    _nbrTrades = new List<DataItem>();
                else
                    _nbrTrades?.Clear();

                if (_durationAverageTrades == null)
                    _durationAverageTrades = new List<DataItem>();
                else
                    _durationAverageTrades?.Clear();

                _logsBoughtSold?.Clear();
                _logsBoughtSoldHistory?.Clear();
                _logsBoughtSoldNegative?.Clear();
                _logsBoughtSoldPositive?.Clear();

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

        public void RefreshTimer(Object source, ElapsedEventArgs e)
        {
            if (_cancelToken.Token.IsCancellationRequested)
                return;

            bool hasNewResult = false;

            try
            {
                refreshTimer.Stop();

                GestionTimer();

                if (executionCount == 60)
                    Interlocked.Exchange(ref executionCount, 0);

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

                Interlocked.Increment(ref executionCount);

                refreshTimer.Start();
            }
        }

        private async void GestionTimer()
        {
            if (executionCount % 5 == 0)
            {
                GestionCompletedTrades();
                GestionDurationAverageTrade();

                _boughtReal?.Clear();
                _soldReal?.Clear();
                _profitReal?.Clear();

                List<DataItem> newListItemBought = null;
                List<DataItem> newListItemSold = null;
                List<DataItem> newListItemProfit = null;

                if (_statSevenDays)
                {
                    newListItemBought = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosBought?accType=Spot&accHolder={CultureInfo.CurrentCulture.TextInfo.ToTitleCase(selectedAccount)}&nbrDays=7&real=1", _cancelToken.Token);
                    newListItemSold = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosSold?accType=Spot&accHolder={CultureInfo.CurrentCulture.TextInfo.ToTitleCase(selectedAccount)}&nbrDays=7&real=1", _cancelToken.Token);
                    newListItemProfit = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosProfit?accType=Spot&accHolder={CultureInfo.CurrentCulture.TextInfo.ToTitleCase(selectedAccount)}&nbrDays=7&real=1", _cancelToken.Token);

                    _accountRealFirstTradeDate = await Http.GetStringAsync($"/api/GetAccountInfosFirstTradeDate?accType=Spot&accHolder={CultureInfo.CurrentCulture.TextInfo.ToTitleCase(selectedAccount)}&nbrDays=7&real=1", _cancelToken.Token);
                }
                else if (_statThirtyDays)
                {
                    newListItemBought = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosBought?accType=Spot&accHolder={CultureInfo.CurrentCulture.TextInfo.ToTitleCase(selectedAccount)}&nbrDays=30&real=1", _cancelToken.Token);
                    newListItemSold = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosSold?accType=Spot&accHolder={CultureInfo.CurrentCulture.TextInfo.ToTitleCase(selectedAccount)}&nbrDays=30&real=1", _cancelToken.Token);
                    newListItemProfit = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosProfit?accType=Spot&accHolder={CultureInfo.CurrentCulture.TextInfo.ToTitleCase(selectedAccount)}&nbrDays=30&real=1", _cancelToken.Token);

                    _accountRealFirstTradeDate = await Http.GetStringAsync($"/api/GetAccountInfosFirstTradeDate?accType=Spot&accHolder={CultureInfo.CurrentCulture.TextInfo.ToTitleCase(selectedAccount)}&nbrDays=30&real=1", _cancelToken.Token);
                }
                else
                {
                    newListItemBought = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosBought?accType=Spot&accHolder={CultureInfo.CurrentCulture.TextInfo.ToTitleCase(selectedAccount)}&real=1", _cancelToken.Token);
                    newListItemSold = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosSold?accType=Spot&accHolder={CultureInfo.CurrentCulture.TextInfo.ToTitleCase(selectedAccount)}&real=1", _cancelToken.Token);
                    newListItemProfit = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosProfit?accType=Spot&accHolder={CultureInfo.CurrentCulture.TextInfo.ToTitleCase(selectedAccount)}&real=1", _cancelToken.Token);

                    _accountRealFirstTradeDate = await Http.GetStringAsync($"/api/GetAccountInfosFirstTradeDate?accType=Spot&accHolder={CultureInfo.CurrentCulture.TextInfo.ToTitleCase(selectedAccount)}&real=1", _cancelToken.Token);
                }

                _accountRealFirstTradeDate = $"First trade at {_accountRealFirstTradeDate}";

                if (newListItemBought != null && newListItemBought != null)
                    _boughtReal.AddRange(newListItemBought);

                if (newListItemSold != null && newListItemSold != null)
                    _soldReal.AddRange(newListItemSold);

                if (newListItemProfit != null && newListItemProfit != null)
                    _profitReal.AddRange(newListItemProfit);
            }

            GestionLogsBoughtSold();
        }

        private async void GestionCompletedTrades()
        {
            _nbrTrades?.Clear();

            List<DataItem> listNbrTrades = null;

            if (_statSevenDays)
            {
                listNbrTrades = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosCompletedTrade?accType=Spot&accHolder={CultureInfo.CurrentCulture.TextInfo.ToTitleCase(selectedAccount)}&nbrDays=7&real=1", _cancelToken.Token);
            }
            else if (_statThirtyDays)
            {
                listNbrTrades = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosCompletedTrade?accType=Spot&accHolder={CultureInfo.CurrentCulture.TextInfo.ToTitleCase(selectedAccount)}&nbrDays=30&real=1", _cancelToken.Token);
            }
            else
            {
                listNbrTrades = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosCompletedTrade?accType=Spot&accHolder={CultureInfo.CurrentCulture.TextInfo.ToTitleCase(selectedAccount)}&real=1", _cancelToken.Token);
            }

            if (listNbrTrades != null && listNbrTrades.Count > 0)
                _nbrTrades.AddRange(listNbrTrades);
        }

        private async void GestionDurationAverageTrade()
        {
            _durationAverageTrades?.Clear();

            List<DataItem> tmpListDataItem = null;

            if (_statSevenDays)
                tmpListDataItem = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosDurationAverageCompletedTrade?accType=Spot&accHolder={CultureInfo.CurrentCulture.TextInfo.ToTitleCase(selectedAccount)}&nbrDays=7&real=1", _cancelToken.Token);
            else if (_statThirtyDays)
                tmpListDataItem = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosDurationAverageCompletedTrade?accType=Spot&accHolder={CultureInfo.CurrentCulture.TextInfo.ToTitleCase(selectedAccount)}&nbrDays=30&real=1", _cancelToken.Token);
            else
                tmpListDataItem = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosDurationAverageCompletedTrade?accType=Spot&accHolder={CultureInfo.CurrentCulture.TextInfo.ToTitleCase(selectedAccount)}&real=1", _cancelToken.Token);

            if (tmpListDataItem != null)
                _durationAverageTrades = tmpListDataItem.ToList();
        }

        private async void GestionLogsBoughtSold()
        {
            if (!_logsClicked.HasValue || (_logsClicked.HasValue && (DateTime.Now - _logsClicked.Value).Ticks > TimeSpan.FromSeconds(10).Ticks))
            {
                _logsClicked = null;

                _logsBoughtSold = await Http.GetFromJsonAsync<List<LogInfoItemDto>>($"/api/GetBoughtSold?accType=Spot&accHolder={CultureInfo.CurrentCulture.TextInfo.ToTitleCase(selectedAccount)}", _cancelToken.Token);

                if (_logsBoughtSold != null)
                {
                    _logsBoughtSoldHistory = _logsBoughtSold.Where(x => x.SoldDate.HasValue && x.PriceSold > 0).ToList().OrderByDescending(x => x.BsId).ToList();

                    if (_logsBoughtSoldHistory != null)
                        _labelLogsBoughtSoldHistory = $"History ({_logsBoughtSoldHistory.Count().ToString()} trades)";

                    _logsBoughtSoldPositive = _logsBoughtSold.Where(x => !x.SoldDate.HasValue && x.Close > x.PriceBought).ToList().OrderByDescending(x => x.BsId).ToList();

                    if (_logsBoughtSoldPositive != null)
                        _labelLogsBoughtSoldPositive = $"Positive ({_logsBoughtSoldPositive.Count().ToString()} trades)";

                    _logsBoughtSoldNegative = _logsBoughtSold.Where(x => !x.SoldDate.HasValue && x.Close <= x.PriceBought).ToList().OrderByDescending(x => x.BsId).ToList();

                    if (_logsBoughtSoldNegative != null)
                        _labelLogsBoughtSoldNegative = $"Negative ({_logsBoughtSoldNegative.Count().ToString()} trades)";
                }
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

        protected void DataGridLogsBoughtSoldPage(Radzen.PagerEventArgs args)
        {
            if (args.PageIndex == 0)
                _logsClicked = null;
            else
                _logsClicked = DateTime.Now;
        }
    }
}
