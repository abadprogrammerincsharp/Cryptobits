using Contracts.Enums;
using Contracts.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Concrete
{
    public class ExchangeOrder
    {
        public ExchangeOrder() { }
        public ExchangeOrder(TradingPair pair, decimal price, decimal quantity, OrderSide side, DateTimeOffset orderPlaced, TimeInForce timeInForce = TimeInForce.GoodTillCanceled)
        {
            OrderId = pair.GetUppercaseSymbolPair() + orderPlaced.ToString("yyyyMMddhhmmssff");
            TradingPair = pair;
            Price = price;
            Quantity = quantity;
            Side = side;

            IcebergQuantity = (pair.MaxOrderSize > 0) && (quantity > pair.MaxOrderSize) ? pair.MaxOrderSize : null;
            TimeInForce = IcebergQuantity != null ? TimeInForce.GoodTillCanceled : timeInForce;

            TimestampInUnixMilliseconds = orderPlaced.ToUnixTimeMilliseconds();
        }



        public TradingPair TradingPair { get; set; }
        public string OrderId { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public OrderSide Side { get; set; }
        public TimeInForce TimeInForce { get; set; }

        public decimal? IcebergQuantity { get; set; }
        public long TimestampInUnixMilliseconds { get; set; }
    }
}
