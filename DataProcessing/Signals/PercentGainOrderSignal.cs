using Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessing.Signals
{
    public static class PercentGainOrderSignal
    {
        public static OrderSignal GetSellOrderSignal(decimal buyPosition, decimal currentValue, decimal percentageGainHopingFor = 0.03m, decimal percentageLossAcceptable = -0.02m)
        {
            OrderSignal signal = OrderSignal.Hold;
            var valueRealized = (currentValue - buyPosition) / buyPosition;

            if (valueRealized >= percentageGainHopingFor)
                signal = OrderSignal.Sell;
            else if (valueRealized <= percentageLossAcceptable)
                signal = OrderSignal.StrongSell;
            
            return signal;
        }
    }
}