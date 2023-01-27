using BlazorApp.Shared;
using BlazorApp.Shared.CoreDto;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Radzen;
using System.Net.Http.Json;
using System.Timers;

namespace BlazorApp.Client.Pages
{
    public partial class Index_Yannick
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

        private static bool _onWork = false;

        private string _selectedAccount = "Yannick";

        private static System.Timers.Timer refreshTimer;
        private static int _currentNumber = 0;

        private string _textCurrentAccount = string.Empty;
        private string _stringAccount = null;
        private AccountDto _account = null;

        private bool _statSevenDays = true;
        private bool _statThirtyDays = false;
        private bool _statAllTimes = false;

        private List<DataItem> _nbrTrades = new List<DataItem>();
        private decimal? _totalTrades = 0;
        private decimal? _percentageSucceededTrades = 0;
        private bool _realPercentage = true;

        private List<DataItem> _profitSimulated = new List<DataItem>();
        private List<DataItem> _profitReal = new List<DataItem>();



        private List<LogInfoItemDto> _logs = null;
        private List<LogInfoItemDto> _logsPotential = null;

        private List<LogInfoItemDto> _logsBoughtSold = null;

        private bool panelBoughtSoldCollapsed = false;

        private bool panelPotentialCollapsed = true;
        private bool panelLogCollapsed = false;

        bool showDataLabels = true;

        protected override async Task OnInitializedAsync()
        {
            _textCurrentAccount = $"Account {_selectedAccount}";

            try
            {
                if (refreshTimer == null)
                {
                    refreshTimer = new System.Timers.Timer(TimeSpan.FromSeconds(1).TotalMilliseconds);
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

            try
            {
                if (!_onWork)
                {
                    _onWork = true;

                    GestionTimer();

                    Interlocked.Increment(ref _currentNumber);

                    hasNewResult = true;

                    _onWork = false;
                }
            }
            catch
            {
                hasNewResult = false;
            }

            if (hasNewResult)
            {
                if (_currentNumber == 31)
                    Interlocked.Exchange(ref _currentNumber, 0);

                _ = InvokeAsync(StateHasChanged);
            }
        }

        private async void GestionTimer()
        {
            if (_currentNumber == 10 || _currentNumber == 30)
            {
                _stringAccount = await Http.GetStringAsync($"/api/GetInfos?accType=Spot&accHolder={_selectedAccount}");
                CalculateProfitQuotes();

                _logsBoughtSold = await Http.GetFromJsonAsync<List<LogInfoItemDto>>($"/api/GetBoughtSold?accType=Spot&accHolder={_selectedAccount}");
            }
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

                        DataItem quoteReal = new DataItem();
                        quoteReal.Base = itemQuote;
                        quoteReal.Profit = totalSoldReal - totalBoughtReal;

                        if (_profitReal.Any(x => x.Base == quoteReal.Base))
                            _profitReal.Remove(quoteReal);

                        _profitReal.Add(quoteReal);
                    }

                    _nbrTrades.Clear();

                    if (_realPercentage)
                    {
                        DataItem nbrTradesRealSucceeded = new DataItem();
                        nbrTradesRealSucceeded.Base = "Succeeded trade";
                        nbrTradesRealSucceeded.Profit = (double)(_account.RealTotalPositiveTrades ?? 0);

                        _nbrTrades.Add(nbrTradesRealSucceeded);

                        DataItem nbrTradesRealNotSucceeded = new DataItem();
                        nbrTradesRealNotSucceeded.Base = "Failed trade";
                        nbrTradesRealNotSucceeded.Profit = (double)((_account.RealTotalTrades - _account.RealTotalPositiveTrades) ?? 0);

                        _totalTrades = _account.RealTotalTrades ?? 0;
                        _percentageSucceededTrades = Math.Round(((_account.RealTotalPositiveTrades ?? 0) * 100) / (_account.RealTotalTrades ?? 1), 2);

                        _nbrTrades.Add(nbrTradesRealNotSucceeded);
                    }
                    else
                    {
                        DataItem nbrTradesSimulatedSucceeded = new DataItem();
                        nbrTradesSimulatedSucceeded.Base = "Succeeded trade";
                        nbrTradesSimulatedSucceeded.Profit = (double)(_account.SimulatedTotalPositiveTrades ?? 0);

                        _nbrTrades.Add(nbrTradesSimulatedSucceeded);

                        DataItem nbrTradesSimulatedNotSucceeded = new DataItem();
                        nbrTradesSimulatedNotSucceeded.Base = "Failed trade";
                        nbrTradesSimulatedNotSucceeded.Profit = (double)((_account.SimulatedTotalTrades - _account.SimulatedTotalPositiveTrades) ?? 0);

                        _totalTrades = _account.SimulatedTotalTrades ?? 0;
                        _percentageSucceededTrades = Math.Round(((_account.SimulatedTotalPositiveTrades ?? 0) * 100) / (_account.SimulatedTotalTrades ?? 1), 2);

                        _nbrTrades.Add(nbrTradesSimulatedNotSucceeded);
                    }
                }
            }
        }

        protected async System.Threading.Tasks.Task ButtonStatSevenDaysClick(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            _statSevenDays = true;
            _statThirtyDays = false;
            _statAllTimes = false;
        }

        protected async System.Threading.Tasks.Task ButtonStatThirtyDaysClick(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            _statSevenDays = false;
            _statThirtyDays = true;
            _statAllTimes = false;
        }

        protected async System.Threading.Tasks.Task ButtonStatAllTimesClick(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            _statSevenDays = false;
            _statThirtyDays = false;
            _statAllTimes = true;
        }

        protected async System.Threading.Tasks.Task ButtonGetRealTradesPercentageClick(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            _realPercentage = true;
        }

        protected async System.Threading.Tasks.Task ButtonGetPossibleTradesPercentageClick(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            _realPercentage = false;
        }
    }
}
