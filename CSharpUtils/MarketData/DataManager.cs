using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using CSharpUtils.Common;

namespace CSharpUtils.MarketData
{
    public interface IDataManager
    {
        IEnumerable<Instrument> AvailableInstruments { get; }
        IEnumerable<TimeFrame> GetAvailableTimeFrames(Instrument forInstrument);
        DoubleSeries GetFourierSeries(double amplitude, double phaseShift, int count = 5000);
        PriceSeries GetPriceData(string symbol, TimeFrame timeFrame);
        DoubleSeries GetSquirlyWave();
    }

    public class DataManager : IDataManager
    {
        private readonly List<DoubleSeries> _acousticPlotData = new List<DoubleSeries>();
        private readonly IDictionary<string, PriceSeries> _dataSets = new Dictionary<string, PriceSeries>();
        private readonly Random _random = new Random();
        private IList<Instrument> _availableInstruments;
        private IDictionary<Instrument, IList<TimeFrame>> _availableTimeFrames;
        public static DataManager Instance { get; } = new DataManager();

        public IEnumerable<Instrument> AvailableInstruments
        {
            get
            {
                if (_availableInstruments == null)
                    lock (typeof(DataManager))
                    {
                        if (_availableInstruments == null)
                        {
                            Assembly assembly = typeof(DataManager).Assembly;
                            _availableInstruments = new List<Instrument>();

                            foreach (string resourceString in assembly.GetManifestResourceNames())
                                if (resourceString.Contains("_"))
                                {
                                    string instrumentString = GetSubstring(resourceString, ResourceDirectory + ".", "_");
                                    Instrument instr = Instrument.Parse(instrumentString);
                                    if (!_availableInstruments.Contains(instr))
                                        _availableInstruments.Add(instr);
                                }
                        }
                    }

                return _availableInstruments;
            }
        }

        private static string ResourceDirectory => "Abt.Controls.SciChart.Example.Resources";

        public IList<double> ComputeMovingAverage(IList<double> prices, int length)
        {
            double[] result = new double[prices.Count];
            for (int i = 0; i < prices.Count; i++)
            {
                if (i < length)
                {
                    result[i] = double.NaN;
                    continue;
                }

                result[i] = AverageOf(prices, i - length, i);
            }

            return result;
        }

        public DoubleSeries GenerateEEG(int count, ref double startPhase, double phaseStep)
        {
            var doubleSeries = new DoubleSeries();
            var rand = new Random((int)DateTime.Now.Ticks);

            for (int i = 0; i < count; i++)
            {
                var xyPoint = new XyPoint();

                double time = i / (double)count;
                xyPoint.X = time;
                //double mod = 0.2 * Math.Sin(startPhase);
                xyPoint.Y = //mod * Math.Sin(startPhase / 4.9) +
                    0.05 * (rand.NextDouble() - 0.5) +
                    1.0;

                doubleSeries.Add(xyPoint);
                startPhase += phaseStep;
            }

            return doubleSeries;
        }

        public DoubleSeries GenerateSpiral(double xCentre, double yCentre, double maxRadius, int count)
        {
            var doubleSeries = new DoubleSeries();
            double radius = 0;
            double x, y;
            double deltaRadius = maxRadius / count;
            for (int i = 0; i < count; i++)
            {
                double sinX = Math.Sin(2 * Math.PI * i * 0.05);
                double cosX = Math.Cos(2 * Math.PI * i * 0.05);
                x = xCentre + radius * sinX;
                y = yCentre + radius * cosX;
                doubleSeries.Add(new XyPoint { X = x, Y = y });
                radius += deltaRadius;
            }
            return doubleSeries;
        }

        public DoubleSeries GetAcousticChannel(int channelNumber)
        {
            if (channelNumber > 7)
                throw new InvalidOperationException("Only channels 0-7 allowed");

            if (_acousticPlotData.Count != 0)
                return _acousticPlotData[channelNumber];

            // e.g. resource format: Abt.Controls.SciChart.Example.Resources.EURUSD_Daily.csv
            string csvResource = $"{ResourceDirectory}.AcousticPlots.csv";

            var ch0 = new DoubleSeries(100000);
            var ch1 = new DoubleSeries(100000);
            var ch2 = new DoubleSeries(100000);
            var ch3 = new DoubleSeries(100000);
            var ch4 = new DoubleSeries(100000);
            var ch5 = new DoubleSeries(100000);
            var ch6 = new DoubleSeries(100000);
            var ch7 = new DoubleSeries(100000);

            Assembly assembly = typeof(DataManager).Assembly;
            using (Stream stream = assembly.GetManifestResourceStream(csvResource))
            using (var streamReader = new StreamReader(stream))
            {
                string line = streamReader.ReadLine();
                line = streamReader.ReadLine();
                while (line != null)
                {
                    // Line Format:
                    // Date, Open, High, Low, Close, Volume
                    // 2007.07.02 03:30, 1.35310, 1.35310, 1.35280, 1.35310, 12
                    string[] tokens = line.Split(',');
                    double x = double.Parse(tokens[0], NumberFormatInfo.InvariantInfo);
                    double y0 = double.Parse(tokens[1], NumberFormatInfo.InvariantInfo);
                    double y1 = double.Parse(tokens[2], NumberFormatInfo.InvariantInfo);
                    double y2 = double.Parse(tokens[3], NumberFormatInfo.InvariantInfo);
                    double y3 = double.Parse(tokens[4], NumberFormatInfo.InvariantInfo);
                    double y4 = double.Parse(tokens[5], NumberFormatInfo.InvariantInfo);
                    double y5 = double.Parse(tokens[6], NumberFormatInfo.InvariantInfo);
                    double y6 = double.Parse(tokens[7], NumberFormatInfo.InvariantInfo);
                    double y7 = double.Parse(tokens[8], NumberFormatInfo.InvariantInfo);

                    ch0.Add(new XyPoint { X = x, Y = y0 });
                    ch1.Add(new XyPoint { X = x, Y = y1 });
                    ch2.Add(new XyPoint { X = x, Y = y2 });
                    ch3.Add(new XyPoint { X = x, Y = y3 });
                    ch4.Add(new XyPoint { X = x, Y = y4 });
                    ch5.Add(new XyPoint { X = x, Y = y5 });
                    ch6.Add(new XyPoint { X = x, Y = y6 });
                    ch7.Add(new XyPoint { X = x, Y = y7 });

                    line = streamReader.ReadLine();
                }
            }

            _acousticPlotData.AddRange(new[] { ch0, ch1, ch2, ch3, ch4, ch5, ch6, ch7 });

            return _acousticPlotData[channelNumber];
        }

        public IEnumerable<TimeFrame> GetAvailableTimeFrames(Instrument forInstrument)
        {
            if (_availableTimeFrames == null)
                lock (typeof(DataManager))
                {
                    if (_availableTimeFrames == null)
                    {
                        // Initialise the Timeframe dictionary
                        _availableTimeFrames = new Dictionary<Instrument, IList<TimeFrame>>();
                        foreach (Instrument instr in AvailableInstruments)
                            _availableTimeFrames[instr] = new List<TimeFrame>();

                        Assembly assembly = typeof(DataManager).Assembly;

                        foreach (string resourceString in assembly.GetManifestResourceNames())
                            if (resourceString.Contains("_"))
                            {
                                Instrument instrument = Instrument.Parse(GetSubstring(resourceString, ResourceDirectory + ".", "_"));
                                TimeFrame timeframe = TimeFrame.Parse(GetSubstring(resourceString, "_", ".csv"));

                                _availableTimeFrames[instrument].Add(timeframe);
                            }
                    }
                }

            return _availableTimeFrames[forInstrument];
        }

        public DoubleSeries GetButterflyCurve(int count = 2000)
        {
            // From http://en.wikipedia.org/wiki/Butterfly_curve_%28transcendental%29
            // x = sin(t) * (e^cos(t) - 2cos(4t) - sin^5(t/12))
            // y = cos(t) * (e^cos(t) - 2cos(4t) - sin^5(t/12))
            double temp = 0.01;
            var doubleSeries = new DoubleSeries(count);
            for (int i = 0; i < count; i++)
            {
                double t = i * temp;

                double multiplier = Math.Pow(Math.E, Math.Cos(t)) - 2 * Math.Cos(4 * t) - Math.Pow(Math.Sin(t / 12), 5);

                double x = Math.Sin(t) * multiplier;
                double y = Math.Cos(t) * multiplier;
                doubleSeries.Add(new XyPoint { X = x, Y = y });
            }
            return doubleSeries;
        }

        public DoubleSeries GetClusteredPoints(double xCentre, double yCentre, double deviation, int count)
        {
            var doubleSeries = new DoubleSeries();
            for (int i = 0; i < count; i++)
            {
                double x = GetGaussianRandomNumber(xCentre, deviation);
                double y = GetGaussianRandomNumber(yCentre, deviation);
                doubleSeries.Add(new XyPoint { X = x, Y = y });
            }
            return doubleSeries;
        }

        public DoubleSeries GetDampedSinewave(double amplitude, double dampingFactor, int pointCount, int freq = 10)
        {
            return GetDampedSinewave(0, amplitude, 0.0, dampingFactor, pointCount, freq);
        }

        public DoubleSeries GetDampedSinewave(int pad, double amplitude, double phase, double dampingFactor, int pointCount, int freq = 10)
        {
            var doubleSeries = new DoubleSeries();

            for (int i = 0; i < pad; i++)
            {
                double time = 10 * i / (double)pointCount;
                doubleSeries.Add(new XyPoint { X = time });
            }

            for (int i = pad, j = 0; i < pointCount; i++, j++)
            {
                var xyPoint = new XyPoint();

                double time = 10 * i / (double)pointCount;
                double wn = 2 * Math.PI / (pointCount / (double)freq);

                xyPoint.X = time;
                xyPoint.Y = amplitude * Math.Sin(j * wn + phase);
                doubleSeries.Add(xyPoint);

                amplitude *= 1.0 - dampingFactor;
            }

            return doubleSeries;
        }

        public DoubleSeries GetExponentialCurve(double power, int pointCount)
        {
            var doubleSeries = new DoubleSeries(pointCount);

            double x = 0.00001;
            const double FUDGE_FACTOR = 1.4;
            for (int i = 0; i < pointCount; i++)
            {
                x *= FUDGE_FACTOR;
                double y = Math.Pow((double)i + 1, power);
                doubleSeries.Add(new XyPoint { X = x, Y = y });
            }

            return doubleSeries;
        }

        public DoubleSeries GetFourierSeries(double amplitude, double phaseShift, int count = 5000)
        {
            var doubleSeries = new DoubleSeries();

            for (int i = 0; i < count; i++)
            {
                var xyPoint = new XyPoint();

                double time = 10 * i / (double)count;
                double wn = 2 * Math.PI / (count / 10);

                xyPoint.X = time;
                xyPoint.Y = Math.PI *
                            amplitude *
                            (Math.Sin(i * wn + phaseShift) +
                             0.33 * Math.Sin(i * 3 * wn + phaseShift) +
                             0.20 * Math.Sin(i * 5 * wn + phaseShift) +
                             0.14 * Math.Sin(i * 7 * wn + phaseShift) +
                             0.11 * Math.Sin(i * 9 * wn + phaseShift) +
                             0.09 * Math.Sin(i * 11 * wn + phaseShift));
                doubleSeries.Add(xyPoint);
            }

            return doubleSeries;
        }

        public DoubleSeries GetFourierSeriesZoomed(double amplitude, double phaseShift, double xStart, double xEnd, int count = 5000)
        {
            DoubleSeries data = GetFourierSeries(amplitude, phaseShift, count);

            int index0 = 0;
            int index1 = 0;
            for (int i = 0; i < count; i++)
            {
                if (data.XData[i] > xStart && index0 == 0)
                    index0 = i;

                if (data.XData[i] > xEnd && index1 == 0)
                {
                    index1 = i;
                    break;
                }
            }

            var result = new DoubleSeries();

            double[] xData = data.XData.Skip(index0).Take(index1 - index0).ToArray();
            double[] yData = data.YData.Skip(index0).Take(index1 - index0).ToArray();

            for (int i = 0; i < xData.Length; i++)
                result.Add(new XyPoint { X = xData[i], Y = yData[i] });

            return result;
        }

        public double GetGaussianRandomNumber(double mean, double stdDev)
        {
            double u1 = _random.NextDouble(); //these are uniform(0,1) random doubles
            double u2 = _random.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                                   Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            double randNormal =
                mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
            return randNormal;
        }

        public DoubleSeries GetLissajousCurve(double alpha, double beta, double delta, int count = 200)
        {
            // From http://en.wikipedia.org/wiki/Lissajous_curve
            // x = Asin(at + d), y = Bsin(bt)
            var doubleSeries = new DoubleSeries(count);
            for (int i = 0; i < count; i++)
            {
                double x = Math.Sin(alpha * i * 0.1 + delta);
                double y = Math.Sin(beta * i * 0.1);
                doubleSeries.Add(new XyPoint { X = x, Y = y });
            }
            return doubleSeries;
        }

        public DoubleSeries GetNoisySinewave(double amplitude, double phase, int pointCount, double noiseAmplitude)
        {
            DoubleSeries sinewave = GetSinewave(amplitude, phase, pointCount);

            // Add some noise
            for (int i = 0; i < pointCount; i++)
                sinewave[i].Y += _random.NextDouble() * noiseAmplitude - noiseAmplitude * 0.5;

            return sinewave;
        }

        public PriceSeries GetPriceData(string symbol, TimeFrame timeFrame)
        {
            return GetPriceData($"{symbol}_{timeFrame}");
        }
        public PriceSeries GetPriceData(string dataset)
        {
            if (_dataSets.ContainsKey(dataset))
                return _dataSets[dataset];

            // e.g. resource format: Abt.Controls.SciChart.Example.Resources.EURUSD_Daily.csv
            string csvResource = $"{ResourceDirectory}.{Path.ChangeExtension(dataset, "csv")}";

            var priceSeries = new PriceSeries
            {
                Symbol = dataset
            };

            Assembly assembly = typeof(DataManager).Assembly;
            // Debug.WriteLine(string.Join(", ", assembly.GetManifestResourceNames()));
            using (Stream stream = assembly.GetManifestResourceStream(csvResource))
            {
                Debug.Assert(stream != null, "stream != null");
                using (var streamReader = new StreamReader(stream))
                {
                    string line = streamReader.ReadLine();
                    while (line != null)
                    {
                        var priceBar = new PriceBar();
                        // Line Format:
                        // Date, Open, High, Low, Close, Volume
                        // 2007.07.02 03:30, 1.35310, 1.35310, 1.35280, 1.35310, 12
                        string[] tokens = line.Split(',');
                        priceBar.DateTime = DateTime.Parse(tokens[0], DateTimeFormatInfo.InvariantInfo);
                        priceBar.Open = double.Parse(tokens[1], NumberFormatInfo.InvariantInfo);
                        priceBar.High = double.Parse(tokens[2], NumberFormatInfo.InvariantInfo);
                        priceBar.Low = double.Parse(tokens[3], NumberFormatInfo.InvariantInfo);
                        priceBar.Close = double.Parse(tokens[4], NumberFormatInfo.InvariantInfo);
                        priceBar.Volume = long.Parse(tokens[5], NumberFormatInfo.InvariantInfo);
                        priceSeries.Add(priceBar);

                        line = streamReader.ReadLine();
                    }
                }
            }

            _dataSets.Add(dataset, priceSeries);

            return priceSeries;
        }

        public Color GetRandomColor()
        {
            return Color.FromArgb(0xFF, (byte)_random.Next(255), (byte)_random.Next(255), (byte)_random.Next(255));
        }

        public DoubleSeries GetRandomDoubleSeries(int pointCount)
        {
            var doubleSeries = new DoubleSeries();
            double amplitude = _random.NextDouble() + 0.5;
            double freq = Math.PI * (_random.NextDouble() + 0.5) * 10;
            double offset = _random.NextDouble() - 0.5;

            for (int i = 0; i < pointCount; i++)
                doubleSeries.Add(new XyPoint { X = i, Y = offset + amplitude * Math.Sin(freq * i) });

            return doubleSeries;
        }

        public PriceSeries GetRandomTrades(out List<Trade> trades, out List<NewsEvent> news)
        {
            var priceSeries = new PriceSeries();
            trades = new List<Trade>();
            news = new List<NewsEvent>();

            var startDate = new DateTime(2012, 01, 01);

            double randomWalk = 0.0;

            // Note: Change the value below to increase or decrease the point count and trade frequency
            const int COUNT = 1000;
            const uint TRADE_FREQUENCY = 14;

            // Generate the X,Y data with sequential dates on the X-Axis and slightly positively biased random walk on the Y-Axis
            for (int i = 0; i < COUNT; i++)
            {
                randomWalk += _random.NextDouble() - 0.498;
                priceSeries.Add(new PriceBar(startDate.AddMinutes(i * 10), randomWalk, randomWalk, randomWalk, randomWalk, 0));
            }

            // The random walk is a truly random series, so it may contain negative values. Here we find the minimum and offset it
            // so it is always positive.
            double yOffset = -priceSeries.CloseData.Min() + _random.NextDouble();

            for (int i = 0; i < COUNT; i++)
            {
                // Now update with the offset so it is never negative
                priceSeries[i].Close += yOffset;

                // Every N'th tick create a random trade
                if (i % TRADE_FREQUENCY == 0)
                {
                    var trade = new Trade
                    {
                        // randomize buy or sell
                        BuySell = _random.NextDouble() > 0.48 ? BuySell.Buy : BuySell.Sell,

                        // Set dealprice and date
                        DealPrice = priceSeries[i].Close,
                        TradeDate = priceSeries[i].DateTime,

                        // Set instrument and quantity
                        Instrument = Instrument.CrudeOil,
                        Quantity = _random.Next(100, 500)
                    };
                    trades.Add(trade);
                }

                // Every N'th tick create a random news event
                if (_random.Next(0, 99) > 95)
                {
                    var newsEvent = new NewsEvent
                    {
                        EventDate = priceSeries[i].DateTime,
                        Headline = "OPEC meeting minutes",
                        Body =
                            "The Organization of the Petroleum Exporting Countries voted today to increase production of Crude oil from its member states"
                    };

                    news.Add(newsEvent);
                }
            }

            return priceSeries;
        }

        public DoubleSeries GetSinewave(double amplitude, double phase, int pointCount, int freq = 10)
        {
            return GetDampedSinewave(0, amplitude, phase, 0.0, pointCount, freq);
        }
        public DoubleSeries GetSquirlyWave()
        {
            var doubleSeries = new DoubleSeries();
            var rand = new Random((int)DateTime.Now.Ticks);

            const int COUNT = 1000;
            for (int i = 0; i < COUNT; i++)
            {
                var xyPoint = new XyPoint();

                double time = i / (double)COUNT;
                xyPoint.X = time;
                xyPoint.Y = time * Math.Sin(2 * Math.PI * i / COUNT) +
                            0.2 * Math.Sin(2 * Math.PI * i / (COUNT / 7.9)) +
                            0.05 * (rand.NextDouble() - 0.5) +
                            1.0;

                doubleSeries.Add(xyPoint);
            }

            return doubleSeries;
        }
        public DoubleSeries GetStraightLine(double gradient, double yIntercept, int pointCount)
        {
            var doubleSeries = new DoubleSeries(pointCount);

            for (int i = 0; i <= pointCount; i++)
            {
                double x = i + 1;
                double y = gradient * x + yIntercept;
                doubleSeries.Add(new XyPoint { X = x, Y = y });
            }

            return doubleSeries;
        }

        public IEnumerable<double> Offset(IList<double> inputList, double offset)
        {
            foreach (double value in inputList)
                yield return value + offset;
        }

        private static double AverageOf(IList<double> prices, int from, int to)
        {
            double result = 0.0;
            for (int i = from; i < to; i++)
                result += prices[i];

            return result / (to - from);
        }

        private string GetSubstring(string input, string before, string after)
        {
            int beforeIndex = string.IsNullOrEmpty(before) ? 0 : input.IndexOf(before) + before.Length;
            int afterIndex = string.IsNullOrEmpty(after) ? input.Length : input.IndexOf(after) - beforeIndex;
            return input.Substring(beforeIndex, afterIndex);
        }
    }
}