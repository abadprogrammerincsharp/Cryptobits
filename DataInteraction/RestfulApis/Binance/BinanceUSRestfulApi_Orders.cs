using Contracts.Concrete;
using Contracts.Enums;
using Contracts.Extensions;
using DataInteraction.RestfulEntities.Binance;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DataInteraction.RestfulApis.Binance
{
    public partial class BinanceUSRestfulApi
    {
        const string OCOEndpoint = "/api/v3/order/oco";
        const string OrderEndpoint = "/api/v3/order";
        public long MaxSecondsToWaitForProcessing { get; set; }

        //IOrderApi
        public async Task<ExchangeOrderResult> PutOrder(ExchangeOrder order) 
        {
            return await PlaceOrderRequest(order, Put);
        }
        public async Task<ExchangeOrderResult> GetOrder(ExchangeOrder order)
        {
            return await PlaceOrderRequest(order, Get);
        }
        public async Task<ExchangeOrderResult> DeleteOrder(ExchangeOrder order)
        {
            return await PlaceOrderRequest(order, Delete);
        }
        public async Task<List<ExchangeOrderResult>> PutOcoOrder(ExchangeOrder limitOrder, ExchangeOrder stopLossOrder, decimal? stopLossLimit = null)
        {
            var request = new BinanceOcoRequestEntity()
            {
                Quantity = limitOrder.Quantity,
                Side = limitOrder.Side == OrderSide.Buy ? "BUY" : "SELL",
                Symbol = limitOrder.TradingPair.GetUppercaseSymbolPair(),
                ListClientOrderId = "LIST-" + limitOrder.OrderId,

                LimitClientOrderId = limitOrder.OrderId,
                LimitIcebergQuantity = limitOrder.IcebergQuantity,
                LimitPrice = limitOrder.Price,
                StopLossPrice = stopLossOrder.Price,
                StopIcebergQuantity = stopLossOrder.IcebergQuantity,
                StopClientOrderId = stopLossOrder.OrderId,
                StopLimitPrice = stopLossOrder.StopLossLimitPrice,
                StopLimitTimeInForce = stopLossOrder.StopLossLimitPrice != null ? stopLossOrder.GetTimeInForceAsBinanceString() : null,
                TimestampInUnixSeconds = limitOrder.TimestampInUnixMilliseconds,
                RecevingWindow = MaxSecondsToWaitForProcessing
            };

            var response = await SendRequestAsync(request, OCOEndpoint, Post, true);
            var responseAsEntity = JsonConvert.DeserializeObject<BinanceOcoResponseEntity>(response);
            var limitOrderResponse = responseAsEntity.OrderReports.Single(x => x.ClientOrderId == limitOrder.OrderId);
            var stopOrderResponse = responseAsEntity.OrderReports.Single(x => x.ClientOrderId == stopLossOrder.OrderId);

            var limitOrderResult = new ExchangeOrderResult(limitOrder.TradingPair, limitOrder.Side, limitOrderResponse.Price, limitOrderResponse.OrigQty, limitOrderResponse.ExecutedQty);
            var stopLossOrderResult = new ExchangeOrderResult(stopLossOrder.TradingPair, stopLossOrder.Side, stopOrderResponse.Price, stopOrderResponse.OrigQty, stopOrderResponse.ExecutedQty);

            limitOrderResult.SetBinanceOrderStatus(limitOrderResponse.Status);
            stopLossOrderResult.SetBinanceOrderStatus(stopOrderResponse.Status);

            return new List<ExchangeOrderResult>(){limitOrderResult, stopLossOrderResult};
        }
        public bool CanPlaceOcoOrder()
        {
            return CanMakeOrderApiCall(2, 2);
        }
        
        private async Task<ExchangeOrderResult> PlaceOrderRequest(ExchangeOrder order, string httpMethod)
        {
            var request = new BinanceOrderRequestEntity()
            {
                ClientOrderId = order.OrderId,
                IcebergQuantity = order.IcebergQuantity,
                LimitPrice = order.Price,
                Quantity = order.Quantity,
                RecevingWindow = MaxSecondsToWaitForProcessing,
                Side = order.Side == OrderSide.Buy ? "BUY" : "SELL",
                StopLossPrice = order.StopLossLimitPrice,
                QuoteOrderQuantity = order.QuoteOrderQuantity,
                Symbol = order.TradingPair.GetUppercaseSymbolPair(),
                TimeInForce = order.GetTimeInForceAsBinanceString()
            };

            var response = await SendRequestAsync(request, OrderEndpoint, httpMethod, true);
            var responseAsEntity = JsonConvert.DeserializeObject<BinanceOrderResponseEntity>(response);

            var orderResult = new ExchangeOrderResult(order.TradingPair, order.Side, responseAsEntity.Price, responseAsEntity.OrigQty, responseAsEntity.ExecutedQty);
            orderResult.SetBinanceOrderStatus(responseAsEntity.Status);

            return orderResult;
        }
    }
}
