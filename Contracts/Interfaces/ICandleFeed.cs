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
        TimeSpan Interval { get; set; }
        public event EventHandler<FeedAvailibilityEvent> FeedAvailabilityChanged;
        public event EventHandler<Candlestick> RecievedCandlestickData;
        public ILogger Log { get; set; }

        public bool TrySubscribeToCandleFeed(TradingPair tradingPair);
        public void UnsubscribeFromCandleFeed(TradingPair tradingPair);
    }
}
