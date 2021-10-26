using Contracts.Concrete;
using Contracts.Generics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessing.Indicators
{
    public class BollingerBandsCandlestickIndicator : GenericCandlestickIndicator
    {
        private IndicatorResult _bandResult;
        private string _highBandKey, _lowBandKey, _middleKey, _highLowDifferenceKey;
        private CircularBuffer<Candlestick> _candlesOfPeriod;
        private decimal _factor; 
        private int _period;

        public BollingerBandsCandlestickIndicator(string highBandKey, string lowBandKey, string middleKey, string highLowDifferenceKey, decimal factor, int period):
            base(true, period + 1)
        {
            _highBandKey = highBandKey ?? throw new ArgumentNullException(nameof(highBandKey));
            _lowBandKey = lowBandKey ?? throw new ArgumentNullException(nameof(lowBandKey));
            _middleKey = middleKey ?? throw new ArgumentNullException(nameof(middleKey));
            _highLowDifferenceKey = highLowDifferenceKey ?? throw new ArgumentNullException(nameof(highLowDifferenceKey));
            _factor = factor;
            _period = period;
            _dataInitialize = InitializeBollingerBands;
        }

        private void InitializeBollingerBands()
        {
            _candlesOfPeriod.AddRange(_candlesticks.ToList().GetRange(0, _period - 1));

            var candlesticks = _candlesticks.ToList();
            for (int i = _period; i < candlesticks.Count; i++)
                UpdateValueSet(candlesticks[i]);
        }

        private void UpdateValueSet(Candlestick candle)
        {
            _candlesOfPeriod.Add(candle);
            var lastCandles = _candlesOfPeriod.ToList();
            CalculateBollingerBands(lastCandles.Select(x => x.Close).ToList(), _factor);

        }

        private void CalculateBollingerBands(List<decimal> values, decimal factor)
        {            
            var sma = CalculateSma(values);
            var standardDeviation = GetStandardDeviation(values, sma);

            var middleBand = sma;
            var upperBand = sma + (factor * standardDeviation);
            var lowerBand = sma - (factor * standardDeviation);

            _bandResult = new IndicatorResult();
            _bandResult.ResultSet.Add(_highBandKey, upperBand);
            _bandResult.ResultSet.Add(_middleKey, middleBand);
            _bandResult.ResultSet.Add(_lowBandKey, lowerBand);
            _bandResult.ResultSet.Add(_highLowDifferenceKey, upperBand - lowerBand);
            _bandResult.LastUpdated = DateTimeOffset.UtcNow;

        }

        private decimal CalculateSma(List<decimal> values)
        {
            var sum = values.Sum();
            return sum / values.Count;
        }
        private decimal GetStandardDeviation(List<decimal> values, decimal sma)
        {
            var deductionValues = new List<decimal>();
            foreach (var value in values)
            {
                var deduction = value - sma;
                var deductionSquared = deduction * deduction;
                deductionValues.Add(deductionSquared);
            }

            var sumOfDeductionValues = deductionValues.Sum();
            var average = sumOfDeductionValues / values.Count;
            var standardDeviation = (decimal)Math.Sqrt((double)average);

            return standardDeviation;
        }

        protected override void CalculateIndicator()
        {
            if (!_candlesticks.TryPeekLast(out var candle))
                return;
            UpdateValueSet(candle);
            Results.Add(_bandResult);
            OnIndicatorUpdated(this);
        }
    }
}
