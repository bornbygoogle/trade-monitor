using BlazorApp.Shared.CoreDto;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using System;
using System.IO.Pipelines;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

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

        //private List<SymbolItemDto> Symbols = null;
        //private SymbolItemDto SelectedSymbol = null;
        private string SelectedSymbol = null;
        private List<string> SymbolsInString = null;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                SymbolsInString = await Http.GetFromJsonAsync<List<string>>($"/api/GetListStringSymbols?accType=Spot&accHolder=An");
                //Symbols = await Http.GetFromJsonAsync<List<SymbolItemDto>>($"/api/GetListSymbolsWithBaseQuote?accType=Spot&accHolder=An") ?? new List<SymbolItemDto> { };
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
