using System;
using System.Collections.Generic;

namespace CSharpUtils.MarketData
{
    public class MarketDataService : IMarketDataService
    {
        private readonly RandomPriceQuotesDataSource _generator;

        public MarketDataService(DateTime startDate, int timeFramMs, int tickTimerIntervalms, double startingPx)
        {
            if (tickTimerIntervalms > timeFramMs)
                throw new ArgumentException("timer interval must be shorter than timeFrame", nameof(tickTimerIntervalms));

            _generator = new RandomPriceQuotesDataSource(timeFramMs, true, tickTimerIntervalms, 25, 367367, startingPx, startDate);
        }

        public void ClearSubscriptions()
        {
            if (!_generator.IsRunning) return;
            _generator.StopGeneratePriceBars();
            _generator.ClearEventHandlers();
        }

        public IEnumerable<IPriceQuote> GetHistoricalData(int numberOfPrices)
        {
            List<PriceQuote> prices = new List<PriceQuote>(numberOfPrices);
            for (int i = 0; i < numberOfPrices; i++)
                prices.Add(_generator.GetNextData());

            return prices;
        }

        public IPriceQuote GetNextBar()
        {
            return _generator.Tick();
        }

        public void SubscribePriceUpdate(Action<IPriceQuote> callback)
        {
            if (_generator.IsRunning) return;

            _generator.NewData += arg => callback?.Invoke(arg);
            _generator.StartGeneratePriceBars();
        }
    }
}