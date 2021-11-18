using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contracts.Concrete;

namespace DataInteraction.RestfulEntities
{
    public class BinanceOcoRequestEntity
    {
        [ApiParameter("symbol")]
        public string Symbol { get; set; }

        [ApiParameter("listClientOrderId")]
        public string ListClientOrderId { get; set; }

        [ApiParameter("side")]
        public string Side { get; set; }

        [ApiParameter("quantity")]
        public decimal Quantity { get; set; }

        [ApiParameter("limitClientOrderId")]
        public string LimitClientOrderId { get; set; }

        [ApiParameter("price")]
        public decimal LimitPrice { get; set; }

        [ApiParameter("limitIcebergQty")]
        public decimal? LimitIcebergQuantity { get; set; }

        [ApiParameter("stopClientOrderId")]
        public string StopClientOrderId { get; set; }

        [ApiParameter("stopPrice")]
        public decimal StopPrice { get; set; }

        [ApiParameter("stopLimitPrice")]
        public decimal? StopLimitPrice { get; set; }

        [ApiParameter("stopIcebergQty")]
        public decimal? StopIcebergQuantity { get; set; }

        [ApiParameter("stopLimitTimeInForce")]
        public string StopLimitTimeInForce { get; set; }

        [ApiParameter("newOrderRespType")]
        public string NewOrderRespType { get; set; }

        [ApiParameter("recvWindow")]
        public long RecevingWindow { get; set; }

        [ApiParameter("timestamp")]
        public long TimestampInUnixSeconds { get; set; }
    }
}
