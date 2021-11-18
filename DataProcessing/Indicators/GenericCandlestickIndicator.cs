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
        private TradingPair _tradingPair;

        protected GenericCandlestickIndicator(TradingPair tradingPair, bool feedRequiresInitialLoad, int dataLoadQuantity)
        {
            _feedRequiresInitialDataLoad = feedRequiresInitialLoad;
            _dataLoadQuantity = dataLoadQuantity;
            var queueSize = Math.Max(dataLoadQuantity, DEFAULT_QUEUE_SIZE);

            Results = new CircularBuffer<IndicatorResult>(queueSize);
            _candlesticks = new CircularBuffer<Candlestick>(queueSize);
            _tradingPair = tradingPair;
        }

        public virtual async Task StartDataFeedAsync()
        {
            if (_dataFeedIsLive)
                return;

            if (DataFeed is null)
                throw new ArgumentNullException(nameof(DataFeed));

            if (_feedRequiresInitialDataLoad)
            {
                if (DataLoad is null)
                    throw new ArgumentNullException(nameof(DataLoad));

                var quantity = Math.Max(_dataLoadQuantity, DataLoad.MinCandleQuantity);
                var candles = await DataLoad.GetLatestCandlesAsync(_tradingPair, quantity);
                _candlesticks.Clear();
                _candlesticks.AddRange(candles);
                _dataInitialize?.Invoke();
            }

            DataFeed.ReceivedCandlestickData += OnReceivedData;
            DataFeed.CandleFeedAvailabilityChanged += OnDataFeedAvailabilityChanged;
            if (await DataFeed.TryStartStream() && DataFeed.TrySubscribeToCandleFeed(_tradingPair))
            {                
                OnAvailabilityChanged(this, true);
                _dataFeedIsLive = true;
            }
            else
            {
                OnAvailabilityChanged(this, false);
                DataFeed.ReceivedCandlestickData -= OnReceivedData;
                DataFeed.CandleFeedAvailabilityChanged -= OnDataFeedAvailabilityChanged;
            }
        }
        public virtual void StopDataFeed()
        {
            if (DataFeed is null)
                return;

            OnAvailabilityChanged(this, false);
            _dataFeedIsLive = false;
            DataFeed.ReceivedCandlestickData -= OnReceivedData;
            DataFeed.CandleFeedAvailabilityChanged -= OnDataFeedAvailabilityChanged;
        }
        public virtual async Task ResetFeedAsync()
        {
            if (_dataFeedIsLive)
                StopDataFeed();
            await StartDataFeedAsync();
        }
        public override string ToString()
        {
            if (Results.TryPeekLast(out var latestResult))
            {
                var headers = latestResult.GetCsvHeaders();
                StringBuilder builder = new StringBuilder();                
                builder.Append($"{latestResult.GetCsvData(headers[0])}");
                for (int i = 1; i < headers.Count; i++)
                    builder.Append($",{latestResult.GetCsvData(headers[i])}");
                builder.AppendLine();
                return builder.ToString();
            }
            else
                return string.Empty;
        }
        public string AllToString()
        {
            if (Results.TryPeekFirst(out var firstResult))
            {
                var headers = firstResult.GetCsvHeaders();
                StringBuilder builder = new StringBuilder();
                foreach (var result in Results.ToList())
                {
                    builder.Append($"{result.GetCsvData(headers[0])}");
                    for (int i = 1; i < headers.Count; i++)
                        builder.Append($",{result.GetCsvData(headers[i])}");
                    builder.AppendLine();
                }
                return builder.ToString();
            }
            else
                return string.Empty;
        }
        public string PrintCsvHeaders()
        {
            if (Results.TryPeekFirst(out var result))
            {
                return string.Join(",", result.GetCsvHeaders());
            }
            else
                return string.Empty;
        }

        protected abstract void CalculateIndicator();

        protected virtual void OnDataFeedAvailabilityChanged(object sender, CandleFeedAvailabilityEvent e)
        {
            if (!e.IsAvailable && _dataFeedIsLive)
                StopDataFeed();
        }
        protected virtual void OnReceivedData(object sender, Candlestick e)
        {
            if (e.TradingPair == _tradingPair)
            {
                _candlesticks.Add(e);
                CalculateIndicator();
            }
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
