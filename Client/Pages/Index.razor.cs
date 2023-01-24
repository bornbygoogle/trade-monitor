using BlazorApp.Shared.CoreDto;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Radzen;
using System.Globalization;
using System.Net.Http.Json;
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

        private string _stringAccount = null;
        private AccountDto _account = null;

        private bool _statSevenDays = true;
        private bool _statThirtyDays = false;
        private bool _statAllTimes = false;

        private bool _onlyLogTrading = true;
        private bool _onlyLogTradingInfos = true;
        private bool _allLogs = true;

        private List<DataItem> profitSimulated = new List<DataItem>();
        private List<DataItem> profitReal = new List<DataItem>();

        private Dictionary<string, double> _dictBoughtSimulated = new Dictionary<string, double>();
        private Dictionary<string, double> _dictSoldSimulated = new Dictionary<string, double>();
        private Dictionary<string, double> _dictBoughtReal = new Dictionary<string, double>();
        private Dictionary<string, double> _dictSoldReal = new Dictionary<string, double>();

        private List<LogInfoItemDto> _logs = null;
        private List<LogInfoItemDto> _logsPotential = null;

        private bool panelPotentialCollapsed = true;
        private bool panelLogCollapsed = false;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                if (refreshTimer == null)
                {
                    refreshTimer = new System.Timers.Timer(50000);
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

                hasNewResult = true;
            }
            catch
            {
                hasNewResult = false;
            }

            if (hasNewResult)
                _ = InvokeAsync(StateHasChanged);
        }

        private async void GestionTimer()
        {
            _stringAccount = await Http.GetStringAsync($"/api/GetInfos?accType=Spot&accHolder=An");
            CalculateProfitQuotes();

            _logs = await Http.GetFromJsonAsync<List<LogInfoItemDto>>($"/api/GetAllLogs?accType=Spot&accHolder=An");
            _logsPotential = await Http.GetFromJsonAsync<List<LogInfoItemDto>>($"/api/GetLogKlinePotential?accType=Spot&accHolder=An");
        }

        private void CalculateProfitQuotes()
        {
            if (!string.IsNullOrEmpty(_stringAccount))
            {
                _account = JsonConvert.DeserializeObject<AccountDto>(_stringAccount);

                if (_account != null)
                {
                    profitSimulated.Clear();
                    profitReal.Clear();

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

                        if (profitSimulated.Any(x => x.Base == quoteSimulated.Base))
                            profitSimulated.Remove(quoteSimulated);

                        profitSimulated.Add(quoteSimulated);


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

                        if (profitReal.Any(x => x.Base == quoteReal.Base))
                            profitReal.Remove(quoteReal);

                        profitReal.Add(quoteReal);
                    }
                }
            }
        }

        protected async System.Threading.Tasks.Task ButtonStatSevenDaysClick(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            _statSevenDays= true;
        }

        protected async System.Threading.Tasks.Task ButtonStatThirtyDaysClick(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            _statThirtyDays = true;
        }

        protected async System.Threading.Tasks.Task ButtonStatAllTimesClick(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            _statAllTimes = true;
        }

        protected async System.Threading.Tasks.Task ButtonLogTradingClick(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            _onlyLogTrading = true;
        }

        protected async System.Threading.Tasks.Task ButtonLogTradingInfosClick(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            _onlyLogTradingInfos = true;
        }

        protected async System.Threading.Tasks.Task ButtonAlLogsClick(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            _allLogs = true;
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
    }
}
