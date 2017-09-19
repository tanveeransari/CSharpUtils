using System;

namespace CSharpUtils.MarketData
{
    public class PriceQuote : IPriceQuote
    {
        public PriceQuote()
        {
            AskPx = double.NaN;
            BidPx = double.NaN;
            TimeStampLocal = DateTime.Now;
        }

        public PriceQuote(DateTime timeStamp)
        {
            TimeStampLocal = timeStamp;
        }

        public PriceQuote(DateTime timeStamp, double bidPx, uint bidQty, double askPx, uint askQty) : this(timeStamp)
        {
            BidPx = bidPx;
            AskPx = askPx;
            BidQuantity = bidQty;
            AskQuantity = askQty;
        }

        public PriceQuote(DateTime timeStamp, double bidPx, uint bidQty, double askPx, uint askQty, uint volume) : this(
            timeStamp, bidPx, bidQty, askPx, askQty)
        {
            Volume = volume;
        }

        public double AskPx { get; }

        public uint AskQuantity { get; }

        public double BidPx { get; }

        public uint BidQuantity { get; }

        public DateTime TimeStampLocal { get; }
        public DateTime TimeStampUtc => TimeStampLocal.ToUniversalTime();
        public uint Volume { get; protected set; }

        public override string ToString()
        {
            return $" {TimeStampLocal:yy/MM/dd HH:mm:ss:fff}  Bid [{BidQuantity}@{BidPx:N2}]  Offer [{AskQuantity}@{AskPx:N2}]";
        }

    }
}