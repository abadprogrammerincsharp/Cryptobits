using Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessing.Signals
{
    public static class EMAMarketSignal
    {        
        public static MarketSignal GetMarketSignal(decimal lastValue, decimal emaSeven, decimal emaFourteen, decimal emaTwentyEight)
        {
            MarketSignal signal = MarketSignal.Neutral;

            if (emaFourteen > emaTwentyEight) //Bullish Market
            {
                if (emaSeven > emaFourteen && lastValue > emaSeven) signal = MarketSignal.StrongBull;
                else if (emaSeven < emaFourteen) signal = MarketSignal.BearishReversal;
                else signal = MarketSignal.BullishContinuation;
            }

            else if (emaFourteen < emaTwentyEight) //Bearish Market
            {
                if (emaSeven < emaFourteen && lastValue < emaSeven) signal = MarketSignal.StrongBear;
                else if (emaSeven > emaFourteen) signal = MarketSignal.BullishReversal;
                else signal = MarketSignal.BearishContinuation;
            }

            return signal;
        }
    }
}
