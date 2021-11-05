using Contracts.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessing.Indicators
{
    public class EmaCandlestickIndicator : GenericCandlestickIndicator
    {
        private IndicatorResult _valueSet, _previousSet;
        private readonly int _emaPeriod;
        private readonly string _valueSetKey;

        public EmaCandlestickIndicator(TradingPair tradingPair, int period, string descriptor = "") : base(tradingPair, true, period + 1)
        {
            _emaPeriod = period;
            _valueSetKey = $"{descriptor}{period}EMA";
            _dataInitialize = InitializeEmaData;
        }
        protected override void CalculateIndicator()
        {
            if (!_candlesticks.TryPeekLast(out var candle))
                return;
            _previousSet.ResultSet[_valueSetKey] = _valueSet.ResultSet[_valueSetKey];
            _valueSet.ResultSet[_valueSetKey] = CalculateEmaValue(candle.Close, _emaPeriod, _previousSet.ResultSet[_valueSetKey]);
            _valueSet.LastUpdated = DateTimeOffset.UtcNow;
            Results.Add(_valueSet);
            OnIndicatorUpdated(this);
        }

        private void InitializeEmaData()
        {
            var closingValues = _candlesticks.ToList().Select(x => x.Close).ToList();
            var smaValues = closingValues.GetRange(0, _emaPeriod);

            var ema = CalculateSmaValue(smaValues);
            for (int i = _emaPeriod; i < closingValues.Count; i++)
                ema = CalculateEmaValue(closingValues[i], _emaPeriod, ema);

            _valueSet = new IndicatorResult();
            _previousSet = new IndicatorResult();
            _valueSet.ResultSet.Add(_valueSetKey, ema); 
            _valueSet.LastUpdated = DateTimeOffset.UtcNow;
        }
        private decimal CalculateSmaValue(List<decimal> values)
        {
            var result = values.Sum();
            return result / values.Count;
        }
        private decimal CalculateEmaValue(decimal value, int numberOfPeriods, decimal previousEma)
        {
            decimal smoothingConstant = 2m / (numberOfPeriods + 1m);
            return (value - previousEma) * smoothingConstant + previousEma;
        }

    }
}
