using Contracts.Concrete;
using Contracts.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessing.Indicators
{
    public class RawDataIndicator : GenericCandlestickIndicator
    {
        string _prefix;
        const string START = "S", END = "E", OPEN = "O", CLOSE = "C", HIGH = "H", LOW = "L",
                     VOLUME = "V", DELTA = "D", PERECENT = "P";

        public RawDataIndicator(TradingPair tradingPair) : base(tradingPair, false, 0)
        {
            _prefix = $"{tradingPair.GetUppercaseSymbolPair()}.{tradingPair.Exchange}.{tradingPair.CandlestickInterval.TotalMinutes}";
        }

        protected override void CalculateIndicator()
        {
            var indicator = new IndicatorResult();
            if (!_candlesticks.TryPeekLast(out var candle))
                return;
            indicator.ResultSet.Add(_prefix + START, candle.Start.ToUnixTimeSeconds());
            indicator.ResultSet.Add(_prefix + END, candle.End.ToUnixTimeSeconds());
            indicator.ResultSet.Add(_prefix + OPEN, candle.Open);
            indicator.ResultSet.Add(_prefix + CLOSE, candle.Close);
            indicator.ResultSet.Add(_prefix + HIGH, candle.High);
            indicator.ResultSet.Add(_prefix + LOW, candle.Low);
            indicator.ResultSet.Add(_prefix + VOLUME, candle.TradeVolume);
            indicator.ResultSet.Add(_prefix + DELTA, candle.Change);
            indicator.ResultSet.Add(_prefix + PERECENT, candle.ChangePercent);
            indicator.LastUpdated = DateTimeOffset.UtcNow;
            Results.Add(indicator);

            OnIndicatorUpdated(this);
        }
    }
}
