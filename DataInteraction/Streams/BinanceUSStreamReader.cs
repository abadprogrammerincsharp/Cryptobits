using System;
using System.Threading.Tasks;
using Contracts.Interfaces;
using Contracts.Concrete;
using Websocket.Client;
using Websocket.Client.Models;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Linq;
using DataInteraction.StreamEntities;
using Contracts.Enums;
using Contracts.Extensions;

namespace DataInteraction.Streams
{
    public class BinanceUSStreamReader : ICandleFeed
    {
        private WebsocketClient _websocketClient;
        private bool disposedValue;
        private const string BinanceUSWebsocketAddress = @"wss://stream.binance.us:9443/ws";
        private const int ConnectId = 1000, DisconnectId = 2000; //IDs are arbritrary, used simply to help separate messages
        private const int StreamMillisecondsDelay = 334; //Binance accepts maximum of 5 incoming messages per second. We will send a maximum of 3 + 1 pong to ensure we don't reach limit.
        private int _id = 0, _totalStreams = 0;
        private object _syncObject = new object();

        public ILogger Log { get; set; }
        public List<TradingPair> CurrentlySubscribed { get; set; } = new List<TradingPair>();
        public event EventHandler<CandleFeedAvailabilityEvent> CandleFeedAvailabilityChanged;
        public event EventHandler<Candlestick> ReceivedCandlestickData;


        public async Task<bool> TryStartStream()
        {
            bool started = false;
            if (_websocketClient?.IsRunning ?? false)
                return true;
            else if (_websocketClient == null || _websocketClient.Url != new Uri(BinanceUSWebsocketAddress))
            {
                Uri uri = new Uri(BinanceUSWebsocketAddress);
                Func<ClientWebSocket> factory = new Func<ClientWebSocket>(() =>
                {
                    ClientWebSocket wsClient = new ClientWebSocket
                    {
                        Options = { KeepAliveInterval = TimeSpan.FromSeconds(30) /*interval to send pong frames*/}
                    };

                    return wsClient;
                });

                _websocketClient = new WebsocketClient(uri, factory);
                _websocketClient.Name = "BinanceUSClient";
                _websocketClient.ReconnectTimeout = TimeSpan.FromSeconds(30);
                _websocketClient.ErrorReconnectTimeout = TimeSpan.FromSeconds(30);
                _websocketClient.ReconnectionHappened.Subscribe(OnReconnection);
                _websocketClient.DisconnectionHappened.Subscribe(OnDisconnect);

                _websocketClient.MessageReceived
                    .Where((ResponseMessage message) => message.MessageType == WebSocketMessageType.Text)
                    .Subscribe(OnMessage);
            }

            try
            {
                await _websocketClient.StartOrFail();
                started = true;
            }
            catch (Exception ex)
            {
                Log?.Add($"Error: { ex.Message }");
                started = false;
            }

            return started;
        }
        public void StopStream()
        {
            _websocketClient.Stop(WebSocketCloseStatus.NormalClosure, "Closing feed");
        }

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

            lock (_syncObject){
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
                Log?.Add($"Subscribed to {tradingPair.GetUppercaseSymbolPair()}_{tradingPair.GetBinanceIntervalString()} on US Binance Stream");
                sentJustFine = true;
            }
            catch
            {
                Log?.Add("Could not subscribe to {tradingPair.QuoteAssetSymbol}{tradingPair.BaseAssetSymbol}_{GetIntervalAsString(tradingPair.CandlestickInterval)}", LoggingLevel.Error);
                sentJustFine = false;
            }

            return sentJustFine;
        }
        public void UnsubscribeFromCandleFeed(TradingPair tradingPair)
        {
            lock (_syncObject) {
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
        
        private bool TryGetCandlestickFromEntity(BinanceUsCandleEntity entity, out Candlestick candlestick)
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

        private void OnReconnection(ReconnectionInfo info)
        {
            foreach (var pair in CurrentlySubscribed)
                CandleFeedAvailabilityChanged?.Invoke(this, new CandleFeedAvailabilityEvent() { IsAvailable = true, TradingPair = pair });
            Log?.Add("Reconnected to US Binance Stream");

            //List<TradingPair> unsubscribed = new List<TradingPair>();
            //var initialCount = CurrentlySubscribed.Count;
            //foreach (var subscription in CurrentlySubscribed)
            //{
            //    if (!TrySubscribeToCandleFeed(subscription, false))
            //    {
            //        FeedAvailabilityChanged?.Invoke(this, new FeedAvailibilityEvent() { IsAvailable = false, TradingPair = subscription });
            //        unsubscribed.Add(subscription);
            //    }
            //}

            //CurrentlySubscribed = CurrentlySubscribed.Except(unsubscribed).ToList();
            //if (CurrentlySubscribed.Count == initialCount)
            //    Log.Add("Reconnected");
            //else if (CurrentlySubscribed.Count > 0)
            //    Log.Add("Partially Reconnected");
        }
        private void OnMessage(ResponseMessage message)
        {
            if (TryGetJsonObject<BinanceUsCandleEntity>(message.Text, out var candleEntity) &&
                TryGetCandlestickFromEntity(candleEntity, out var candlestick))
                ReceivedCandlestickData?.Invoke(this, candlestick);           
            
            Log?.Add("Received message: " + message.Text);
        }
        private void OnDisconnect(DisconnectionInfo info)
        {
            foreach (var pair in CurrentlySubscribed)
                CandleFeedAvailabilityChanged(this, new CandleFeedAvailabilityEvent() { IsAvailable = false, TradingPair = pair });
            Log?.Add("Disconnected from US Binance Stream");
        }
        private bool TryGetJsonObject<T>(string message, out T objectReturned)
        {
            bool success = false;
            objectReturned = default(T);

            try {
                objectReturned = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(message);
                if (!EqualityComparer<T>.Default.Equals(objectReturned, default(T)))
                    success = true;
            }
            catch (Exception ex) {
                success = false;
            }

            return success;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _websocketClient.Dispose();
                    CurrentlySubscribed.Clear();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                CurrentlySubscribed = null;
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
