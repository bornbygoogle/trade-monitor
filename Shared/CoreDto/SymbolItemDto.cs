using System;

namespace BlazorApp.Shared.CoreDto
{
    public class SymbolItemDto : DtoBase
    {
        public long Id { get; set; }

        public string Symbol { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public string SymbolBase { get; set; } = string.Empty;
        public string SymbolQuote { get; set; } = string.Empty;

        public decimal? SymbolStepSizeBase { get; set; } = null;
        public decimal? SymbolMinQuantityBase { get; set; } = null;
        public decimal? SymbolMaxQuantityBase { get; set; } = null;
        public decimal? SymbolStepSizeQuote { get; set; } = null;
        public decimal? SymbolMinQuantityQuote { get; set; } = null;
        public decimal? SymbolMaxQuantityQuote { get; set; } = null;

        public decimal? SymbolMultiplierUp { get; set; } = null;
        public decimal? SymbolMultiplierDown { get; set; } = null;
        public decimal? SymbolAveragePriceMinutes { get; set; } = null;

        public DateTime? LastKlineReceived { get; set; } = null;
        public DateTime? LastTmpKlineReceived { get; set; } = null;

        public decimal? PriceTick { get; set; }
        public decimal? QuantityTick { get; set; }

        public int BasePrecision { get; set; } = 0;
        public int QuotePrecision { get; set; } = 0;

        public bool IsMonitored { get; set; }

        public DateTime? MonitoredAt { get; set; } = null;
    }
}
