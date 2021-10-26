using Contracts.Concrete;
using Contracts.Generics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessing.Indicators
{
    public class RsiCandlestickIndicator : GenericCandlestickIndicator
    {
        private int _rsiPeriod;
        private decimal _smmaUp, _smmaDown, _rsi;
        private string _rsiKey;
        CircularBuffer<decimal> _ups, _downs;

        public RsiCandlestickIndicator(int period, string rsiKey) : base(true, period + 1)
        {
            _rsiPeriod = period;
            _ups = new CircularBuffer<decimal>();
            _downs = new CircularBuffer<decimal>();
            _dataInitialize = GetInitialRsiValueSet;
            _rsiKey = rsiKey;
        }

        protected override void CalculateIndicator()
        {
            if (!_candlesticks.TryPeekLast(out var candle))
                return;
            UpdateValueSet(candle.Close);
            var indicator = new IndicatorResult();
            indicator.ResultSet.Add(_rsiKey, _rsi);
            Results.Add(indicator);
            OnIndicatorUpdated(this);
        }

        private void GetInitialRsiValueSet()
        {
            var values = _candlesticks.ToList().Select(x => x.Close).ToList();

            if (values == null)
                return;

            var value = values[0];
            decimal up = 0, down = 0;

            if (value <= 0)
            {
                down = Math.Abs(value);
                _ups.Add(0);
                _downs.Add(down);
            }
            else
            {
                up = value;
                _ups.Add(up);
                _downs.Add(0);
            }

            _smmaUp = up;
            _smmaDown = down;

            if (down == 0)
                _rsi = 100;
            else
                _rsi = 100 - 100 / 1 + (_smmaUp / _smmaDown);

            for (int i = 1; i < values.Count; i++)
                UpdateValueSet(values[i]);

        }
        private void UpdateValueSet(decimal value)
        {
            _rsi = CalculateRsi(value, _smmaUp, _smmaDown, out var isUp, out var absVal, out var newSmmaUp, out var newSmmaDown);

            if (!isUp)
            {
                _ups.Add(0);
                _downs.Add(absVal);
            }
            else
            {
                _ups.Add(absVal);
                _downs.Add(0);
            }

            _smmaUp= newSmmaUp;
            _smmaDown= newSmmaDown;
        }

        private decimal CalculateRsi(decimal value, decimal currentSmmaUp, decimal currentSmmaDown, out bool valueIsUp, out decimal absValue, out decimal newSmmaUp, out decimal newSmmaDown)
        {
            decimal latestDown = 0, latestUp = 0, rsi = 0;
            valueIsUp = value >= 0;

            if (!valueIsUp)
            {
                absValue = Math.Abs(value);
                latestDown = absValue;
            }
            else
            {
                absValue = value;
                latestUp = absValue;
            }

            newSmmaUp = CalculateEmaValue(latestUp, _rsiPeriod, currentSmmaUp);
            newSmmaDown = CalculateEmaValue(latestDown, _rsiPeriod, currentSmmaDown);

            if (newSmmaDown == 0)
                rsi = 100;
            else
                rsi = 100 - (100 / (1 + (newSmmaUp / newSmmaDown)));

            return rsi;
        }
        private decimal CalculateEmaValue(decimal value, int numberOfPeriods, decimal previousEma)
        {
            decimal smoothingConstant = 1m / (numberOfPeriods);
            return (value - previousEma) * smoothingConstant + previousEma;
        }
    }
}
