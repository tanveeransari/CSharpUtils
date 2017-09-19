using System.Collections.Generic;
using System.Linq;

namespace CSharpUtils.MarketData
{
    public class DoubleSeries : List<XYPoint>
    {
        public DoubleSeries()
        {
        }

        public DoubleSeries(int capacity) : base(capacity)
        {
        }

        public IList<double> XData => this.Select(x => x.X).ToArray();

        public IList<double> YData => this.Select(x => x.Y).ToArray();
    }
}