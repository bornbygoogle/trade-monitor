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

        private async void GestionTimer()
        {
            List<DataItem> newListItem = null;

            if (_statSevenDays)
                newListItem = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosSimulatedProfit?accType=Spot&accHolder=An&nbrDays=7", _cancelToken.Token);
            else if (_statThirtyDays)
                newListItem = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosSimulatedProfit?accType=Spot&accHolder=An&nbrDays=30", _cancelToken.Token);
            else
                newListItem = await Http.GetFromJsonAsync<List<DataItem>>($"/api/GetAccountInfosSimulatedProfit?accType=Spot&accHolder=An", _cancelToken.Token);

            if (newListItem != null && _profitSimulated != null)
            {
                _profitSimulated.Clear();
                _profitSimulated.AddRange(newListItem);
            }

            GestionPercentageTrade();

            if (_tdPotential)
                _logsPotential = await Http.GetFromJsonAsync<List<LogInfoItemDto>>($"/api/GetLogKlinePotential?accType=Spot&accHolder=An", _cancelToken.Token);
            else if (_tdCombo)
                _logsPotential = await Http.GetFromJsonAsync<List<LogInfoItemDto>>($"/api/GetLogKlineTDCombo?accType=Spot&accHolder=An", _cancelToken.Token);
            else if (_tdCountDown)
                _logsPotential = await Http.GetFromJsonAsync<List<LogInfoItemDto>>($"/api/GetLogKlineTDCountDown?accType=Spot&accHolder=An", _cancelToken.Token);
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

                        if (_account.RealAccountProfitAllTimes != null)
                        {
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
                    }
                }
            }
        }

        private async void GestionPercentageTrade()
        {
            _nbrTrades.Clear();

            _accountSimulatedTotalTrades = await Http.GetFromJsonAsync<decimal>($"/api/GetAccountInfosSimulatedTotalTrades?accType=Spot&accHolder=An", _cancelToken.Token);
            _accountSimulatedTotalPositiveTrades = await Http.GetFromJsonAsync<decimal>($"/api/GetAccountInfosSimulatedTotalPositiveTrades?accType=Spot&accHolder=An", _cancelToken.Token);

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
