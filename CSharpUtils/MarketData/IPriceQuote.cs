using System;

namespace CSharpUtils.MarketData
{
    public interface IPriceQuote
    {
        double AskPx { get; }
        uint AskQuantity { get; }
        double BidPx { get; }

        uint BidQuantity { get; }

        DateTime TimeStampLocal { get; }

        DateTime TimeStampUtc { get; }

        uint Volume { get; }
    }
}