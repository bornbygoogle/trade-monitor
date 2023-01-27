using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using System.Net.Http.Json;

namespace BlazorApp.Client.Shared
{
    public partial class MainLayout
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

        public bool panelCurrentSymbolExpanded = true;
        public bool panelCurrentSymbolCollapsed = false;

        private string SelectedSymbol = null;
        private List<string> SymbolsInString = null;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                SymbolsInString = await Http.GetFromJsonAsync<List<string>>($"/api/GetListStringSymbols?accType=Spot&accHolder=An");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        void SidebarToggleClick()
        {
            sidebarExpanded = !sidebarExpanded;
        }

        void OnChange(string value)
        {
            SelectedSymbol = value;
        }
    }
}
