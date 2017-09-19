﻿using System;

namespace CSharpUtils.MarketData
{
    public class PriceBar
    {
        public PriceBar()
        {
        }

        public PriceBar(DateTime date, double open, double high, double low, double close, long volume)
        {
            DateTime = date;
            Open = open;
            High = high;
            Low = low;
            Close = close;
            Volume = volume;
        }

        public double Close { get; set; }

        public DateTime DateTime { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Open { get; set; }
        public long Volume { get; set; }
    }
}