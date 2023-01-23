using BlazorApp.Shared.CoreDto;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using System.Net.Http.Json;
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

        private List<LogInfoItemDto> _logs = null;
        private List<LogInfoItemDto> _logsPotential = null;

        private bool panelPotentialCollapsed = true;
        private bool panelLogCollapsed = true;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                if (refreshTimer == null)
                {
                    refreshTimer = new System.Timers.Timer(10000);
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
                _logs = await Http.GetFromJsonAsync<List<LogInfoItemDto>>($"/api/GetAllLogs?accType=Spot&accHolder=An");
                _logsPotential = await Http.GetFromJsonAsync<List<LogInfoItemDto>>($"/api/GetLogKlinePotential?accType=Spot&accHolder=An");

                hasNewResult = true;
            }
            catch
            {
                hasNewResult = false;
            }

            if (hasNewResult)
                _ = InvokeAsync(StateHasChanged);
        }
    }
}
