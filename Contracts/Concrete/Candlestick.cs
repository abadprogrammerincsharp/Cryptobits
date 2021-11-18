using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contracts.Extensions;

namespace Contracts.Concrete
{
    public class Candlestick
    {
        public Candlestick() { }
        public Candlestick(TradingPair tradingPair, object start, object open, object high, object low, object close, object tradeVolume, object end)
        {
            TradingPair = tradingPair;
            Symbol = tradingPair.GetUppercaseSymbolPair();
            Start = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(start));
            Open = Convert.ToDecimal(open);
            High = Convert.ToDecimal(high);
            Low = Convert.ToDecimal(low);
            Close = Convert.ToDecimal(close);
            TradeVolume = Convert.ToDecimal(tradeVolume);
            End = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(end));
        }

        public TradingPair TradingPair { get; set; }
        public string Symbol { get; set; }
        public DateTimeOffset Start { get; set; }
        public DateTimeOffset End { get; set; }
        public decimal Low { get; set; }
        public decimal High { get; set; }
        public decimal Open { get; set; }
        public decimal Close { get; set; }
        public decimal Change { get => Close - Open; }
        public decimal ChangePercent { get => Change / Open; }
        public decimal TradeVolume { get; set; }
        public bool IsOpen { get; set; }

        public override string ToString()
        {
            return $"{Symbol}, {Start}-{End}, {Open:0.00000}, {High:0.00000}, {Low:0.00000}, {Close:0.00000}";
        }
    }
}
