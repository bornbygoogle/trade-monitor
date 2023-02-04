using BlazorApp.Shared;
using BlazorApp.Shared.CoreDto;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Radzen;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http.Json;
using System.Security.Cryptography;
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

        private string _textCurrentAccount = string.Empty;
        private string _stringAccount = null;
        private AccountDto _account = null;

        private decimal? _accountSimulatedTotalTrades = null;
        private decimal? _accountSimulatedTotalPositiveTrades = null;
        private string _accountSimulatedFirstTradeDate = null;

        private decimal? _accountRealTotalTrades = null;
        private decimal? _accountRealTotalPositiveTrades = null;
        private string _accountRealFirstTradeDate = null;

        private bool _statSevenDays = true;
        private bool _statThirtyDays = false;
        private bool _statAllTimes = false;

        private List<DataItem> _nbrTrades = new List<DataItem>();
        private List<DataItem> _durationAverageTrades = new List<DataItem>();
        private decimal? _totalTrades = 0;
        private decimal? _percentageSucceededTrades = 0;
        private bool _realPercentage = true;



        private List<DataItem> _profitSimulated = new List<DataItem>();

        private List<DataItem> _profitReal = new List<DataItem>();

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

        public void RefreshTimer(Object source, ElapsedEventArgs e)
        {
            if (_cancelToken.Token.IsCancellationRequested)
                return;

            bool hasNewResult = false;

            try
            {
                refreshTimer.Stop();

                _textCurrentAccount = $"Account {selectedAccount.ToUpper()}";

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
                GestionPercentageTrade();
                GestionDurationAverageTrade();
            }                

            List<DataItem> newListItemSimulated = null;
            List<DataItem> newListItemReal = null;

            if (_statSevenDays)
            {
                newListItemSimulated = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosProfit?accType=Spot&accHolder=An&nbrDays=7&real=0", _cancelToken.Token);
                newListItemReal = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosProfit?accType=Spot&accHolder=An&nbrDays=7&real=1", _cancelToken.Token);

                _accountSimulatedFirstTradeDate = await Http.GetStringAsync($"/api/GetAccountInfosFirstTradeDate?accType=Spot&accHolder={CultureInfo.CurrentCulture.TextInfo.ToTitleCase(selectedAccount)}&nbrDays=7&real=0", _cancelToken.Token);
                _accountRealFirstTradeDate = await Http.GetStringAsync($"/api/GetAccountInfosFirstTradeDate?accType=Spot&accHolder={CultureInfo.CurrentCulture.TextInfo.ToTitleCase(selectedAccount)}&nbrDays=7&real=1", _cancelToken.Token);
            }
            else if (_statThirtyDays)
            {
                newListItemSimulated = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosProfit?accType=Spot&accHolder=An&nbrDays=30&real=0", _cancelToken.Token);
                newListItemReal = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosProfit?accType=Spot&accHolder=An&nbrDays=30&real=1", _cancelToken.Token);

                _accountSimulatedFirstTradeDate = await Http.GetStringAsync($"/api/GetAccountInfosFirstTradeDate?accType=Spot&accHolder={CultureInfo.CurrentCulture.TextInfo.ToTitleCase(selectedAccount)}&nbrDays=30&real=0", _cancelToken.Token);
                _accountRealFirstTradeDate = await Http.GetStringAsync($"/api/GetAccountInfosFirstTradeDate?accType=Spot&accHolder={CultureInfo.CurrentCulture.TextInfo.ToTitleCase(selectedAccount)}&nbrDays=30&real=1", _cancelToken.Token);
            }
            else
            {
                newListItemSimulated = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosProfit?accType=Spot&accHolder=An&real=0", _cancelToken.Token);
                newListItemReal = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosProfit?accType=Spot&accHolder=An&real=1", _cancelToken.Token);

                _accountSimulatedFirstTradeDate = await Http.GetStringAsync($"/api/GetAccountInfosFirstTradeDate?accType=Spot&accHolder={CultureInfo.CurrentCulture.TextInfo.ToTitleCase(selectedAccount)}&real=1", _cancelToken.Token);
                _accountRealFirstTradeDate = await Http.GetStringAsync($"/api/GetAccountInfosFirstTradeDate?accType=Spot&accHolder={CultureInfo.CurrentCulture.TextInfo.ToTitleCase(selectedAccount)}&real=0", _cancelToken.Token);
            }

            _accountSimulatedFirstTradeDate = $"First trade at {_accountSimulatedFirstTradeDate}";
            _accountRealFirstTradeDate = $"First trade at {_accountRealFirstTradeDate}";

            if (newListItemSimulated != null && _profitSimulated != null)
            {
                _profitSimulated.Clear();
                _profitSimulated.AddRange(newListItemSimulated);
            }

            if (newListItemReal != null && _profitReal != null)
            {
                _profitReal.Clear();
                _profitReal.AddRange(newListItemReal);
            }

            //CalculateProfitQuotes();

            GestionLogsBoughtSold();
        }

        private void CalculateProfitQuotes()
        {
            if (!string.IsNullOrEmpty(_stringAccount))
            {
                _account = JsonConvert.DeserializeObject<AccountDto>(_stringAccount);

                if (_account != null)
                {
                    _profitSimulated.Clear();
                    _profitReal.Clear();

                    foreach (var itemQuote in _account.ListQuotes)
                    {
                        double totalBoughtSimulated = 0;
                        double totalSoldSimulated = 0;

                        Dictionary<string, double> dictBoughtSimulated = new Dictionary<string, double>();
                        Dictionary<string, double> dictSoldSimulated = new Dictionary<string, double>();

                        var statAccountSimulated = _account.SimulatedAccountProfitSevenDays;

                        if (_statThirtyDays)
                            statAccountSimulated = _account.SimulatedAccountProfitThirtyDays;
                        else if (_statAllTimes)
                            statAccountSimulated = _account.SimulatedAccountProfitAllTimes;

                        if (statAccountSimulated != null)
                        {
                            foreach (var itemProfit in statAccountSimulated)
                            {
                                double listQuoteSold = (double)itemProfit.CompletedDetailsSold.Where(x => x.Key == itemQuote).Sum(x => x.Value);
                                double listQuoteBought = (double)itemProfit.CompletedDetailsBought.Where(x => x.Key == itemQuote).Sum(x => x.Value);

                                if (dictBoughtSimulated.ContainsKey(itemQuote))
                                {
                                    dictBoughtSimulated.TryGetValue(itemQuote, out totalBoughtSimulated);
                                    totalBoughtSimulated += listQuoteBought;

                                    dictBoughtSimulated.Remove(itemQuote);
                                    dictBoughtSimulated.Add(itemQuote, totalBoughtSimulated);
                                }
                                else
                                    dictBoughtSimulated.Add(itemQuote, listQuoteBought);

                                if (dictSoldSimulated.ContainsKey(itemQuote))
                                {
                                    dictSoldSimulated.TryGetValue(itemQuote, out totalSoldSimulated);
                                    totalSoldSimulated += listQuoteSold;

                                    dictSoldSimulated.Remove(itemQuote);
                                    dictSoldSimulated.Add(itemQuote, totalSoldSimulated);
                                }
                                else
                                    dictSoldSimulated.Add(itemQuote, listQuoteSold);
                            }
                        }

                        DataItem quoteSimulated = new DataItem();
                        quoteSimulated.Base = itemQuote;
                        quoteSimulated.Profit = totalSoldSimulated - totalBoughtSimulated;

                        if (_profitSimulated.Any(x => x.Base == quoteSimulated.Base))
                            _profitSimulated.Remove(quoteSimulated);

                        _profitSimulated.Add(quoteSimulated);

                        double totalBoughtReal = 0;
                        double totalSoldReal = 0;
                        Dictionary<string, double> dictBoughtReal = new Dictionary<string, double>();
                        Dictionary<string, double> dictSoldReal = new Dictionary<string, double>();

                        var statAccountReal = _account.RealAccountProfitSevenDays;

                        if (_statThirtyDays)
                            statAccountReal = _account.RealAccountProfitThirtyDays;
                        else if (_statAllTimes)
                            statAccountReal = _account.RealAccountProfitAllTimes;

                        if (statAccountReal != null)
                        {
                            foreach (var itemProfit in statAccountReal)
                            {
                                double listQuoteSold = (double)itemProfit.CompletedDetailsSold.Where(x => x.Key == itemQuote).Sum(x => x.Value);
                                double listQuoteBought = (double)itemProfit.CompletedDetailsBought.Where(x => x.Key == itemQuote).Sum(x => x.Value);

                                if (dictBoughtReal.ContainsKey(itemQuote))
                                {
                                    dictBoughtReal.TryGetValue(itemQuote, out totalBoughtReal);
                                    totalBoughtReal += listQuoteBought;

                                    dictBoughtReal.Remove(itemQuote);
                                    dictBoughtReal.Add(itemQuote, totalBoughtReal);
                                }
                                else
                                    dictBoughtReal.Add(itemQuote, listQuoteBought);

                                if (dictSoldReal.ContainsKey(itemQuote))
                                {
                                    dictSoldReal.TryGetValue(itemQuote, out totalSoldReal);
                                    totalSoldReal += listQuoteSold;

                                    dictSoldReal.Remove(itemQuote);
                                    dictSoldReal.Add(itemQuote, totalSoldReal);
                                }
                                else
                                    dictSoldReal.Add(itemQuote, listQuoteSold);
                            }
                        }

                        DataItem quoteReal = new DataItem();
                        quoteReal.Base = itemQuote;
                        quoteReal.Profit = totalSoldReal - totalBoughtReal;

                        if (_profitReal.Any(x => x.Base == quoteReal.Base))
                            _profitReal.Remove(quoteReal);

                        _profitReal.Add(quoteReal);
                    }
                }
            }
        }

        private async void GestionPercentageTrade()
        {
            _nbrTrades.Clear();

            if (_realPercentage)
            {
                if (_statSevenDays)
                {
                    _accountRealTotalTrades = await Http.GetFromJsonAsync<decimal>($"/api/GetAccountInfosTotalTrades?accType=Spot&accHolder={CultureInfo.CurrentCulture.TextInfo.ToTitleCase(selectedAccount)}&nbrDays=7&real=1", _cancelToken.Token);
                    _accountRealTotalPositiveTrades = await Http.GetFromJsonAsync<decimal>($"/api/GetAccountInfosTotalPositiveTrades?accType=Spot&accHolder={CultureInfo.CurrentCulture.TextInfo.ToTitleCase(selectedAccount)}&nbrDays=7&real=1", _cancelToken.Token);
                }
                else if (_statThirtyDays)
                {
                    _accountRealTotalTrades = await Http.GetFromJsonAsync<decimal>($"/api/GetAccountInfosTotalTrades?accType=Spot&accHolder={CultureInfo.CurrentCulture.TextInfo.ToTitleCase(selectedAccount)}&nbrDays=30&real=1", _cancelToken.Token);
                    _accountRealTotalPositiveTrades = await Http.GetFromJsonAsync<decimal>($"/api/GetAccountInfosTotalPositiveTrades?accType=Spot&accHolder={CultureInfo.CurrentCulture.TextInfo.ToTitleCase(selectedAccount)}&nbrDays=30&real=1", _cancelToken.Token);
                }
                else
                {
                    _accountRealTotalTrades = await Http.GetFromJsonAsync<decimal>($"/api/GetAccountInfosTotalTrades?accType=Spot&accHolder={CultureInfo.CurrentCulture.TextInfo.ToTitleCase(selectedAccount)}&real=1", _cancelToken.Token);
                    _accountRealTotalPositiveTrades = await Http.GetFromJsonAsync<decimal>($"/api/GetAccountInfosTotalPositiveTrades?accType=Spot&accHolder={CultureInfo.CurrentCulture.TextInfo.ToTitleCase(selectedAccount)}&real=1", _cancelToken.Token);
                }
            }
            else
            {
                if (_statSevenDays)
                {
                    _accountSimulatedTotalTrades = await Http.GetFromJsonAsync<decimal>($"/api/GetAccountInfosTotalTrades?accType=Spot&accHolder={CultureInfo.CurrentCulture.TextInfo.ToTitleCase(selectedAccount)}&nbrDays=7&real=0", _cancelToken.Token);
                    _accountSimulatedTotalPositiveTrades = await Http.GetFromJsonAsync<decimal>($"/api/GetAccountInfosTotalPositiveTrades?accType=Spot&accHolder={CultureInfo.CurrentCulture.TextInfo.ToTitleCase(selectedAccount)}&nbrDays=7&real=0", _cancelToken.Token);

                }
                else if (_statThirtyDays)
                {
                    _accountSimulatedTotalTrades = await Http.GetFromJsonAsync<decimal>($"/api/GetAccountInfosTotalTrades?accType=Spot&accHolder={CultureInfo.CurrentCulture.TextInfo.ToTitleCase(selectedAccount)}&nbrDays=30&real=0", _cancelToken.Token);
                    _accountSimulatedTotalPositiveTrades = await Http.GetFromJsonAsync<decimal>($"/api/GetAccountInfosTotalPositiveTrades?accType=Spot&accHolder={CultureInfo.CurrentCulture.TextInfo.ToTitleCase(selectedAccount)}&nbrDays=30&real=0", _cancelToken.Token);
                }
                else
                {
                    _accountSimulatedTotalTrades = await Http.GetFromJsonAsync<decimal>($"/api/GetAccountInfosTotalTrades?accType=Spot&accHolder={CultureInfo.CurrentCulture.TextInfo.ToTitleCase(selectedAccount)}&real=0", _cancelToken.Token);
                    _accountSimulatedTotalPositiveTrades = await Http.GetFromJsonAsync<decimal>($"/api/GetAccountInfosTotalPositiveTrades?accType=Spot&accHolder={CultureInfo.CurrentCulture.TextInfo.ToTitleCase(selectedAccount)}&real=0", _cancelToken.Token);
                }
            }

            if (_realPercentage)
            {
                if (_accountRealTotalPositiveTrades.HasValue && _accountRealTotalTrades.HasValue)
                {
                    DataItem nbrTradesRealSucceeded = new DataItem();
                    nbrTradesRealSucceeded.Base = "Succeeded trade";
                    nbrTradesRealSucceeded.Profit = (double)(_accountRealTotalPositiveTrades ?? 0);

                    _nbrTrades.Add(nbrTradesRealSucceeded);

                    DataItem nbrTradesRealNotSucceeded = new DataItem();
                    nbrTradesRealNotSucceeded.Base = "Failed trade";
                    nbrTradesRealNotSucceeded.Profit = (double)((_accountRealTotalTrades - _accountRealTotalPositiveTrades) ?? 0);

                    _totalTrades = _accountRealTotalTrades ?? 0;
                    _percentageSucceededTrades = Math.Round(((_accountRealTotalPositiveTrades ?? 0) * 100) / (_accountRealTotalTrades ?? 1), 2);

                    _nbrTrades.Add(nbrTradesRealNotSucceeded);
                }
            }
            else
            {
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

            _nbrTrades = _nbrTrades.ToList();
        }

        private async void GestionDurationAverageTrade()
        {
            List<DataItem> tmpListDataItem = null;

            if (_statSevenDays)
                tmpListDataItem = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosDurationAverageCompletedTrade?accType=Spot&accHolder={CultureInfo.CurrentCulture.TextInfo.ToTitleCase(selectedAccount)}&nbrDays=7&real={(_realPercentage ? "1" : "0")}", _cancelToken.Token);
            else if (_statThirtyDays)
                tmpListDataItem = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosDurationAverageCompletedTrade?accType=Spot&accHolder={CultureInfo.CurrentCulture.TextInfo.ToTitleCase(selectedAccount)}&nbrDays=30&real={(_realPercentage ? "1" : "0")}", _cancelToken.Token);
            else
                tmpListDataItem = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosDurationAverageCompletedTrade?accType=Spot&accHolder={CultureInfo.CurrentCulture.TextInfo.ToTitleCase(selectedAccount)}&real={(_realPercentage ? "1" : "0")}", _cancelToken.Token);

            if (tmpListDataItem != null)
            {
                _durationAverageTrades.Clear();
                _durationAverageTrades = tmpListDataItem.ToList();
            }            
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

        protected void ButtonGetRealTradesPercentageClick(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            _realPercentage = true;
        }

        protected void ButtonGetPossibleTradesPercentageClick(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            _realPercentage = false;
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
