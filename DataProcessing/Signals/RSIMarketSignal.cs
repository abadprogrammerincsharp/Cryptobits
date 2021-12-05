using Contracts.Enums;
using Contracts.Generics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataProcessing.Indicators;

namespace DataProcessing.Signals
{
    public static class RSIMarketSignal
    {
        private const string RSIKeyword = "RSI";
        public static MarketSignal GetMarketSignal (RsiCandlestickIndicator rsiIndicator, decimal oversold, decimal overbought, int rsiSampleCount = 10, int trendCount = 3)
        {
            MarketSignal signal = MarketSignal.Neutral;
            var rsiResults = rsiIndicator.Results.ToList();
            var rsiValues = rsiResults.Select(x=>x.GetResultByKeyword(RSIKeyword)).ToList();

            rsiValues.Reverse(); //Queue order (FIFO), meaning latest is last. We want to start from the last.
            var latestRsiValues = rsiValues.GetRange(0, rsiSampleCount);
            signal = CheckForOversoldOrOverbought(oversold, overbought, signal, latestRsiValues);
            signal = CheckForRateOfChange(trendCount, signal, latestRsiValues);

            return signal;
        }
        private static MarketSignal CheckForOversoldOrOverbought(decimal oversold, decimal overbought, MarketSignal signal, List<decimal> latestRsiValues)
        {
            if (latestRsiValues[0] < oversold) //Bear, with possible Bullish Divergence
            {
                signal = MarketSignal.BearishContinuation;
                foreach (var rsiValue in latestRsiValues)
                    if (rsiValue < latestRsiValues[0] && latestRsiValues.Min() == rsiValue)
                        signal = MarketSignal.BullishReversal;
            }
            else if (latestRsiValues[0] > overbought) //Bull, with possible Bearish Divergence
            {
                signal = MarketSignal.BullishContinuation;
                foreach (var rsiValue in latestRsiValues)
                    if (rsiValue > latestRsiValues[0] && latestRsiValues.Max() == rsiValue)
                        signal = MarketSignal.BearishReversal;
            }

            return signal;
        }
        private static MarketSignal CheckForRateOfChange(int trendCount, MarketSignal signal, List<decimal> latestRsiValues)
        {
            if (signal == MarketSignal.Neutral) //Checking incline for bull market
            {
                bool isSequential = true;
                for (int i = 0; i < trendCount - 1 && isSequential; i++)
                    isSequential = latestRsiValues[i] > latestRsiValues[i + 1];
                signal = (isSequential) ? MarketSignal.StrongBullContinuation : signal;
            }

            if (signal == MarketSignal.Neutral) //Checking decline for bear market
            {
                bool isSequential = true;
                for (int i = 0; i < trendCount - 1 && isSequential; i++)
                    isSequential = latestRsiValues[i] < latestRsiValues[i + 1];
                signal = (isSequential) ? MarketSignal.StrongBearContinuation : signal;
            }

            return signal;
        }

    }
}
