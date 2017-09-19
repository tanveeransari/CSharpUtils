using System;

namespace CSharpUtils.Common
{
    public class Trade
    {
        public BuySell BuySell { get; set; }
        public double DealPrice { get; set; }
        public Instrument Instrument { get; set; }
        public int Quantity { get; set; }
        public double TotalPrice => DealPrice * Quantity;
        public DateTime TradeDate { get; set; }
    }
}