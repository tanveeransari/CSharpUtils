using System;
using System.Collections.Generic;

// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable

namespace CSharpUtils.MarketData
{
    public class MarketDataBarsService : IMarketDataBarsService
    {
        private readonly RandomPriceBarsDataSource _generator;
        private readonly DateTime _startDate;
        private readonly int _tickTimerIntervalms;
        private readonly int _timeFrameMinutes;

        public MarketDataBarsService(DateTime startDate, int timeFrameMinutes, int tickTimerIntervalms, double startingPx)
        {
            _startDate = startDate;
            _timeFrameMinutes = timeFrameMinutes;
            _tickTimerIntervalms = tickTimerIntervalms;
            _generator = new RandomPriceBarsDataSource(_timeFrameMinutes, true, _tickTimerIntervalms, 25, 367367, startingPx, _startDate);
        }

        public void ClearSubscriptions()
        {
            if (!_generator.IsRunning) return;
            _generator.StopGeneratePriceBars();
            _generator.ClearEventHandlers();
        }

        public IEnumerable<PriceBar> GetHistoricalData(int numberBars)
        {
            List<PriceBar> prices = new List<PriceBar>(numberBars);
            for (int i = 0; i < numberBars; i++)
            {
                prices.Add(_generator.GetNextData());
            }

            return prices;
        }

        public PriceBar GetNextBar()
        {
            return _generator.Tick();
        }

        public void SubscribePriceUpdate(Action<PriceBar> callback)
        {
            if (_generator.IsRunning) return;

            _generator.NewData += (arg) => callback?.Invoke(arg);
            _generator.UpdateData += (arg) => callback?.Invoke(arg);

            _generator.StartGeneratePriceBars();
        }
    }
}