using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessing.Strategies
{
    class CandleReaderStrategy
    {
        //Pull last 6 candles
        //If last pattern stored had a reversal, check indicators for
        //a reversal. If reversal is confirmed, send order.
        //Clear reversal queue.

        //[0]-[3] : Get a trend signal - if no trend, check EMA
        //[1]-[4] : Get a trend signal - if no trend, check EMA
        
        //Check three candle patterns using [0][3] trend
        //If a pattern is found, store pattern in reversal queue

        //If no pattern is found, check two candle patterns using [1][4] trend
        //If a pattern is found, store pattern in  reversal queue

        //If there's a pattern in the reversal queue, check indicators for 
        //a reversal. If a reversal is confirmed, send order.
        //If reversal is confirmed, clear reversal queue
    }
}
