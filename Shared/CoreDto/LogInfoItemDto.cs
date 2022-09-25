using Binance.Net.Enums;
using System;

namespace BlazorApp.Shared.CoreDto
{
    public class LogInfoItemDto : DtoBase
    {
        public long Id { get; set; }

        public DateTime LogTime { get; set; }

        public string AccountHolder { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;


        public string Service { get; set; } = string.Empty;
        public LogType LogType { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public long SymbolId { get; set; }

        public string SymbolBase { get; set; } = string.Empty;
        public string SymbolQuote { get; set; } = string.Empty;

        public long KlineId { get; set; }
        public KlineInterval Interval { get; set; }
        public bool IsLogKline { get; set; } = false;
        public DateTime? KlineReceived { get; set; } = null;
        public DateTime? OpenTime { get; set; } = null;
        public DateTime? CloseTime { get; set; } = null;
        public decimal Open { get; set; } = 0;
        public decimal Close { get; set; } = 0;
        public decimal Low { get; set; } = 0;
        public decimal High { get; set; } = 0;
        ///Heikin-Ashi kline
        ///Close = (Open(0) + High(0) + Low(0) + Close(0)) /4
        ///Open = ((HA)Open(-1) + (HA)Close(-1)) / 2
        ///High = Max(High(0), (HA)Open(0), (HA)Close(0))
        ///Low = Min(Low(0), (HA)Open(0), (HA)Close(0))
        ///  0 : Values from the current period
        /// -1 : Values from the prior period
        /// HA : Heikin-Ashi

        public decimal HAOpen { get; set; }
        public decimal HAClose { get; set; }
        public decimal HALow { get; set; }
        public decimal HAHigh { get; set; }
        public decimal BaseVolume { get; set; } = 0;
        public decimal QuoteVolume { get; set; } = 0;
        public int TradeCount { get; set; } = 0;
        public decimal TakerBaseVolume { get; set; } = 0;
        public decimal TakerQuoteVolume { get; set; } = 0;
        public string Side { get; set; } = string.Empty;
        public int TdCountDown { get; set; } = 0;
        public decimal TdResistanceCountDown { get; set; } = 0;
        public decimal TdSupportCountDown { get; set; } = 0;
        public int TdCombo { get; set; } = 0;
        public decimal TdResistanceCombo { get; set; } = 0;
        public decimal TdSupportCombo { get; set; } = 0;
        public string Prediction { get; set; } = string.Empty;
        public string PredictionResult { get; set; } = string.Empty;
        public bool IsFinal { get; set; } = false;
        public bool IsRefreshed { get; set; } = false;

        public long BsId { get; set; }
        public DateTime? BoughtDate { get; set; } = null;
        public decimal Quantity { get; set; } = 0;
        public decimal PriceBought { get; set; }
        public decimal PriceStopLoss { get; set; }
        public decimal PriceProfit { get; set; }
        public DateTime? SoldDate { get; set; } = null;
        public decimal PriceSold { get; set; }
        public long SbsId { get; set; }

        public LogInfoItemDto()
        {
            LogTime = DateTime.Now.ToLocalTime();
            Service = string.Empty;
            Symbol = string.Empty;
            Message = string.Empty;
        }

        public LogInfoItemDto(string sServiceName, string sSymbol, string sMessage)
        {
            //Id = Guid.NewGuid();
            LogTime = DateTime.Now.ToLocalTime();
            Service = sServiceName;
            Symbol = sSymbol;
            Message = sMessage;
        }

        public LogInfoItemDto(long id, DateTime dateTime, string sServiceName, string sSymbol, string sMessage)
        {
            Id = id;
            LogTime = dateTime;
            Service = sServiceName;
            Symbol = sSymbol;
            Message = sMessage;
        }

        public LogInfoItemDto(long id, string sSymbolName,
                                DateTime openTime, DateTime closeTime,
                                decimal open, decimal close, decimal low, decimal high,
                                decimal quoteVolume, decimal baseVolume,
                                int tradeCount,
                                decimal takerBaseVolume, decimal takerQuoteVolume,
                                string sSide, string sTdCountDown, string sTdCombo)
        {
            Id = id;

            IsLogKline = true;
            Symbol = sSymbolName;

            OpenTime = openTime;
            CloseTime = closeTime;
            Open = open;
            Close = close;
            Low = low;
            High = high;
            BaseVolume = baseVolume;
            QuoteVolume = quoteVolume;
            TradeCount = tradeCount;
            TakerBaseVolume = takerBaseVolume;
            TakerQuoteVolume = takerQuoteVolume;
            Side = sSide;
            TdCountDown = string.IsNullOrEmpty(sTdCountDown) ? 0 : Convert.ToInt32(sTdCountDown);
            TdCombo = string.IsNullOrEmpty(sTdCombo) ? 0 : Convert.ToInt32(sTdCombo);
        }
    }
}
