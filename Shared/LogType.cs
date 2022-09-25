using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorApp.Shared
{
    public enum LogType
    {
        Info,
        InfoService,
        Error,
        ErrorService,
        Technic,
        Trading,
        TradingBuy,
        TradingSell,
        TradingError,
        TradingROE,

        UserBuy,
        UserSell,

        ReceiveKline,
        KlineRefreshed,

        LogKline,
        LogKlineCombo,
        LogKlineCountDown,
        LogTmpKline,
        PredictKline,
        ResultPredictKline,

        ClearListCountDown,
        ClearListCombo,
        ClearListTmpKline,

        EnterShort,
        ExitShort,

        BaseBuy,
        BaseSell,
    }
}
