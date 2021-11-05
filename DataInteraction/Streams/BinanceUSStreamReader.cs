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

namespace DataInteraction.Streams
{
    public class BinanceUSStreamReader : ICandleFeed
    {
        private string _streamName;
        private WebsocketClient _websocketClient;
        private bool disposedValue;
        private const string BinanceUSWebsocketAddress = "";
        private const int ConnectId = 1000, DisconnectId = 2000; //IDs are arbritrary, used simply to help separate messages
        private int _id = 0;
        private object _syncObject = new object();

        public TimeSpan Interval { get; set; }
        public ILogger Log { get; set; }
        public List<TradingPair> CurrentlySubscribed { get; set; } = new List<TradingPair>();

        public event EventHandler<FeedAvailibilityEvent> FeedAvailabilityChanged;
        public event EventHandler<Candlestick> RecievedCandlestickData;


        public async Task<bool> TryStartStream()
        {
            bool started = false;
            if (_websocketClient.IsRunning)
                return true;
            else if (_websocketClient == null || _websocketClient.Url != new Uri(BinanceUSWebsocketAddress))
            {
                Uri uri = new Uri(BinanceUSWebsocketAddress);
                Func<ClientWebSocket> factory = new Func<ClientWebSocket>(() =>
                {
                    ClientWebSocket wsClient = new ClientWebSocket
                    {
                        Options = {
                    KeepAliveInterval = TimeSpan.FromSeconds(30) // interval to send pong frames
                    }
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
                Log.Add($"Error: { ex.Message }");
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

            lock (_syncObject){
                _id = (_id == 999) ? 0 : _id + 1;
            }

            bool sentJustFine = false;
            string subscribeText = $"{{\"method\": \"SUBSCRIBE\",\"params\": [\"{GetCandlestickConnectionString(tradingPair)}\"],\"id\": {ConnectId + _id}}}";
            if (addToList)
                CurrentlySubscribed.Add(tradingPair);
            try
            {
                _websocketClient.Send(subscribeText);
                sentJustFine = true;
            }
            catch
            {
                sentJustFine = false;
            }

            return sentJustFine;
        }
        public void UnsubscribeFromCandleFeed(TradingPair tradingPair)
        {
            lock (_syncObject) {
                _id = (_id == 999) ? 0 : _id + 1;
            }

            string unsubscribeText = $"{{\"method\": \"UNSUBSCRIBE\",\"params\": [\"{GetCandlestickConnectionString(tradingPair)}\"],\"id\": {DisconnectId + _id}}}";
            CurrentlySubscribed.Remove(tradingPair);
            if (_websocketClient.IsRunning)
                _websocketClient.Send(unsubscribeText);
        }
        private string GetCandlestickConnectionString(TradingPair pair)
        {
            var interval = GetIntervalAsString(pair.CandlestickInterval);
            return $"{pair.BaseAssetSymbol.Trim().ToLower()}{pair.QuoteAssetSymbol.Trim().ToLower()}@kline_{interval}";
        }
        private string GetIntervalAsString(TimeSpan candlestickInterval)
        {
            var minutes = (int)candlestickInterval.TotalMinutes;
            var days = (int)candlestickInterval.TotalDays;
            string interval = null;
            switch (minutes)
            {
                case 1:
                    interval = "1m";
                    break;
                case 3:
                    interval = "3m";
                    break;
                case 5:
                    interval = "5m";
                    break;
                case 15:
                    interval = "15m";
                    break;
                case 30:
                    interval = "30m";
                    break;
                case 60:
                    interval = "1h";
                    break;
                case 120:
                    interval = "2h";
                    break;
                case 240:
                    interval = "4h";
                    break;
                case 360:
                    interval = "6h";
                    break;
                case 480:
                    interval = "8h";
                    break;
                case 720:
                    interval = "12h";
                    break;
                default:
                    switch (days)
                    {
                        case 1:
                            interval = "1d";
                            break;
                        case 3:
                            interval = "3d";
                            break;
                        case 7:
                            interval = "1w";
                            break;
                        case 28:
                        case 29:
                        case 30:
                        case 31:
                            interval = "1M";
                            break;
                        default:
                            throw new InvalidOperationException("Interval is not supported by Binance US");
                    }
                    break;

            }

            return interval;
        }
        private bool TryGetCandlestickFromEntity(BinanceUsCandleEntity entity, out Candlestick candlestick)
        {
            bool hasSubscriptionData = false;
            candlestick = null;
            try
            {
                var subscribedPair = CurrentlySubscribed.SingleOrDefault(x => entity.Symbol.ToUpper() == $"{x.BaseAssetSymbol.ToUpper()}{x.QuoteAssetSymbol.ToUpper()}" &&
                                                                              entity.Kline.Interval == GetIntervalAsString(x.CandlestickInterval));
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
                        TradingPair = subscribedPair
                    };
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
            List<TradingPair> unsubscribed = new List<TradingPair>();
            var initialCount = CurrentlySubscribed.Count;
            foreach (var subscription in CurrentlySubscribed)
            {
                if (!TrySubscribeToCandleFeed(subscription, false))
                {
                    FeedAvailabilityChanged?.Invoke(this, new FeedAvailibilityEvent() { IsAvailable = false, TradingPair = subscription });
                    unsubscribed.Add(subscription);
                }
            }

            CurrentlySubscribed = CurrentlySubscribed.Except(unsubscribed).ToList();
            if (CurrentlySubscribed.Count == initialCount)
                Log.Add("Reconnected");
            else if (CurrentlySubscribed.Count > 0)
                Log.Add("Partially Reconnected");
        }
        private void OnMessage(ResponseMessage message)
        {
            if (TryGetJsonObject<BinanceUsCandleEntity>(message.Text, out var candleEntity) &&
                TryGetCandlestickFromEntity(candleEntity, out var candlestick))
                RecievedCandlestickData?.Invoke(this, candlestick);             
            

            Log.Add(message.Text);
        }
        private void OnDisconnect(DisconnectionInfo info)
        {
            Log.Add("Diconnected");
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
            catch {
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
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~BinanceUsCandleStreamReader()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

    }
}
