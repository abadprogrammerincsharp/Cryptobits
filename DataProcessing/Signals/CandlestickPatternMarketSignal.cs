using Contracts.Concrete;
using Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessing.Signals
{
    public static class CandlestickPatternMarketSignal
    {
        private const decimal FivePercent = 0.05m;
        private const decimal TenPercent = 0.1m;

       
        public static MarketSignal GetMarketSignalFromTwoCandles(Candlestick previous, Candlestick current, MarketSignal priorTrend, out string patternName)
        {
            MarketSignal signal = priorTrend;
            bool isReversing = false;
            patternName = "No Pattern";

            if (priorTrend <= MarketSignal.BearishContinuation)
            {
                isReversing = IsBullishEngulfing(previous, current, out patternName);
                isReversing = isReversing ? isReversing : IsBullishHarami(previous, current, out patternName);
                isReversing = isReversing ? isReversing : IsPiercingLine(previous, current, out patternName);
                isReversing = isReversing ? isReversing : IsTweezerBottom(previous, current, out patternName);
                signal = isReversing ? MarketSignal.BullishReversal : priorTrend;
            }
            if (!isReversing && priorTrend >= MarketSignal.BullishContinuation)
            {
                isReversing = IsBearishEngulfing(previous, current, out patternName);
                isReversing = isReversing ? isReversing : IsBearishHarami(previous, current, out patternName);
                isReversing = isReversing ? isReversing : IsDarkCloudCover(previous, current, out patternName);
                isReversing = isReversing ? isReversing : IsTweezerTop(previous, current, out patternName);
                signal = isReversing ? MarketSignal.BearishReversal : priorTrend;
            }
            if (!isReversing)
            {
                isReversing = IsBullishKicker(previous, current, out patternName);
                signal = isReversing ? MarketSignal.BullishReversal : priorTrend;
                isReversing = isReversing ? isReversing : IsBearishKicker(previous, current, out patternName);
                signal = signal == priorTrend ? (isReversing ? MarketSignal.BearishReversal : priorTrend) : signal;
            }
            
            patternName = signal == priorTrend ? "No Pattern" : patternName;

            return signal;
        }
        public static MarketSignal GetMarketSignalFromThreeCandles(Candlestick oldest, Candlestick previous, Candlestick current, MarketSignal priorTrend, out string patternName) 
        {
            MarketSignal signal = priorTrend;
            bool isReversing = false;
            patternName = "No Pattern";

            if (priorTrend <= MarketSignal.BearishContinuation)
            {
                isReversing = IsMorningstar(oldest, previous, current, out patternName);
                isReversing = isReversing ? isReversing : IsBullishAbandonedBaby(oldest, previous, current, out patternName);
                isReversing = isReversing ? isReversing : IsThreeWhiteSoldiers(oldest, previous, current, out patternName);
                isReversing = isReversing ? isReversing : IsMorningDojiStar(oldest, previous, current, out patternName);
                isReversing = isReversing ? isReversing : IsThreeOutsideUp(oldest, previous, current, out patternName);
                signal = isReversing ? MarketSignal.BullishReversal : priorTrend;
            }
            if (!isReversing && priorTrend >= MarketSignal.BullishContinuation)
            {
                isReversing = IsEveningStar(oldest, previous, current, out patternName);
                isReversing = isReversing ? isReversing : IsBearishAbandonedBaby(oldest, previous, current, out patternName);
                isReversing = isReversing ? isReversing : IsThreeBlackCrows(oldest, previous, current, out patternName);
                isReversing = isReversing ? isReversing : IsEveningDojiStar(oldest, previous, current, out patternName);
                isReversing = isReversing ? isReversing : IsThreeOutsideDown(oldest, previous, current, out patternName);
                signal = isReversing ? MarketSignal.BearishReversal : priorTrend;
            }

            patternName = signal == priorTrend ? "No Pattern" : patternName;
            return signal;
        }

        
        //Bullish Two-Candle Patterns
        private static bool IsBullishKicker(Candlestick previous, Candlestick current, out string patternName)
        {
            patternName = "Bullish Kicker";
            return (previous.Close < previous.Open) &&
                   (current.Close > current.Open) &&

                   (current.Low > previous.Open);
        }
        private static bool IsBullishEngulfing(Candlestick previous, Candlestick current, out string patternName)
        {
            patternName = "Bullish Engulfing";
            //Bull pattern
            return (previous.Close < previous.Open) &&
                   (current.Close > current.Open) &&

                   (current.Open < previous.Close) &&
                   (current.Close > previous.Open);
        }
        private static bool IsBullishHarami(Candlestick previous, Candlestick current, out string patternName)
        {
            patternName = "Bullish Harami";

            return (previous.Close < previous.Open) &&
                   (current.Close > current.Open) &&

                   (current.Open > previous.Close) &&
                   (current.Close < previous.Open);
        }
        private static bool IsPiercingLine(Candlestick previous, Candlestick current, out string patternName)
        {
            patternName = "Piercing Line";
            //50% point
            var halfwayLossPoint = previous.Open - ((previous.Open - previous.Close) / 2);

            return (previous.Close < previous.Open) &&
                   (current.Close > current.Open) &&

                   (current.Open <= previous.Low) &&
                   (current.Close > halfwayLossPoint) &&
                   (current.Close < previous.Open);
        }
        private static bool IsTweezerBottom(Candlestick previous, Candlestick current, out string patternName)
        {
            patternName = "Tweezer Bottom";
            return (current.Low == previous.Low) &&
                   current.Low != current.High;
        }

        //Bearish Two-Candle Patterns
        private static bool IsBearishKicker(Candlestick previous, Candlestick current, out string patternName)
        {
            patternName = "Bearish Kicker";
            return (previous.Close > previous.Open) &&
                   (current.Close < current.Open) &&

                   (current.High < previous.Open);
        }
        private static bool IsBearishEngulfing(Candlestick previous, Candlestick current, out string patternName)
        {
            patternName = "Bearish Engulfing";
            //Bear pattern
            return (previous.Close > previous.Open) &&
                   (current.Close < current.Open) &&

                   (current.Open >= previous.Close) &&
                   (current.Close < previous.Open);
        }
        private static bool IsBearishHarami(Candlestick previous, Candlestick current, out string patternName)
        {
            patternName = "Bearish Harami";
            //Bear pattern
            return (previous.Close > previous.Open) &&
                   (current.Close < current.Open) &&

                   (current.Open < previous.Close) &&
                   (current.Close > previous.Open);
        }
        private static bool IsDarkCloudCover(Candlestick previous, Candlestick current, out string patternName)
        {
            patternName = "Dark Cloud Cover";
            //50% point
            var halfwayGainPoint = previous.Close - ((previous.Close - previous.Open) / 2);

            //Bear pattern
            return (previous.Close > previous.Open) &&
                   (current.Close < current.Open) &&

                   (current.Open >= previous.Close) &&
                   (current.Close < halfwayGainPoint) &&
                   (current.Close > previous.Open);
        }
        private static bool IsTweezerTop(Candlestick previous, Candlestick current, out string patternName)
        {
            patternName = "Tweezer Top";
            return (current.High == previous.High) &&
                    current.High != current.Low;
        }

        //Bullish Three-Candle Patterns
        private static bool IsMorningstar(Candlestick oldest, Candlestick previous, Candlestick current, out string patternName)
        {
            patternName = "Morningstar";
            var halfwayLoss = oldest.Open - (oldest.Open - oldest.Close / 2);

            //Oldest is a bear 
            return oldest.Close < oldest.Open &&
                    //Previous has a candle body below the oldest
                    previous.Open < oldest.Close &&
                    previous.Close < oldest.Close &&
                    //Current is a bull
                    current.Close > current.Open &&
                    //Current body opened higher than previous body
                    current.Open > previous.Open &&
                    current.Open > previous.Close &&
                    //Current close is >= oldest 50% loss
                    current.Close >= halfwayLoss;
        }
        private static bool IsBullishAbandonedBaby(Candlestick oldest, Candlestick previous, Candlestick current, out string patternName)
        {
            patternName = "Bullish Abandoned Baby";
            //Oldest is a bear 
            return oldest.Close < oldest.Open &&
                    //Previous entire candle is below the oldest
                    previous.High < oldest.Low &&
                    //Previous is a doji
                    (previous.Close >= previous.Open - (FivePercent * (previous.High - previous.Low)) ||
                     previous.Close <= previous.Open + (FivePercent * (previous.High - previous.Low)))&&
                    //Current is a bull
                    current.Close > current.Open &&
                    //Current body stayed higher than previous body
                    current.Low > previous.High;
        }
        private static bool IsThreeWhiteSoldiers(Candlestick oldest, Candlestick previous, Candlestick current, out string patternName)
        {
            patternName = "Three White Soldiers";
            //Oldest is a bull 
            return oldest.Close > oldest.Open &&
                    //Oldest lower shadow is less than 10% of the trade value
                    oldest.Open - oldest.Low <= (TenPercent * (oldest.High - oldest.Low)) &&
                    //Oldest upper shadow is less than 10% of the trade value
                    oldest.High - oldest.Close <= (TenPercent * (oldest.High - oldest.Low)) &&

                    //Previous is a bull
                    previous.Close > previous.Open &&
                    //Previous lower shadow is less than 10% of the trade value
                    previous.Open - previous.Low <= (TenPercent * (previous.High - previous.Low)) &&
                    //Previous upper shadow is less than 10% of the trade value
                    previous.High - previous.Close <= (TenPercent * (previous.High - previous.Low)) &&
                    //Previous opened between oldest candle
                    previous.Open > oldest.Open && previous.Open < oldest.Close &&
                    //Previous closed higher than oldest
                    previous.Close > oldest.Close &&

                    //Current is a bull
                    current.Close > current.Open &&
                    //Current lower shadow is less than 10% of the trade value
                    current.Open - current.Low <= (TenPercent * (current.High - current.Low)) &&
                    //Current upper shadow is less than 10% of the trade value
                    current.High - current.Close <= (TenPercent * (current.High - current.Low)) &&
                    //Current opened between oldest candle
                    current.Open > previous.Open && current.Open < previous.Close &&
                    //Current closed higher than oldest
                    current.Close > previous.Close;
        }
        private static bool IsMorningDojiStar(Candlestick oldest, Candlestick previous, Candlestick current, out string patternName)
        {
            patternName = "Bullish Morning Doji Star";
            var halfwayLoss = oldest.Open - (oldest.Open - oldest.Close / 2);

            //First Candle rules: is bear
            return  oldest.Close < oldest.Open &&
                    //Second Candle: is doji, body below previous candle, high price above last low
                    (previous.Close >= previous.Open - (FivePercent * (previous.High - previous.Low))) ||
                    (previous.Close <= previous.Open + (FivePercent * (previous.High - previous.Low))) &&
                    previous.Open < oldest.Close && previous.Close < oldest.Close &&
                    previous.High > oldest.Low &&
                    //Third candle rules: is bull and closing price above midpoint of first body
                    current.Close > current.Open &&
                    current.Close >= halfwayLoss;

        }
        private static bool IsThreeOutsideUp(Candlestick oldest, Candlestick previous, Candlestick current, out string patternName)
        {
            patternName = "Three Outside Up";

                    //First Candle rules: is bear
            return  oldest.Close < oldest.Open &&
                    //Second Candle rules: engulfs first candle, is bull
                    previous.Close > previous.Open &&
                    previous.Open < oldest.Close && previous.Close > oldest.Open &&
                    //Third Candle rules: is bull, candle closes higher than last
                    current.Close > previous.Close &&
                    current.Close > current.Open;                   

        }

        //Bearish Three-Candle Patterns
        private static bool IsEveningStar(Candlestick oldest, Candlestick previous, Candlestick current, out string patternName)
        {
            patternName = "Evening Star";
            var halfwayGain = oldest.Close - (oldest.Close - oldest.Open / 2);

            //Oldest is a bull 
            return oldest.Close > oldest.Open &&
                    //Previous has a candle body above the oldest
                    previous.Open > oldest.Close &&
                    previous.Close > oldest.Close &&
                    //Current is a bear
                    current.Close < current.Open &&
                    //Current body opened lower than previous body
                    current.Open < previous.Open &&
                    current.Open < previous.Close &&
                    //Current close is <= oldest 50% gain
                    current.Close <= halfwayGain;
        }
        private static bool IsBearishAbandonedBaby(Candlestick oldest, Candlestick previous, Candlestick current, out string patternName)
        {
            patternName = "Bearish Abandoned Baby";
            //Oldest is a bull 
            return oldest.Close > oldest.Open &&
                    //Previous entire candle and shadow is above the oldest
                    previous.Low > oldest.High &&
                    //Previous is a doji
                    (previous.Close >= previous.Open - (FivePercent * (previous.High - previous.Low)) ||
                     previous.Close <= previous.Open + (FivePercent * (previous.High - previous.Low))) &&
                    //Current is a bear
                    current.Close < current.Open &&
                    //Current body stayed lower than previous body
                    current.High < previous.Low;
        }
        private static bool IsThreeBlackCrows(Candlestick oldest, Candlestick previous, Candlestick current, out string patternName)
        {
            patternName = "Three Black Crows";
            //Oldest is a bear 
            return oldest.Close < oldest.Open &&
                    //Oldest lower shadow is less than 10% of the trade value
                    oldest.Open - oldest.Low <= (TenPercent * (oldest.High - oldest.Low)) &&
                    //Oldest upper shadow is less than 10% of the trade value
                    oldest.High - oldest.Close <= (TenPercent * (oldest.High - oldest.Low)) &&

                    //Previous is a bear
                    previous.Close > previous.Open &&
                    //Previous lower shadow is less than 10% of the trade value
                    previous.Open - previous.Low <= (TenPercent * (previous.High - previous.Low)) &&
                    //Previous upper shadow is less than 10% of the trade value
                    previous.High - previous.Close <= (TenPercent * (previous.High - previous.Low)) &&
                    //Previous opened between oldest candle
                    previous.Open > oldest.Close && previous.Open < oldest.Open &&
                    //Previous closed lower than oldest
                    previous.Close < oldest.Close &&

                    //Current is a bear
                    current.Close < current.Open &&
                    //Current lower shadow is less than 10% of the trade value
                    current.Open - current.Low <= (TenPercent * (current.High - current.Low)) &&
                    //Current upper shadow is less than 10% of the trade value
                    current.High - current.Close <= (TenPercent * (current.High - current.Low)) &&
                    //Current opened between oldest candle
                    current.Open > previous.Close && current.Open < previous.Open &&
                    //Current closed higher than oldest
                    current.Close < previous.Close;
        }
        private static bool IsEveningDojiStar(Candlestick oldest, Candlestick previous, Candlestick current, out string patternName)
        {
            patternName = "Bearish Evening Doji Star";
            var halfwayGain = oldest.Close - (oldest.Close - oldest.Open / 2);

            //First Candle rules: is bull
            return oldest.Close > oldest.Open &&
                    //Second Candle: is doji, body above previous candle, low price above last high
                    (previous.Close >= previous.Open - (FivePercent * (previous.High - previous.Low))) ||
                    (previous.Close <= previous.Open + (FivePercent * (previous.High - previous.Low))) &&
                    previous.Open < oldest.Close && previous.Close < oldest.Close &&
                    previous.Low > oldest.High &&
                    //Third candle rules: is bear and closing price below midpoint of first body
                    current.Close < current.Open &&
                    current.Close <= halfwayGain;

        }
        private static bool IsThreeOutsideDown(Candlestick oldest, Candlestick previous, Candlestick current, out string patternName)
        {
            patternName = "Three Outside Down";

            //First Candle rules: is bull
            return oldest.Close > oldest.Open &&
                    //Second Candle rules: engulfs first candle, is bear
                    previous.Close < previous.Open &&
                    previous.Open > oldest.Close && previous.Close < oldest.Open &&
                    //Third Candle rules: is bear, candle closes lower than last
                    current.Close < previous.Close &&
                    current.Close < current.Open;

        }

    }
}
