using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Concrete
{
    public class TradingPair
    {
        public string QuoteAsset { get; set; }
        public string BaseAsset { get; set; }
        public TimeSpan CandlestickInterval { get; set; }
        public string Exchange { get; set; }
        public decimal MaxOrderSize { get; set; }

        public static bool operator == (TradingPair lhs, TradingPair rhs)
        {
            if (lhs is null)
                return (rhs is null);
            return lhs.Equals(rhs);
        }
        public static bool operator !=(TradingPair lhs, TradingPair rhs) => !(lhs == rhs);

        public override bool Equals(object obj)
        {      
            return obj != null &&
                   obj is TradingPair pair &&
                   QuoteAsset == pair.QuoteAsset &&
                   BaseAsset == pair.BaseAsset &&
                   CandlestickInterval.Equals(pair.CandlestickInterval) &&
                   Exchange == pair.Exchange;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(QuoteAsset, BaseAsset, CandlestickInterval, Exchange);
        }
    }
}
