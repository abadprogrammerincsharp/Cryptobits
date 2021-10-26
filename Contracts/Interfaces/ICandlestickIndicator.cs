using Contracts.Concrete;
using Contracts.Generics;
using Contracts.Interfaces;
using System;
using System.Threading.Tasks;

namespace Contracts.Interfaces
{
    public interface ICandlestickIndicator
    {
        ICandleFeed DataFeed { get; set; }
        ICandleLoad DataLoad { get; set; }
        CircularBuffer<IndicatorResult> Results { get; }

        event EventHandler<bool> AvailabilityChanged;
        event EventHandler IndicatorChanged;

        Task ResetFeedAsync();
        Task StartDataFeedAsync();
        void StopDataFeed();
    }
}