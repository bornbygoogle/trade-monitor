using BlazorApp.Shared;
using BlazorApp.Shared.CoreDto;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using System.Net.Http.Json;
using System.Timers;

namespace BlazorApp.Client.Shared
{
    public partial class MainLayout : IDisposable
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

        private bool sidebarExpanded = true;

        private CancellationTokenSource _cancelToken = null;

        private static System.Timers.Timer refreshTimer;

        public bool panelCurrentSymbolExpanded = true;
        public bool panelCurrentSymbolCollapsed = false;

        private string SelectedSymbol = null;
        private List<string> SymbolsInString = null;

        private bool _onlyLogTrading = true;
        private bool _onlyLogTradingInfos = false;
        private bool _allLogs = false;
        private DateTime? _dtPageClicked = null;

        private List<LogInfoItemDto> _logs = null;

        private bool panelLogCollapsed = false;

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

                SymbolsInString = await Http.GetFromJsonAsync<List<string>>($"/api/GetListStringSymbols?accType=Spot&accHolder=An", _cancelToken.Token);

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
            string urlLog;
            if (_onlyLogTrading)
                urlLog = $"/api/GetLogsTrading?accType=Spot&accHolder=An";
            else if (_onlyLogTradingInfos)
                urlLog = $"/api/GetLogsTradingInfos?accType=Spot&accHolder=An";
            else
                urlLog = $"/api/GetAllLogs?accType=Spot&accHolder=An";

            if (!_dtPageClicked.HasValue || (_dtPageClicked.HasValue && (DateTime.Now - _dtPageClicked.Value).Ticks > TimeSpan.FromSeconds(ClsUtilCommon.TIMER_DURATION).Ticks))
                _logs = await Http.GetFromJsonAsync<List<LogInfoItemDto>>(urlLog, _cancelToken.Token);
        }

        void SidebarToggleClick()
        {
            sidebarExpanded = !sidebarExpanded;
        }

        void OnChange(string value)
        {
            SelectedSymbol = value;
        }

        protected void ButtonLogTradingClick(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            _onlyLogTrading = true;
            _onlyLogTradingInfos = false;
            _allLogs = false;
        }

        protected void ButtonLogTradingInfosClick(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            _onlyLogTrading = false;
            _onlyLogTradingInfos = true;
            _allLogs = false;
        }

        protected void ButtonAlLogsClick(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            _onlyLogTrading = false;
            _onlyLogTradingInfos = false;
            _allLogs = true;
        }

        protected void DataGridLogsPageChanged(Radzen.PagerEventArgs args)
        {
            if (args.PageIndex == 0)
                _dtPageClicked = null;
            else
                _dtPageClicked = DateTime.Now;
        }
    }
}
