using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessing.Strategies
{
    public class LetItRideStrategy
    {
        //Check current position
        //If looking for awaiting entry,
        //  Check RSI, EMA and MACD that at least one is >= Bullish Divergence, and all are >= Neutral
        //  Set status to awaiting buy
        //  Look at the BollingerBands for 3x, 6x and 12x -
        //      If all status > neutral, place order and store at awaiting exit position
        //If looking for awaiting exit,
        //  While awaiting, check for shock drops.
        //  Await until Percent Gain breaks expected percent or if total loss > 5%.
        //  Once expected percent has been broken, store as high. Set total loss against new high.
    }
}
