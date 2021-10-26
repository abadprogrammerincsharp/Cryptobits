using Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessing.Signals
{
    public static class ShockDropOrderSignal
    {
        public static OrderSignal GetOrderSignal(decimal previousChange, decimal currentChange, decimal shockDropPercent = 0.05m, decimal marginOfError = 0.015m)
        {
            OrderSignal signal = OrderSignal.Hold;
            if (previousChange > (shockDropPercent - marginOfError) && currentChange > shockDropPercent)
                signal = OrderSignal.StrongSell;
            return signal;
        }
    }
}
