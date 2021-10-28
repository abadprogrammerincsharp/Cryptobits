using Contracts.Concrete;
using Contracts.Generics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessing.Indicators
{
    public class MacdCandlestickIndicator : GenericCandlestickIndicator
    {
        int _fastLength, _slowLength, _signalLength;
        decimal _fast, _slow;
        string  _signalKey, _macdKey;
        IndicatorResult _macdValues, _previousMacdValues;
        CircularBuffer<decimal> _macdBufferValues;

        public MacdCandlestickIndicator(int fastLength, int slowLength, int signalLength, string descriptor ="") : base(true, slowLength + 1)
        {
            _fastLength = fastLength;
            _slowLength = slowLength;
            _signalLength = signalLength;
            _signalKey = $"{descriptor}{signalLength}Signal";
            _macdKey = $"{descriptor}{fastLength}/{slowLength}MACD";

            _dataInitialize = GetInitialMacdValueSet;
        }

        protected override void CalculateIndicator()
        {
            if (!_candlesticks.TryPeekLast(out var candle))
                return;
            GetNextMacdValueSet(candle.Close);
            Results.Add(_macdValues);
            OnIndicatorUpdated(this);
        }


        private void GetInitialMacdValueSet()
        {
            List<decimal> values = _candlesticks.ToList().Select(x => x.Close).ToList();
            _macdValues = new IndicatorResult();
            _previousMacdValues = new IndicatorResult();

            _macdValues.ResultSet.Add(_macdKey, 0);
            _macdValues.ResultSet.Add(_signalKey, 0);

            _previousMacdValues.ResultSet.Add(_macdKey, 0);
            _previousMacdValues.ResultSet.Add(_signalKey, 0);


            /*************
             * Seed using SMA
             *************/
            //decimal fastLengthSma = CalculateSmaValue(values.GetRange(0, _fastLength));
            //decimal fastLengthEma = fastLengthSma;
            //for (int i = _fastLength; i < _slowLength; i++)
            //    fastLengthEma = CalculateEmaValue(values[i], _fastLength, fastLengthEma);
            //decimal slowLengthSma = CalculateSmaValue(values.GetRange(0, _slowLength));
            /****************/

            /************* 
             * Seed using First Value
             *************/
            decimal fastLengthEma = values[0];
            for (int i = 1; i < _slowLength; i++)
                fastLengthEma = CalculateEmaValue(values[i], _fastLength, fastLengthEma);
            _fast = fastLengthEma;
            decimal slowLengthSma = values[0];
            /****************/

            decimal slowLengthEma = CalculateEmaValue(values[_slowLength], _slowLength, slowLengthSma);
            _slow = slowLengthEma;
            _fast = fastLengthEma;

            var macd = fastLengthEma - slowLengthEma;
            _macdValues.ResultSet[_macdKey] = macd;
            _macdValues.LastUpdated = DateTimeOffset.UtcNow;
            _macdBufferValues.Add(macd);
        }
        private void GetNextMacdValueSet(decimal currentValue)
        {
            _previousMacdValues.ResultSet[_macdKey] = _macdValues.ResultSet[_macdKey];
            _previousMacdValues.ResultSet[_signalKey] = _macdValues.ResultSet[_signalKey];

            var macd = CalculateMacd(currentValue, _slow, _fast, out var slowLengthEma, out var fastLengthEma);
            _slow = slowLengthEma;
            _fast = fastLengthEma;
            _macdValues.ResultSet[_macdKey] = macd;

            var macdValuesProcessed = _macdBufferValues.Count;

            if (macdValuesProcessed > _signalLength)
            {
                var signal = CalcluateSignal(macd, _macdValues.ResultSet[_signalKey]);
                _macdValues.ResultSet[_signalKey] = signal;
            }
            else if (macdValuesProcessed == _signalLength)
                _macdValues.ResultSet[_signalKey] = CalculateSmaValue(_macdBufferValues.ToList());
            _macdValues.LastUpdated = DateTimeOffset.UtcNow;
        }
        private decimal CalcluateSignal(decimal latestMacdValue, decimal signalEma)
        {
            return CalculateEmaValue(latestMacdValue, _signalLength, signalEma);
        }
        private decimal CalculateMacd(decimal currentValue, decimal slowEma, decimal fastEma, out decimal newSlowEma, out decimal newFastEma)
        {
            newSlowEma = CalculateEmaValue(currentValue, _slowLength, slowEma);
            newFastEma = CalculateEmaValue(currentValue, _fastLength, fastEma);
            return newFastEma - newSlowEma;
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
