using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contracts.Concrete;
using Contracts.Generics;
using Contracts.Enums;
using DataProcessing.Signals;
using DataProcessing.Indicators;
using Contracts.Interfaces;

namespace DataProcessing.Strategies
{
    public class IndicatorMatchPatternStrategy
    {
        private TradingPair _pair;
        private CircularBuffer<MarketSignal> _patternSignals = new CircularBuffer<MarketSignal>(3);
        private CircularBuffer<MarketSignal> _indicatorSignals = new CircularBuffer<MarketSignal>(3);
        private CircularBuffer<Candlestick> _tradingCandlesticks = new CircularBuffer<Candlestick>(3);
        private object lockObject = new object();
        private bool _sevenUpdated = false, _fourteenUpdated = false, _twentyEightUpdated = false, 
                     _macdUpdated = false, _rsiUpdated = false, _candlesUpdated = false;


        //EMA 7/14/28 (current period * ~10) will determine trend
        //Candle pattern highlights entry/exit, MACD and RSI confirm
        //Volume valdiates movement against the trend. 

        ICandleFeed _candleStream;
        ICandleLoad _candleLoad;
        ILogger _logger;

        EmaCandlestickIndicator _seven, _fourteen, _twentyEight;
        MacdCandlestickIndicator _macd;
        RsiCandlestickIndicator _rsi;

        private int _oversold, _overbought;


        public event EventHandler<MarketSignal> MarketSignalUpdated;
        public event EventHandler<string> CandlestickPatternFound;

        private void InitializeComponents(TradingPair pair, TradingPair pairAtTenXPeriod)
        {
            _candleStream.ReceivedCandlestickData += ReceivedCandlestickData;


            _seven = new EmaCandlestickIndicator(pairAtTenXPeriod, 7);
            _seven.IndicatorChanged += (sender, e) => MarkIndicatorUpdated(ref _sevenUpdated);
            _seven.DataFeed = _candleStream;
            _seven.DataLoad = _candleLoad;

            _fourteen = new EmaCandlestickIndicator(pairAtTenXPeriod, 14);
            _fourteen.IndicatorChanged += (sender, e) => MarkIndicatorUpdated(ref _fourteenUpdated);
            _fourteen.DataFeed = _candleStream;
            _fourteen.DataLoad = _candleLoad;

            _twentyEight = new EmaCandlestickIndicator(pairAtTenXPeriod, 28);
            _twentyEight.IndicatorChanged += (sender, e) => MarkIndicatorUpdated(ref _twentyEightUpdated);
            _twentyEight.DataFeed = _candleStream;
            _twentyEight.DataLoad = _candleLoad;

            _macd = new MacdCandlestickIndicator(pair, 12, 24, 9);
            _macd.IndicatorChanged += (sender, e) => MarkIndicatorUpdated(ref _macdUpdated);
            _macd.DataFeed = _candleStream;
            _macd.DataLoad = _candleLoad;

            _rsi = new RsiCandlestickIndicator(pair, 12);
            _rsi.IndicatorChanged += (sender, e) => MarkIndicatorUpdated(ref _rsiUpdated);
            _rsi.DataFeed = _candleStream;
            _rsi.DataLoad = _candleLoad;

            _candleStream.ReceivedCandlestickData += (sender, e) => MarkIndicatorUpdated(ref _candlesUpdated);
        }

        private void ReceivedCandlestickData(object sender, Candlestick e)
        {
            if (e.TradingPair == _pair)
                _tradingCandlesticks.Add(e);
        }

        private void MarkIndicatorUpdated(ref bool state)
        {
            lock (lockObject)
            {
                state = true;
                if (_sevenUpdated && _fourteenUpdated && _twentyEightUpdated && _macdUpdated && _rsiUpdated && _candlesUpdated)
                {
                    GetMarketSignal();
                    _sevenUpdated = false;
                    _fourteenUpdated = false;
                    _twentyEightUpdated = false;
                    _macdUpdated = false;
                    _rsiUpdated = false;
                    _candlesUpdated = false;
                }
            }
        }

        private void GetMarketSignal() 
        {
            if (_tradingCandlesticks.Count < 3)
                return;

            var candles = _tradingCandlesticks.ToList();
            var lastSignal = _patternSignals.Count > 0 ? _patternSignals.ToList().Last() : MarketSignal.Neutral;

            var patternSignal = GetMarketSignalFromCandles(candles[0], candles[1], candles[2], lastSignal);
            var indicatorSignal = GetMarketSignalFromIndicators(candles[1], candles[2], _seven, _fourteen, _twentyEight, _macd, _rsi, _oversold, _overbought);

            if (IndicatorMatchesRecentPattern(indicatorSignal, patternSignal))
                MarketSignalUpdated?.Invoke(this, patternSignal);  
        }

        private bool IndicatorMatchesRecentPattern (MarketSignal indicatorSignal, MarketSignal patternSignal)
        {
            bool signalsMatch = indicatorSignal == patternSignal;
            
            if (!signalsMatch)
            {
                var patterns = _patternSignals.ToList();
                for (int i = 0; i < patterns.Count() && !signalsMatch; i++)
                    signalsMatch = patterns[i] == indicatorSignal;                
            }

            _patternSignals.Add(patternSignal);
            _indicatorSignals.Add(indicatorSignal);
            return signalsMatch;
        }

        private MarketSignal GetMarketSignalFromCandles(Candlestick oldest, Candlestick middle, Candlestick newest, MarketSignal previousTrend, bool mentionPattern = true)
        {
            var candleSignal = CandlestickPatternMarketSignal.GetMarketSignalFromThreeCandles(oldest, middle, newest, previousTrend, out var patternName);
            if (candleSignal == previousTrend)
                candleSignal = CandlestickPatternMarketSignal.GetMarketSignalFromTwoCandles(middle, newest, previousTrend, out patternName);

            if (candleSignal != previousTrend && mentionPattern)
                CandlestickPatternFound?.Invoke(this, patternName);

            return candleSignal;
        }
        private MarketSignal GetMarketSignalFromIndicators(Candlestick previous, Candlestick latest,
                                             EmaCandlestickIndicator seven, EmaCandlestickIndicator fourteen, EmaCandlestickIndicator twentyEight,
                                             MacdCandlestickIndicator macd, RsiCandlestickIndicator rsi, int oversold, int overbought)
        {
            //Two confirmed points:
            // 1) Current trend + Matching Entry/Exit for going long (signals continuation)
            // 2) Current trend + Opposing Entry/Exit for going long + strong momentum (signals reversal)

            var emaMarketSignal = EMAMarketSignal.GetMarketSignal(latest.Close, seven, fourteen, twentyEight);
            var macdMarketSignal = MacdMarketSignal.GetMarketSignal(macd);
            var rsiMarketSignal = RSIMarketSignal.GetMarketSignal(rsi, oversold, overbought);
            var returnValue = MarketSignal.Neutral;

            switch (emaMarketSignal) 
            {
                /*********Upward Continuation*********/
                case > MarketSignal.Neutral when macdMarketSignal > MarketSignal.Neutral || rsiMarketSignal > MarketSignal.Neutral:
                    returnValue = ClarifyUpwardContinuation(macdMarketSignal, rsiMarketSignal);
                    break;

                /*********Upward Reversal*********/
                case < MarketSignal.Neutral when macdMarketSignal > MarketSignal.Neutral || rsiMarketSignal > MarketSignal.Neutral:
                    returnValue = ClarifyUpwardReversal(previous, latest, macdMarketSignal, rsiMarketSignal, returnValue);
                    break;

                /*********Downward Continuation*********/
                case < MarketSignal.Neutral when macdMarketSignal < MarketSignal.Neutral || rsiMarketSignal < MarketSignal.Neutral:
                    returnValue = ClarifyDownwardContinuation(macdMarketSignal, rsiMarketSignal);
                    break;

                /*********Downward Reversal*********/
                case > MarketSignal.Neutral when macdMarketSignal < MarketSignal.Neutral || rsiMarketSignal < MarketSignal.Neutral:
                    returnValue = ClarifyDownwardReversal(previous, latest, macdMarketSignal, rsiMarketSignal, returnValue);
                    break;

                default:
                    returnValue = MarketSignal.Neutral;
                    break;
            }

            return returnValue;
        }

        private static MarketSignal ClarifyDownwardReversal(Candlestick previous, Candlestick latest, MarketSignal macdMarketSignal, MarketSignal rsiMarketSignal, MarketSignal returnValue)
        {
            //Strong trend found in multiple indicators
            if (macdMarketSignal <= MarketSignal.StrongBearContinuation && rsiMarketSignal <= MarketSignal.StrongBearContinuation)
                returnValue = MarketSignal.StrongBearContinuation;

            //Both RSI and MACD are <= BearishContinuation
            else if (macdMarketSignal <= MarketSignal.BearishContinuation && rsiMarketSignal <= MarketSignal.BearishContinuation)
                returnValue = MarketSignal.BearishContinuation;

            //At least one of RSI or MACD is a reversal, and volume confirms reversal
            else if (VolumeMomentumSignal.GetMomentumSignal(latest, previous) > MomentumSignal.Steady) //Volume is increasing, suggesting a reversal is correct.
                returnValue = MarketSignal.BearishReversal;
            return returnValue;
        }
        private static MarketSignal ClarifyDownwardContinuation(MarketSignal macdMarketSignal, MarketSignal rsiMarketSignal)
        {
            MarketSignal returnValue;
            //Both RSI and MACD are <= StrongBearContinuation
            if (macdMarketSignal <= MarketSignal.StrongBearContinuation && rsiMarketSignal <= MarketSignal.StrongBearContinuation)
                returnValue = MarketSignal.StrongBearContinuation;

            //Both RSI and MACD are <= BearishContinuation
            else if (macdMarketSignal <= MarketSignal.BearishContinuation && rsiMarketSignal <= MarketSignal.BearishContinuation)
                returnValue = MarketSignal.BearishContinuation;

            else
                returnValue = MarketSignal.BearishReversal;
            return returnValue;
        }
        private static MarketSignal ClarifyUpwardReversal(Candlestick previous, Candlestick latest, MarketSignal macdMarketSignal, MarketSignal rsiMarketSignal, MarketSignal returnValue)
        {
            //Both RSI and MACD are >= StrongBullContinuation
            if (macdMarketSignal >= MarketSignal.StrongBullContinuation && rsiMarketSignal >= MarketSignal.StrongBullContinuation)
                returnValue = MarketSignal.StrongBullContinuation;

            //Both RSI and MACD are >= BullishContinuation
            else if (macdMarketSignal >= MarketSignal.BullishContinuation && rsiMarketSignal >= MarketSignal.BullishContinuation)
                returnValue = MarketSignal.BullishContinuation;

            //At least one of RSI or MACD is a reversal, and volume confirms reversal
            else if (VolumeMomentumSignal.GetMomentumSignal(latest, previous) > MomentumSignal.Steady) //Volume is increasing, suggesting a reversal is correct.
                returnValue = MarketSignal.BullishReversal;
            return returnValue;
        }
        private static MarketSignal ClarifyUpwardContinuation(MarketSignal macdMarketSignal, MarketSignal rsiMarketSignal)
        {
            MarketSignal returnValue;
            //Both RSI and MACD are >= StrongBullContinuation
            if (macdMarketSignal >= MarketSignal.StrongBullContinuation && rsiMarketSignal >= MarketSignal.StrongBullContinuation)
                returnValue = MarketSignal.StrongBullContinuation;

            //Both RSI and MACD are >= BullishContinuation
            else if (macdMarketSignal >= MarketSignal.BullishContinuation && rsiMarketSignal >= MarketSignal.BullishContinuation)
                returnValue = MarketSignal.BullishContinuation;

            else
                returnValue = MarketSignal.BullishReversal;
            return returnValue;
        }

        
    }
}
