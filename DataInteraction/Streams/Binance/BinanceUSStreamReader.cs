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
using DataInteraction.StreamEntities.Binance;
using Contracts.Enums;
using Contracts.Extensions;

namespace DataInteraction.Streams.Binance
{
    public partial class BinanceUSStreamReader 
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
            if (TryGetJsonObject<BinanceCandleEntity>(message.Text, out var candleEntity) &&
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
