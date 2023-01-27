using BlazorApp.Shared.CoreDto;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Radzen;
using System.Globalization;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Timers;

namespace BlazorApp.Client.Pages
{
    public partial class Index
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

        //[CascadingParameter(Name = "Symbols")]
        //protected List<SymbolItemDto>? Symbols { get; set; }

        private static System.Timers.Timer refreshTimer;
        private static int _currentNumber = 0;

        private string _currentAccount = "Samuel";
        private string _textCurrentAccount = string.Empty;
        private string _stringAccount = null;
        private AccountDto _account = null;

        private bool _statSevenDays = true;
        private bool _statThirtyDays = false;
        private bool _statAllTimes = false;

        private bool _onlyLogTrading = true;
        private bool _onlyLogTradingInfos = false;
        private bool _allLogs = false;
        private DateTime? _dtPageSize = null;

        private bool _tdPotential = true;
        private bool _tdCombo = false;
        private bool _tdCountDown = false;

        private List<DataItem> _nbrTrades = new List<DataItem>();
        private decimal? _totalTrades = 0;
        private decimal? _percentageSucceededTrades = 0;
        private bool _realPercentage = true;

        private List<DataItem> _profitSimulated = new List<DataItem>();
        private List<DataItem> _profitReal = new List<DataItem>();

        private Dictionary<string, double> _dictBoughtSimulated = new Dictionary<string, double>();
        private Dictionary<string, double> _dictSoldSimulated = new Dictionary<string, double>();
        private Dictionary<string, double> _dictBoughtReal = new Dictionary<string, double>();
        private Dictionary<string, double> _dictSoldReal = new Dictionary<string, double>();

        private List<LogInfoItemDto> _logs = null;
        private List<LogInfoItemDto> _logsPotential = null;

        private List<LogInfoItemDto> _logsBoughtSold = null;

        private bool panelBoughtSoldCollapsed = false;

        private bool panelPotentialCollapsed = true;
        private bool panelLogCollapsed = false;

        protected override async Task OnInitializedAsync()
        {
            _textCurrentAccount = $"Account {_currentAccount.ToUpper()}";

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
                GestionTimer();

                Interlocked.Increment(ref _currentNumber);

                hasNewResult = true;
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
                _stringAccount = await Http.GetStringAsync($"/api/GetInfos?accType=Spot&accHolder={_currentAccount}");
                CalculateProfitQuotes();

                _logsBoughtSold = await Http.GetFromJsonAsync<List<LogInfoItemDto>>($"/api/GetBoughtSold?accType=Spot&accHolder={_currentAccount}");
            }                

            string urlLog = $"/api/GetAllLogs?accType=Spot&accHolder=An";

            if (_onlyLogTrading)
                urlLog = $"/api/GetLogsTrading?accType=Spot&accHolder=An";
            else if (_onlyLogTradingInfos)
                urlLog = $"/api/GetLogsTradingInfos?accType=Spot&accHolder=An";
            else
                urlLog = $"/api/GetAllLogs?accType=Spot&accHolder=An";

            _logs = await Http.GetFromJsonAsync<List<LogInfoItemDto>>(urlLog);

            if (_tdPotential)
                _logsPotential = await Http.GetFromJsonAsync<List<LogInfoItemDto>>($"/api/GetLogKlinePotential?accType=Spot&accHolder=An");
            else if (_tdCombo)
                _logsPotential = await Http.GetFromJsonAsync<List<LogInfoItemDto>>($"/api/GetLogKlineTDCombo?accType=Spot&accHolder=An");
            else if (_tdCountDown)
                _logsPotential = await Http.GetFromJsonAsync<List<LogInfoItemDto>>($"/api/GetLogKlineTDCountDown?accType=Spot&accHolder=An");
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
                        _dictBoughtSimulated.Clear();
                        _dictSoldSimulated.Clear();

                        var statAccountSimulated = _account.SimulatedAccountProfitSevenDays;

                        if (_statThirtyDays)
                            statAccountSimulated = _account.SimulatedAccountProfitThirtyDays;
                        else if (_statAllTimes)
                            statAccountSimulated = _account.SimulatedAccountProfitAllTimes;

                        foreach (var itemProfit in statAccountSimulated)
                        {
                            double listQuoteSold = (double)itemProfit.CompletedDetailsSold.Where(x => x.Key == itemQuote).Sum(x => x.Value);
                            double listQuoteBought = (double)itemProfit.CompletedDetailsBought.Where(x => x.Key == itemQuote).Sum(x => x.Value);

                            if (_dictBoughtSimulated.ContainsKey(itemQuote))
                            {
                                _dictBoughtSimulated.TryGetValue(itemQuote, out totalBoughtSimulated);
                                totalBoughtSimulated += listQuoteBought;

                                _dictBoughtSimulated.Remove(itemQuote);
                                _dictBoughtSimulated.Add(itemQuote, totalBoughtSimulated);
                            }
                            else
                                _dictBoughtSimulated.Add(itemQuote, listQuoteBought);

                            if (_dictSoldSimulated.ContainsKey(itemQuote))
                            {
                                _dictSoldSimulated.TryGetValue(itemQuote, out totalSoldSimulated);
                                totalSoldSimulated += listQuoteSold;

                                _dictSoldSimulated.Remove(itemQuote);
                                _dictSoldSimulated.Add(itemQuote, totalSoldSimulated);
                            }
                            else
                                _dictSoldSimulated.Add(itemQuote, listQuoteSold);
                        }

                        DataItem quoteSimulated = new DataItem();
                        quoteSimulated.Base = itemQuote;
                        quoteSimulated.Profit = totalSoldSimulated - totalBoughtSimulated;

                        if (_profitSimulated.Any(x => x.Base == quoteSimulated.Base))
                            _profitSimulated.Remove(quoteSimulated);

                        _profitSimulated.Add(quoteSimulated);

                        double totalBoughtReal = 0;
                        double totalSoldReal = 0;
                        _dictBoughtReal.Clear();
                        _dictSoldReal.Clear();

                        var statAccountReal = _account.RealAccountProfitSevenDays;

                        if (_statThirtyDays)
                            statAccountReal = _account.RealAccountProfitThirtyDays;
                        else if (_statAllTimes)
                            statAccountReal = _account.RealAccountProfitAllTimes;

                        foreach (var itemProfit in statAccountReal)
                        {
                            double listQuoteSold = (double)itemProfit.CompletedDetailsSold.Where(x => x.Key == itemQuote).Sum(x => x.Value);
                            double listQuoteBought = (double)itemProfit.CompletedDetailsBought.Where(x => x.Key == itemQuote).Sum(x => x.Value);

                            if (_dictBoughtReal.ContainsKey(itemQuote))
                            {
                                _dictBoughtReal.TryGetValue(itemQuote, out totalBoughtReal);
                                totalBoughtReal += listQuoteBought;

                                _dictBoughtReal.Remove(itemQuote);
                                _dictBoughtReal.Add(itemQuote, totalBoughtReal);
                            }
                            else
                                _dictBoughtReal.Add(itemQuote, listQuoteBought);

                            if (_dictSoldReal.ContainsKey(itemQuote))
                            {
                                _dictSoldReal.TryGetValue(itemQuote, out totalSoldReal);
                                totalSoldReal += listQuoteSold;

                                _dictSoldReal.Remove(itemQuote);
                                _dictSoldReal.Add(itemQuote, totalSoldReal);
                            }
                            else
                                _dictSoldReal.Add(itemQuote, listQuoteSold);
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

        protected async System.Threading.Tasks.Task ButtonLogTradingClick(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            _onlyLogTrading = true;
            _onlyLogTradingInfos = false;
            _allLogs = false;
        }

        protected async System.Threading.Tasks.Task ButtonLogTradingInfosClick(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            _onlyLogTrading = false;
            _onlyLogTradingInfos = true;
            _allLogs = false;
        }

        protected async System.Threading.Tasks.Task ButtonAlLogsClick(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            _onlyLogTrading = false;
            _onlyLogTradingInfos = false;
            _allLogs = true;
        }

        protected async System.Threading.Tasks.Task ButtonGetRealTradesPercentageClick(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            _realPercentage = true;
        }

        protected async System.Threading.Tasks.Task ButtonGetPossibleTradesPercentageClick(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            _realPercentage = false;
        }

        protected async System.Threading.Tasks.Task ButtonTDPotentialClick(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            _tdPotential = true;
            _tdCombo = false;
            _tdCountDown = false;
        }

        protected async System.Threading.Tasks.Task ButtonTDComboClick(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            _tdPotential = false;
            _tdCombo = true;
            _tdCountDown = false;
        }

        protected async System.Threading.Tasks.Task ButtonTDCountDownClick(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            _tdPotential = false;
            _tdCombo = false;
            _tdCountDown = true;
        }

        protected async System.Threading.Tasks.Task DataGridLogsPageSizeChanged(System.Int32 args)
        {
            _dtPageSize = DateTime.Now;
        }

        bool showDataLabels = true;

        class DataItem
        {
            public string Base { get; set; }
            public double Profit { get; set; }
        }

        string FormatAsUSD(object value)
        {
            //return ((double)value).ToString("###,###,###.#####", CultureInfo.CreateSpecificCulture("en-US"));
            return ((double)value).ToString("#,###.#####", CultureInfo.CreateSpecificCulture("en-US"));
        }

        //DataItem[] revenue2020 = new DataItem[]
        //{
        //    new DataItem
        //    {
        //        Base = "Q1",
        //        Profit = 254000
        //    },
        //    new DataItem
        //    {
        //        Base = "Q2",
        //        Profit = 324000
        //    },
        //    new DataItem
        //    {
        //        Base = "Q3",
        //        Profit = 354000
        //    },
        //    new DataItem
        //    {
        //        Base = "Q4",
        //        Profit = 394000
        //    },
        //};
    }
}
