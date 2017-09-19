using System;
using System.Timers;

namespace CSharpUtils.MarketData
{
    public class RandomPriceQuotesDataSource
    {
        private const int STARTING_QTY = 20;
        private readonly int _candleIntervalMs;
        private readonly TimeSpan _closeMarketTime = new TimeSpan(0, 16, 30, 0);
        private readonly PriceQuoteInfo _initialPriceBar;

        private readonly TimeSpan _openMarketTime = new TimeSpan(0, 08, 0, 0);

        private readonly Random _random;
        private readonly bool _simulateDateGap;
        private readonly Timer _timer;
        private double _currentTime;
        private PriceQuote _lastPrice;

        public RandomPriceQuotesDataSource(
            int candleIntervalMs,
            bool simulateDateGap,
            double timerInterval,
            int updatesPerPrice,
            int randomSeed,
            double startingPrice,
            DateTime startDate
        )
        {
            _candleIntervalMs = candleIntervalMs;
            _simulateDateGap = simulateDateGap;

            _timer = new Timer(timerInterval)
            {
                Enabled = false,
                AutoReset = true
            };
            _timer.Elapsed += TimerElapsed;
            _initialPriceBar = new PriceQuoteInfo
            {
                Price = startingPrice,
                DateTime = startDate
            };

            _lastPrice = new PriceQuote(
                _initialPriceBar.DateTime,
                startingPrice,
                STARTING_QTY,
                Math.Max(startingPrice * 1.01, startingPrice + 0.01),
                STARTING_QTY,
                0);
            _random = new Random(randomSeed);
        }

        public bool IsRunning => _timer.Enabled;

        public event OnNewPrice NewData;

        public void StartGeneratePriceBars()
        {
            _timer.Enabled = true;
        }

        public void StopGeneratePriceBars()
        {
            _timer.Enabled = false;
        }

        public PriceQuote GetNextData()
        {
            PriceQuote nextRandomPriceBar = GetNextRandomPriceQuote();
            return nextRandomPriceBar;
        }

        private PriceQuote GetNextRandomPriceQuote()
        {
            double close = _lastPrice.BidPx;
            double num = (_random.NextDouble() - 0.9) * _initialPriceBar.Price / 30.0;
            double num2 = _random.NextDouble();
            double num3 = _initialPriceBar.Price +
                          _initialPriceBar.Price / 2.0 * Math.Sin(7.27220521664304E-06 * _currentTime) +
                          _initialPriceBar.Price / 16.0 * Math.Cos(7.27220521664304E-05 * _currentTime) +
                          _initialPriceBar.Price / 32.0 * Math.Sin(7.27220521664304E-05 * (10.0 + num2) * _currentTime) +
                          _initialPriceBar.Price / 64.0 * Math.Cos(7.27220521664304E-05 * (20.0 + num2) * _currentTime) +
                          num;
            double num4 = Math.Max(close, num3);
            double num5 = _random.NextDouble() * _initialPriceBar.Price / 100.0;
            double high = num4 + num5;
            double num6 = Math.Min(close, num3);
            double num7 = _random.NextDouble() * _initialPriceBar.Price / 100.0;
            double low = num6 - num7;
            long volume = (long) (_random.NextDouble() * 30000 + 20000);
            DateTime openTime = _simulateDateGap ? EmulateDateGap(_lastPrice.TimeStampLocal) : _lastPrice.TimeStampLocal;
            DateTime closeTime = openTime.AddMilliseconds(_candleIntervalMs);
            var candle = new PriceQuote(closeTime, low, (uint) _random.Next(10, 30), high, (uint) _random.Next(10, 30), (uint) volume);

            _lastPrice = candle;
            _currentTime += _candleIntervalMs * 60;
            return candle;
        }

        private DateTime EmulateDateGap(DateTime candleOpenTime)
        {
            DateTime result = candleOpenTime;
            TimeSpan timeOfDay = candleOpenTime.TimeOfDay;
            if (timeOfDay > _closeMarketTime)
            {
                DateTime dateTime = candleOpenTime.Date;
                dateTime = dateTime.AddDays(1.0);
                result = dateTime.Add(_openMarketTime);
            }
            while (result.DayOfWeek == DayOfWeek.Saturday || result.DayOfWeek == DayOfWeek.Sunday)
                result = result.AddDays(1.0);
            return result;
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            OnTimerElapsed();
        }

        private void OnTimerElapsed()
        {
            PriceQuote nextData = GetNextData();
            NewData?.Invoke(nextData);
        }

        public void ClearEventHandlers()
        {
            NewData = null;
        }

        public PriceQuote Tick()
        {
            return GetNextData();
        }

        private sealed class PriceQuoteInfo
        {
            public DateTime DateTime;
            public double Price;
        }
    }
}