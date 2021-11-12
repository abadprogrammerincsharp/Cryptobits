using Contracts.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Interfaces
{
    public interface ICandleFeed : IWebsocketStreamReader, IDisposable
    {
        public event EventHandler<Candlestick> ReceivedCandlestickData;
        public event EventHandler<CandleFeedAvailabilityEvent> CandleFeedAvailabilityChanged;

        public bool TrySubscribeToCandleFeed(TradingPair tradingPair);
        public void UnsubscribeFromCandleFeed(TradingPair tradingPair);
    }
}
