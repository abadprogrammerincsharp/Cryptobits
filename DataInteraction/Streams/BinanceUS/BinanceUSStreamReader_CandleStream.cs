using Contracts.Concrete;
using Contracts.Enums;
using Contracts.Extensions;
using Contracts.Interfaces;
using DataInteraction.StreamEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataInteraction.Streams.BinanceUS
{
    public partial class BinanceUSStreamReader : ICandleFeed
    {
        public bool TrySubscribeToCandleFeed(TradingPair tradingPair)
        {
            return TrySubscribeToCandleFeed(tradingPair, true);
        }
        private bool TrySubscribeToCandleFeed(TradingPair tradingPair, bool addToList)
        {
            if (CurrentlySubscribed.Contains(tradingPair))
                return true;

            if (_totalStreams + 1 > 1024)
            {
                Log?.Add("Already subscribed to a maximum amount of streams for US Binance Stream");
                return false;
            }

            //Binance accepts maximum of 5 incoming messages per second. We will send a maximum of 3 + 1 pong to ensure we don't reach limit.
            lock (_syncObject)
            {
                _id = (_id == 999) ? 0 : _id + 1;
                Task.Delay(StreamMillisecondsDelay).Wait();
            }

            bool sentJustFine = false;
            string subscribeText = $"{{\"method\": \"SUBSCRIBE\",\"params\": [\"{GetCandlestickConnectionString(tradingPair)}\"],\"id\": {ConnectId + _id}}}";
            if (addToList)
                CurrentlySubscribed.Add(tradingPair);
            try
            {
                _websocketClient.Send(subscribeText);
                _totalStreams += 1;
                Log?.Add($"Subscribed to {tradingPair.GetUppercaseSymbolPair()}_{tradingPair.GetBinanceIntervalString()} on Binance US Stream");
                sentJustFine = true;
            }
            catch
            {
                Log?.Add($"Could not subscribe to {tradingPair.GetUppercaseSymbolPair()}_{tradingPair.GetBinanceIntervalString()}", LoggingLevel.Error);
                sentJustFine = false;
            }

            return sentJustFine;
        }
        public void UnsubscribeFromCandleFeed(TradingPair tradingPair)
        {
            lock (_syncObject)
            {
                _id = (_id == 999) ? 0 : _id + 1;
                Task.Delay(StreamMillisecondsDelay).Wait();
            }

            string unsubscribeText = $"{{\"method\": \"UNSUBSCRIBE\",\"params\": [\"{GetCandlestickConnectionString(tradingPair)}\"],\"id\": {DisconnectId + _id}}}";
            CurrentlySubscribed.Remove(tradingPair);
            if (_websocketClient.IsRunning)
                _websocketClient.Send(unsubscribeText);
            _totalStreams -= 1;
        }
        private string GetCandlestickConnectionString(TradingPair pair)
        {
            var interval = pair.GetBinanceIntervalString();
            return $"{pair.GetLowercaseSymbolPair()}@kline_{interval}";
        }
        private bool TryGetCandlestickFromEntity(BinanceCandleEntity entity, out Candlestick candlestick)
        {
            bool hasSubscriptionData = false;
            candlestick = null;
            try
            {
                var subscribedPair = CurrentlySubscribed.SingleOrDefault(x => entity.Symbol.ToUpper() == x.GetUppercaseSymbolPair() &&
                                                                              entity.Kline.Interval == x.GetBinanceIntervalString());
                if (subscribedPair != null)
                {
                    candlestick = new Candlestick()
                    {
                        Symbol = entity.Symbol,
                        Close = entity.Kline.ClosePrice,
                        Open = entity.Kline.OpenPrice,
                        High = entity.Kline.HighPrice,
                        Low = entity.Kline.LowPrice,
                        Start = DateTimeOffset.FromUnixTimeMilliseconds(entity.Kline.StartTime),
                        End = DateTimeOffset.FromUnixTimeMilliseconds(entity.Kline.CloseTime),
                        TradeVolume = entity.Kline.NumberOfTrades,
                        TradingPair = subscribedPair,
                        IsOpen = entity.Kline.IsOpen
                    };
                    hasSubscriptionData = true;
                }
            }
            catch
            {
                candlestick = null;
                hasSubscriptionData = false;
            }

            return hasSubscriptionData;
        }
    }
}
