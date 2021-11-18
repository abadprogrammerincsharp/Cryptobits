using Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Concrete
{
    public class ExchangeOrderResult
    {
        public ExchangeOrderResult() { }
        public ExchangeOrderResult(TradingPair pair, OrderSide side, object price, object qtyRequested, object qtyExecuted)
        {
            TradingPair = pair;
            Side = side;
            Price = Convert.ToDecimal(price);
            QuantityRequested = Convert.ToDecimal(qtyRequested);
            QuantityExecuted = Convert.ToDecimal(qtyExecuted);
        }

        public TradingPair TradingPair { get; set; }
        public OrderSide Side { get; set; }
        public decimal Price { get; set; }
        public decimal QuantityRequested { get; set; }
        public decimal QuantityExecuted { get; set; }
        public OrderStatus OrderStatus { get; set; }

    }
}
