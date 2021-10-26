using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessing.Strategies
{
    public class ScalpingStrategy
    {
        


        //Check current position
        //If looking for awaiting entry,
        //  Check RSI and EMA signals and ensure we're in Bullish Divergence or Bull.
        //  Set status to awaiting buy
        //  Look at the BollingerBands for 3x, 6x and 12x -
        //      If all status > neutral, place order and store at exit position
        //If looking for awaiting exit,
        //  Check Percent Gain. If > 3% or < 2% sell. Change position to awaiting entry.
    }
}
