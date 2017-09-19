using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using CSharpUtils.Common;

namespace CSharpUtils.MarketData
{
    public interface IPriceDataService
    {
        IEnumerable<PriceBar> GetHistoricalData();

        void SubscribePriceUpdate(Action<PriceBar> callback);
    }

    public class PriceDataService : IPriceDataService, IDisposable
    {
        private readonly Instrument _instrument;
        private readonly object _lockObject = new object();
        private readonly DateTime _startDate;
        private readonly TimeFrame _timeframe;
        private int _currentIndex;
        private bool _disposed;
        private DateTime _endDate;
        private Action<PriceBar> _priceUpdateCallback;
        private Timer _timer;
        public PriceDataService(
            Instrument instrument,
            TimeFrame timeframe,
            DateTime startDate,
            DateTime endDate)
        {
            _instrument = instrument;
            _timeframe = timeframe;
            _startDate = startDate;
            _endDate = endDate;
        }

#pragma warning disable CC0029 // Disposables Should Call Suppress Finalize
        public void Dispose() => Dispose(true);
#pragma warning restore CC0029 // Disposables Should Call Suppress Finalize

        public IEnumerable<PriceBar> GetHistoricalData()
        {
            var priceSeries = DataManager.Instance.GetPriceData(_instrument.Symbol + "_" + _timeframe.Value);
            _currentIndex = priceSeries.IndexOf(priceSeries.FirstOrDefault(x => x.DateTime > _endDate));
            return priceSeries.TakeWhile(x => x.DateTime > _startDate && x.DateTime < _endDate);
        }

        public void SubscribePriceUpdate(Action<PriceBar> callback)
        {
            _priceUpdateCallback = callback;
            StartTimer();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _timer.Dispose();
                }

                _disposed = true;
            }
        }

        private PriceBar Next(PriceSeries priceSeries)
        {
            return priceSeries[_currentIndex++];
        }

        private void StartTimer()
        {
            StopTimer();

            _timer = new Timer(10);
            _timer.Elapsed += TimerOnElapsed;
            _timer.Start();
        }

        private void StopTimer()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Elapsed -= TimerOnElapsed;
                _timer = null;
            }
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            lock (_lockObject)
            {
                var priceSeries = DataManager.Instance.GetPriceData(_instrument.Symbol + "_" + _timeframe.Value);
                _priceUpdateCallback?.Invoke(Next(priceSeries));
            }
        }
    }
}