using System;
using System.Collections.Generic;

namespace CSharpUtils.MarketData
{
    public interface IMarketDataService
    {
        void ClearSubscriptions();

        IEnumerable<IPriceQuote> GetHistoricalData(int numberOfPrices);

        IPriceQuote GetNextBar();

        void SubscribePriceUpdate(Action<IPriceQuote> callback);
    }
}