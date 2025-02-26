﻿using System.Globalization;

namespace BlazorApp.Shared
{
    public static class ClsUtilCommon
    {
        public const int TIMER_DURATION = 5;

        public static string FormatAsUSD(object value)
        {
            return ((double)value).ToString("#,###.#####", CultureInfo.CreateSpecificCulture("en-US"));
        }
    }
}
