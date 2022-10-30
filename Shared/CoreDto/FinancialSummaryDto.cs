using System;
using System.Collections.Generic;

namespace BlazorApp.Shared.CoreDto
{
    public class FinancialSummaryDto
    {
        public DateTime? Month { get; set; } = null;

        public int? CompletedTrades { get; set; } = null;
        public int? CompletedTradesPositives { get; set; } = null;
        public decimal? TotalBoughtCompletedTrades { get; set; } = null;
        public decimal? TotalSoldCompletedTrades { get; set; } = null;

        public int? IncompletedTrades { get; set; } = null;
        public decimal? TotalBoughtIncompletedTrades { get; set; } = null;
        public decimal? TotalSoldIncompletedTrades { get; set; } = null;

        //quote - bought - sold (Completed)
        public Dictionary<string, decimal> CompletedDetailsBought { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, decimal> CompletedDetailsSold { get; set; } = new Dictionary<string, decimal>();

        //quote - bought - sold (Incompleted)
        public Dictionary<string, decimal> IncompletedDetailsBought { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, decimal> IncompletedDetailsSold { get; set; } = new Dictionary<string, decimal>();

        public FinancialSummaryDto() { }
    }
}
