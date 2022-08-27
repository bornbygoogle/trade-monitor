using System;

namespace BlazorApp.Shared.CoreDto
{
  public class LogInfoItemDto : DtoBase
  {
    public long Id { get; set; }
    public DateTime LogTime { get; set; }

    public string AccountHolder { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public long SymbolId { get; set; }
    public string SymbolType { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string SymbolBase { get; set; } = string.Empty;
    public string SymbolQuote { get; set; } = string.Empty;

    public string LogMessage { get; set; } = string.Empty;

    public bool IsLogKline { get; set; } = false;

    //public DateTime? KlineReceived { get; set; } = null;
    public DateTime? OpenTime { get; set; } = null;
    public DateTime? CloseTime { get; set; } = null;
    public decimal Open { get; set; } = 0;
    public decimal Close { get; set; } = 0;
    public decimal Low { get; set; } = 0;
    public decimal High { get; set; } = 0;
    public decimal BaseVolume { get; set; } = 0;
    public decimal QuoteVolume { get; set; } = 0;
    public int TradeCount { get; set; } = 0;
    public decimal TakeBuyBaseVolume { get; set; } = 0;
    public decimal TakeBuyQuoteVolume { get; set; } = 0;
    public string Side { get; set; } = string.Empty;

    public int TdCountDown { get; set; } = 0;
    public decimal TdResistanceCountDown { get; set; } = 0;
    public decimal TdSupportCountDown { get; set; } = 0;

    public int TdCombo { get; set; } = 0;
    public decimal TdResistanceCombo { get; set; } = 0;
    public decimal TdSupportCombo { get; set; } = 0;

    public long BsId { get; set; }
    public DateTime? BoughtDate { get; set; } = null;

    public decimal Quantity { get; set; } = 0;

    public decimal PriceBought { get; set; } = 0;
    public decimal PriceStopLoss { get; set; } = 0;
    public decimal PriceProfit { get; set; } = 0;
    public DateTime? SoldDate { get; set; } = null;
    public decimal PriceSold { get; set; } = 0;
    public long SbsId { get; set; }

    public LogInfoItemDto()
    {
      LogTime = DateTime.Now.ToLocalTime();
      ServiceName = string.Empty;
      Symbol = string.Empty;
      LogMessage = string.Empty;
    }

    public LogInfoItemDto(string sServiceName, string sSymbol, string sMessage)
    {
      //Id = Guid.NewGuid();
      LogTime = DateTime.Now.ToLocalTime();
      ServiceName = sServiceName;
      Symbol = sSymbol;
      LogMessage = sMessage;
    }

    public LogInfoItemDto(long id, DateTime dateTime, string sServiceName, string sSymbol, string sMessage)
    {
      Id = id;
      LogTime = dateTime;
      ServiceName = sServiceName;
      Symbol = sSymbol;
      LogMessage = sMessage;
    }

    public LogInfoItemDto(long id, string sSymbolName,
                            DateTime dtOpenTime, DateTime dtCloseTime,
                            decimal dOpen, decimal dClose, decimal dLow, decimal dHigh,
                            decimal dQuoteVolume, decimal dBaseVolume,
                            int iTradeCount,
                            decimal dTakeBuyBaseVolume, decimal dTakeBuyQuoteVolume,
                            string sSide, string sTdCountDown, string sTdCombo)
    {
      Id = id;

      IsLogKline = true;
      Symbol = sSymbolName;

      OpenTime = dtOpenTime;
      CloseTime = dtCloseTime;
      Open = dOpen;
      Close = dClose;
      Low = dLow;
      High = dHigh;
      BaseVolume = dQuoteVolume;
      QuoteVolume = dBaseVolume;
      TradeCount = iTradeCount;
      TakeBuyBaseVolume = dTakeBuyBaseVolume;
      TakeBuyQuoteVolume = dTakeBuyQuoteVolume;
      Side = sSide;
      TdCountDown = string.IsNullOrEmpty(sTdCountDown) ? 0 : Convert.ToInt32(sTdCountDown);
      TdCombo = string.IsNullOrEmpty(sTdCombo) ? 0 : Convert.ToInt32(sTdCombo);
    }
  }
}
