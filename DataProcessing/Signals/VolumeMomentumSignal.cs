using Contracts.Concrete;
using Contracts.Enums;
using Contracts.Generics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessing.Signals
{
    public static class VolumeMomentumSignal
    {
        public static MomentumSignal GetMomentumSignal(Candlestick currentCandle, Candlestick previousCandle, decimal rateOfRapidChange = 0.1m)
        {
            MomentumSignal momentum = MomentumSignal.Stagnant;

            if (currentCandle.TradeVolume != 0)
            {
                momentum = (currentCandle.TradeVolume > previousCandle.TradeVolume) ? MomentumSignal.SlowlyIncreasing :
                           (currentCandle.TradeVolume == previousCandle.TradeVolume) ? MomentumSignal.Steady : MomentumSignal.SlowlyDecreasing;

                switch (momentum)
                {
                    case MomentumSignal.SlowlyIncreasing:
                        momentum = CheckRateOfIncrease(currentCandle, previousCandle, rateOfRapidChange);
                        break;

                    case MomentumSignal.SlowlyDecreasing:
                        momentum = CheckRateOfDecrease(currentCandle, previousCandle, rateOfRapidChange);
                        break;

                    default:
                        break;
                }
            }

            return momentum;
        }

        private static MomentumSignal CheckRateOfIncrease(Candlestick currentCandle, Candlestick previousCandle, decimal rateOfRapidChange)
        {
            MomentumSignal momentum = ((currentCandle.TradeVolume/previousCandle.TradeVolume) - 1 > rateOfRapidChange) ? 
                MomentumSignal.SharplyIncreasing : 
                MomentumSignal.SlowlyIncreasing;
            return momentum;
        }
        private static MomentumSignal CheckRateOfDecrease(Candlestick currentCandle, Candlestick previousCandle, decimal rateOfRapidChange)
        {
            MomentumSignal momentum = ((previousCandle.TradeVolume / currentCandle.TradeVolume) - 1 > rateOfRapidChange) ?
                MomentumSignal.SharplyDecreasing :
                MomentumSignal.SlowlyDecreasing;
            return momentum;
        }

    }
}
