using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contracts.Concrete;

namespace DataInteraction.RestfulEntities.Binance
{
    public class BinanceOrderRequestEntity
    {
        [ApiParameter("symbol")]
        public string Symbol { get; set; }

        [ApiParameter("side")]
        public string Side { get; set; }

        [ApiParameter("quantity")]
        public decimal? Quantity { get; set; }

        [ApiParameter("quoteOrderQty")]
        public decimal? QuoteOrderQuantity { get; set; }

        [ApiParameter("newClientOrderId")]
        public string ClientOrderId { get; set; }

        [ApiParameter("price")]
        public decimal? LimitPrice { get; set; }

        [ApiParameter("IcebergQty")]
        public decimal? IcebergQuantity { get; set; }

        [ApiParameter("stopPrice")]
        public decimal? StopLossPrice { get; set; }

        [ApiParameter("timeInForce")]
        public string TimeInForce { get; set; }

        [ApiParameter("newOrderRespType")]
        public string NewOrderRespType { get; set; }

        [ApiParameter("recvWindow")]
        public long RecevingWindow { get; set; }

        [ApiParameter("timestamp")]
        public long TimestampInUnixSeconds { get; set; }
    }
}
