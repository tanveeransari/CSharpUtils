using System;
using System.Collections.Generic;

namespace CSharpUtils.MarketData
{
    public interface IMarketDataService
    {
        void ClearSubscriptions();

        IEnumerable<PriceBar> GetHistoricalData(int numberBars);

        PriceBar GetNextBar();

        void SubscribePriceUpdate(Action<PriceBar> callback);
    }
}