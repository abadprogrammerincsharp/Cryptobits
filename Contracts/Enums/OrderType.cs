using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Enums
{
    public enum OrderType
    {
        Limit,
        Market,
        StopLoss,
        StopLossLimit,
        TakeProfit,
        TakeProfitLimit,
        LimitMaker,
        OneCancelsOther,
        OneCancelsOtherStopLimit,
    }
}
