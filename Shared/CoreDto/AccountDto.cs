using System.Collections.Generic;

namespace BlazorApp.Shared.CoreDto
{
    public class AccountDto : DtoBase
    {
        public string AccountHolder { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public string Interval { get; set; } = string.Empty;
        public int NbrSymbols { get; set; } = 0;
        public string UseTDCountDown { get; set; } = string.Empty;
        public string UseTDCombo { get; set; } = string.Empty;
        public string UseConservatoire { get; set; } = string.Empty;

        /// <summary>
        /// 0 = None / 1 = Buy only / 2 = Sell only / 3 = Both
        /// </summary>
        public string AllowBuySell { get; set; } = string.Empty;

        public List<string> ListQuotes { get; set; } = new List<string>();

        public List<FinancialSummaryDto> RealAccountProfit { get; set; } = new List<FinancialSummaryDto>();

        public decimal? RealTotalTrades { get; set; } = null;
        public decimal? RealTotalPositiveTrades { get; set; } = null;
        public decimal? RealPositivePercentage { get; set; } = null;

        public List<FinancialSummaryDto> SimulatedAccountProfit { get; set; } = new List<FinancialSummaryDto>();

        public decimal? SimulatedTotalTrades { get; set; } = null;
        public decimal? SimulatedTotalPositiveTrades { get; set; } = null;
        public decimal? SimulatedPositivePercentage { get; set; } = null;

        public AccountDto() { }
    }
}
