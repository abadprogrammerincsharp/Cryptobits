using Contracts.Concrete;
using Contracts.Generics;
using Contracts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessing.Indicators
{
    public abstract class GenericCandlestickIndicator : ICandlestickIndicator
    {
        public CircularBuffer<IndicatorResult> Results { get; private set; }
        public ICandleFeed DataFeed { get; set; }
        public ICandleLoad DataLoad { get; set; }
        public event EventHandler IndicatorChanged;
        public event EventHandler<bool> AvailabilityChanged;

        protected CircularBuffer<Candlestick> _candlesticks { get; set; }
        protected bool _dataFeedIsLive { get; set; }
        protected readonly bool _feedRequiresInitialDataLoad;
        protected readonly int _dataLoadQuantity;
        protected Action _dataInitialize;

        private const int DEFAULT_QUEUE_SIZE = 5000;

        protected GenericCandlestickIndicator(bool feedRequiresInitialLoad, int dataLoadQuantity)
        {
            _feedRequiresInitialDataLoad = feedRequiresInitialLoad;
            _dataLoadQuantity = dataLoadQuantity;
            var queueSize = Math.Max(dataLoadQuantity, DEFAULT_QUEUE_SIZE);

            Results = new CircularBuffer<IndicatorResult>(queueSize);
            _candlesticks = new CircularBuffer<Candlestick>(queueSize);
        }

        public virtual async Task StartDataFeedAsync()
        {
            if (DataFeed is null)
                throw new ArgumentNullException(nameof(DataFeed));

            if (_feedRequiresInitialDataLoad)
            {
                if (DataLoad is null)
                    throw new ArgumentNullException(nameof(DataLoad));

                var candles = await DataLoad.GetLatestCandlesAsync(_dataLoadQuantity, DataFeed.Interval);
                _candlesticks.Clear();
                _candlesticks.AddRange(candles);
                _dataInitialize?.Invoke();
            }

            DataFeed.ReceivedData += OnReceivedData;
            DataFeed.FeedAvailabilityChanged += OnDataFeedAvailabilityChanged;
            OnAvailabilityChanged(this, true);
            _dataFeedIsLive = true;
        }
        public virtual void StopDataFeed()
        {
            if (DataFeed is null)
                return;

            DataFeed.ReceivedData -= OnReceivedData;
            OnAvailabilityChanged(this, false);
        }
        public virtual async Task ResetFeedAsync()
        {
            if (_dataFeedIsLive)
                StopDataFeed();
            await StartDataFeedAsync();
        }

        protected abstract void CalculateIndicator();

        protected virtual void OnDataFeedAvailabilityChanged(object sender, bool e)
        {
            if (!e && _dataFeedIsLive)
                StopDataFeed();
        }
        protected virtual void OnReceivedData(object sender, Candlestick e)
        {
            _candlesticks.Add(e);
            CalculateIndicator();
        }
        protected virtual void OnAvailabilityChanged(object sender, bool state)
        {
            AvailabilityChanged?.Invoke(sender, state);
        }
        protected virtual void OnIndicatorUpdated(object sender)
        {
            IndicatorChanged?.Invoke(sender, new EventArgs());
        }
    }
}
