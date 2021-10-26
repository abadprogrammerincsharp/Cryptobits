using Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessing.Signals
{
    public static class MacdMarketSignal
    {
        public static MarketSignal GetMarketSignal(decimal macdLine, decimal signalLine, decimal prevMacdLine, decimal prevSignalLine, 
                                                   decimal fluxPercent = 0.33m, decimal neutralPercent = 0.1m)
        {
            MarketSignal signal = MarketSignal.Neutral;
            if ((signalLine > macdLine) && ((signalLine / macdLine) - 1 > neutralPercent)) //Bull Market
            {
                if ((signalLine / macdLine) - 1 > fluxPercent) signal = MarketSignal.StrongBull;
                else if (prevSignalLine < prevMacdLine) signal = MarketSignal.BullishReversal;
                else signal = MarketSignal.BullishContinuation;
            }

            else if ((macdLine > signalLine) && ((macdLine / signalLine) - 1 > neutralPercent)) //Bear Market
            {
                if ((macdLine / signalLine) - 1 > fluxPercent) signal = MarketSignal.StrongBear;
                else if (prevMacdLine < prevSignalLine) signal = MarketSignal.BearishReversal;
                else signal = MarketSignal.BearishContinuation;
            }

            return signal;
        }
    }
}
