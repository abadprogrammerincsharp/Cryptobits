using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Concrete
{
    public class TradingPair
    {
        public string BaseAssetSymbol { get; set; }
        public string QuoteAssetSymbol { get; set; }
        public TimeSpan CandlestickInterval { get; set; }
        public string Exchange { get; set; }

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
                   BaseAssetSymbol == pair.BaseAssetSymbol &&
                   QuoteAssetSymbol == pair.QuoteAssetSymbol &&
                   CandlestickInterval.Equals(pair.CandlestickInterval) &&
                   Exchange == pair.Exchange;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(BaseAssetSymbol, QuoteAssetSymbol, CandlestickInterval, Exchange);
        }
    }
}
