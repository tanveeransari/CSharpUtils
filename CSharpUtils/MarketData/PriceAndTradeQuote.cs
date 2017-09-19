using System;

namespace CSharpUtils.MarketData
{
    public sealed class PriceAndTradeQuote : PriceQuote
    {
        public PriceAndTradeQuote()
        {
            TradePx = double.NaN;
        }

        public PriceAndTradeQuote(
            DateTime timeStamp, double bidPx, uint bidQty, double askPx, uint askQty,
            double tradePx, uint tradeQty) : base(timeStamp, bidPx, bidQty, askPx, askQty)
        {
            TradePx = tradePx;
            TradeQuantity = tradeQty;
        }

        public PriceAndTradeQuote(
            DateTime timeStamp, double bidPx, uint bidQty, double askPx, uint askQty,
            double tradePx, uint tradeQty, uint volume) : this(timeStamp, bidPx, bidQty, askPx, askQty, tradePx, tradeQty)
        {
            Volume = volume;
        }

        public double TradePx { get; }

        public uint TradeQuantity { get; }
    }
}