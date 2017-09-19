using System.Globalization;
using System.Linq;

namespace CSharpUtils.Common
{
    public enum BuySell
    {
        Buy = 1,
        Sell = -1
    }

    public class TimeFrame : StrongTyped<string>
    {
        public static readonly TimeFrame Daily = new TimeFrame("Daily", "Daily");

        public static readonly TimeFrame Hourly = new TimeFrame("Hourly", "1 Hour");

        public static readonly TimeFrame Minute15 = new TimeFrame("Minute15", "15 Minutes");

        public TimeFrame(string value, string displayname) : base(value)
        {
            Displayname = displayname;
        }
        public string Displayname { get; }

        public static TimeFrame Parse(string input)
        {
            return new[] { Minute15, Hourly, Daily }.Single(x => x.Value.ToUpper(CultureInfo.InvariantCulture) == input.ToUpper(CultureInfo.InvariantCulture));
        }
    }
}