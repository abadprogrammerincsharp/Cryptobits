using Contracts.Concrete;
using Contracts.Enums;
using Contracts.Extensions;
using DataInteraction.RestfulEntities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataInteraction.RestfulApis
{
    public partial class BinanceUSRestfulApi
    {
        const string OCOEndpoint = "/api/v3/order/oco";
        public long MaxSecondsToWaitForProcessing { get; set; }

        //IOrderApi
        public async Task<List<ExchangeOrderResult>> PutOcoOrder(ExchangeOrder limitOrder, ExchangeOrder stopLossOrder, ExchangeOrder stopLossLimitOrder = null)
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
                StopPrice = stopLossOrder.Price,
                StopIcebergQuantity = stopLossOrder.IcebergQuantity,
                StopClientOrderId = stopLossOrder.OrderId,
                StopLimitPrice = stopLossLimitOrder?.Price,
                StopLimitTimeInForce = stopLossLimitOrder?.GetTimeInForceAsBinanceString() ?? null,
                TimestampInUnixSeconds = limitOrder.TimestampInUnixMilliseconds,
                RecevingWindow = MaxSecondsToWaitForProcessing
            };

            var response = await SendRequestAsync(request, OCOEndpoint, Post, true);
            var responseAsEntity = JsonConvert.DeserializeObject<BinanceOcoResponseEntity>(response);
            var limitOrderResult = new ExchangeOrderResult();
            var stopLossOrderResult = new ExchangeOrderResult();

            throw new NotImplementedException();
        }
        public bool CanPlaceOcoOrder()
        {
            return CanMakeOrderApiCall(3, 2);
        }

    }
}
