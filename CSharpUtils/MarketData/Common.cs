namespace CSharpUtils.MarketData
{
    public delegate void OnNewData(PriceBar data);

    public delegate void OnUpdateData(PriceBar data);

    public delegate void OnNewPrice(IPriceQuote data);
}