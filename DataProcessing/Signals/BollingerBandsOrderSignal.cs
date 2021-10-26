using Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessing.Signals
{
    public static class BollingerBandsOrderSignal
    {
        public static OrderSignal GetOrderSignal(MarketSignal marketSignal, decimal currentValue, decimal highBand, decimal midBand, decimal lowBand) 
        {
            OrderSignal signal = OrderSignal.Hold;
                                                                                                       //Buy when:
            var timetoBuy  = (marketSignal == MarketSignal.BullishContinuation && currentValue < highBand) ||           //1) We're in a bull run and the value is below the high band (not overbought)
                             (marketSignal >  MarketSignal.BearishContinuation && currentValue < lowBand)  ||           //2) We're not in a bear run and the value is below the low band (oversold)
                             (marketSignal >= MarketSignal.BullishReversal && currentValue < midBand); //3) We're coming out of a bear market and the value is under the expected value

                                                                                                       //Sell when:
            var timetoSell = (marketSignal == MarketSignal.BearishContinuation && currentValue > lowBand)  ||           //1) We're in a bear run and the current value is greater than the low band (not oversold)
                             (marketSignal <  MarketSignal.BullishContinuation && currentValue > highBand) ||           //2) We're not in a bull run and the value is greater than the high band (overbought)
                             (marketSignal <= MarketSignal.BearishReversal && currentValue > midBand); //3) We're coming out of a bull market and the value is over the expected value

            if (marketSignal == MarketSignal.StrongBear)
                signal = OrderSignal.StrongSell;
            else if (timetoSell)
                signal = OrderSignal.Sell;
            else if (timetoBuy)
                signal = OrderSignal.Buy;
            else if (marketSignal == MarketSignal.StrongBull)
                signal = OrderSignal.StrongBuy;

            return signal;
        }
    }
}
