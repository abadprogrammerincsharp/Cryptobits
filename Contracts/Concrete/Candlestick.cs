using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Concrete
{
    public class Candlestick
    {
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

        public override string ToString()
        {
            return $"{Symbol}, {Start}-{End}, {Open:0.00000}, {High:0.00000}, {Low:0.00000}, {Close:0.00000}";
        }
    }
}
