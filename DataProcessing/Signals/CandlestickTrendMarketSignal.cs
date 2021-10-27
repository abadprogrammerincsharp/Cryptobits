using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contracts.Enums;
using Contracts.Concrete;

namespace DataProcessing.Signals
{
    public static class CandlestickTrendMarketSignal
    {
        public static MarketSignal GetMarketSignalFromCandleTrend(Candlestick oldest, Candlestick previous, Candlestick current)
        {
            var marketSignal = MarketSignal.Neutral;
            if (oldest.Close < oldest.Open)
            {
                marketSignal = current.Close < current.Open
                    ? (previous.Close < oldest.Close) ? MarketSignal.StrongBearContinuation : MarketSignal.BearishContinuation
                    : (previous.Close < previous.Open) ? MarketSignal.BearishContinuation : MarketSignal.Neutral;
            }
            else if (oldest.Close > oldest.Open)
            {
                marketSignal = current.Close > current.Open
                    ? (previous.Close > oldest.Close) ? MarketSignal.StrongBullContinuation : MarketSignal.BullishContinuation
                    : (previous.Close > previous.Open) ? MarketSignal.BullishContinuation : MarketSignal.Neutral;
            }

            return marketSignal;
        }
    }
}
