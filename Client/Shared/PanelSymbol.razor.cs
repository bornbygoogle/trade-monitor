using BlazorApp.Client.Pages;
using BlazorApp.Shared.CoreDto;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Radzen;
using System.Net.Http.Json;

namespace BlazorApp.Client.Shared
{
    public partial class PanelSymbol
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


        private string _symbol;
        private SymbolItemDto _selectedSymbol;

        private LogInfoItemDto _latestKline;

        public bool panelCurrentSymbolExpanded = true;
        public bool panelCurrentSymbolCollapsed = false;


        [ParameterAttribute]
        public string Symbol
        {
            get 
            { 
                return _symbol; 
            }
            set 
            { 
                _symbol = value; 
            }
        }

        protected override async Task OnParametersSetAsync()
        {
            base.OnParametersSet();

            try
            {
                if (!string.IsNullOrEmpty(_symbol))
                {
                    var listSymbols = await Http.GetFromJsonAsync<List<SymbolItemDto>>($"/api/GetListSymbolsWithBaseQuote?accType=Spot&accHolder=An&symbol={_symbol}");
                    if (listSymbols != null && listSymbols.Count > 0)
                        _selectedSymbol = listSymbols.First();

                    var listKlines = await Http.GetFromJsonAsync<List<LogInfoItemDto>>($"/api/GetLogKlines?accType=Spot&accHolder=An&symbol={_symbol}");
                    if (listKlines != null && listKlines.Count > 0)
                        _latestKline = listKlines.OrderByDescending(x => x.Id).First();
                }
                else
                {
                    _selectedSymbol = null;
                    _latestKline = null;
                }
            }
            catch (Exception ex)
            {
                var test = ex;

                _selectedSymbol = null;
                _latestKline = null;
            }


        }
    }
}
