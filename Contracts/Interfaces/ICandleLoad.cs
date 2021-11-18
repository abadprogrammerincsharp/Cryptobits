using Contracts.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Interfaces
{
    public interface ICandleLoad : IExchangeApi
    {
        public Task<IEnumerable<Candlestick>> GetLatestCandlesAsync(TradingPair tradingPair, int quantity);
        public bool CanQueryCandlestickData();
        public int MinCandleQuantity { get; }
        public int MaxCandleQuantity { get; }
    }
}
