using Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataProcessing.Indicators;

namespace DataProcessing.Signals
{
    public static class EMAMarketSignal
    {
        private const string EMAKeyword = "EMA";

        public static MarketSignal GetMarketSignal(decimal lastCandleValue, EmaCandlestickIndicator emaSeven, EmaCandlestickIndicator emaFourteen, EmaCandlestickIndicator emaTwentyEight)
        {
            if (emaSeven.Results.TryPeekLast(out var emaSevenResult) && emaFourteen.Results.TryPeekLast(out var emaFourteenResult) && emaTwentyEight.Results.TryPeekLast(out var emaTwentyEightResult))
                return GetMarketSignal(lastCandleValue, 
                    emaSevenResult.GetResultByKeyword(EMAKeyword), 
                    emaFourteenResult.GetResultByKeyword(EMAKeyword), 
                    emaTwentyEightResult.GetResultByKeyword(EMAKeyword));
            
            else 
                return MarketSignal.Neutral;

        }

        public static MarketSignal GetMarketSignal(decimal lastCandleValue, decimal emaSeven, decimal emaFourteen, decimal emaTwentyEight)
        {
            MarketSignal signal = MarketSignal.Neutral;

            if (emaFourteen > emaTwentyEight) //Bullish Market
            {
                if (emaSeven > emaFourteen && lastCandleValue > emaSeven) signal = MarketSignal.StrongBullContinuation;
                else if (emaSeven < emaFourteen) signal = MarketSignal.BearishReversal;
                else signal = MarketSignal.BullishContinuation;
            }

            else if (emaFourteen < emaTwentyEight) //Bearish Market
            {
                if (emaSeven < emaFourteen && lastCandleValue < emaSeven) signal = MarketSignal.StrongBearContinuation;
                else if (emaSeven > emaFourteen) signal = MarketSignal.BullishReversal;
                else signal = MarketSignal.BearishContinuation;
            }

            return signal;
        }
    }
}
