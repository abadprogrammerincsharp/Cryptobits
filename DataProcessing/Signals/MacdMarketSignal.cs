using Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataProcessing.Indicators;

namespace DataProcessing.Signals
{
    public static class MacdMarketSignal
    {
        private const string MacdKeyword = "MACD";
        private const string SignalKeyword = "Signal";

        public static MarketSignal GetMarketSignal (MacdCandlestickIndicator macdIndicator)
        {
            var macdResults = macdIndicator.Results.ToList();
            var latest = macdResults[macdResults.Count - 1];
            var previous = macdResults[macdResults.Count - 2];

            return GetMarketSignal(latest.GetResultByKeyword(MacdKeyword), latest.GetResultByKeyword(SignalKeyword),
                                   previous.GetResultByKeyword(MacdKeyword), latest.GetResultByKeyword(SignalKeyword));

        }

        public static MarketSignal GetMarketSignal(decimal macdLine, decimal signalLine, decimal prevMacdLine, decimal prevSignalLine, 
                                                   decimal fluxPercent = 0.33m, decimal neutralPercent = 0.1m)
        {
            MarketSignal signal = MarketSignal.Neutral;
            if ((signalLine > macdLine) && ((signalLine / macdLine) - 1 > neutralPercent)) //Bull Market
            {
                if ((signalLine / macdLine) - 1 > fluxPercent) signal = MarketSignal.StrongBullContinuation;
                else if (prevSignalLine < prevMacdLine) signal = MarketSignal.BullishReversal;
                else signal = MarketSignal.BullishContinuation;
            }

            else if ((macdLine > signalLine) && ((macdLine / signalLine) - 1 > neutralPercent)) //Bear Market
            {
                if ((macdLine / signalLine) - 1 > fluxPercent) signal = MarketSignal.StrongBearContinuation;
                else if (prevMacdLine < prevSignalLine) signal = MarketSignal.BearishReversal;
                else signal = MarketSignal.BearishContinuation;
            }

            return signal;
        }
    }
}
