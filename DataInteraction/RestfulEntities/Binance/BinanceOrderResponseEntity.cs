using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataInteraction.RestfulEntities.Binance
{
    public class BinanceOrderResponseEntity
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("orderId")]
        public int OrderId { get; set; }

        [JsonProperty("orderListId")]
        public int OrderListId { get; set; }

        [JsonProperty("clientOrderId")]
        public string ClientOrderId { get; set; }

        [JsonProperty("transactTime")]
        public object TransactTime { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("origQty")]
        public string OrigQty { get; set; }

        [JsonProperty("executedQty")]
        public string ExecutedQty { get; set; }

        [JsonProperty("cummulativeQuoteQty")]
        public string CummulativeQuoteQty { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("timeInForce")]
        public string TimeInForce { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("side")]
        public string Side { get; set; }

        [JsonProperty("stopPrice")]
        public string StopPrice { get; set; }
    }
}
