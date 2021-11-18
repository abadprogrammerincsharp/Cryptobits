using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataInteraction.RestfulEntities
{
    public class BinanceOcoResponseEntity
    {
        [JsonProperty("orderListId")]
        public int OrderListId { get; set; }

        [JsonProperty("contingencyType")]
        public string ContingencyType { get; set; }

        [JsonProperty("listStatusType")]
        public string ListStatusType { get; set; }

        [JsonProperty("listOrderStatus")]
        public string ListOrderStatus { get; set; }

        [JsonProperty("listClientOrderId")]
        public string ListClientOrderId { get; set; }

        [JsonProperty("transactionTime")]
        public long TransactionTime { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("orders")]
        public List<Order> Orders { get; set; }

        [JsonProperty("orderReports")]
        public List<BinanceOrderResponseEntity> OrderReports { get; set; }

    }
    public class Order
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("orderId")]
        public int OrderId { get; set; }

        [JsonProperty("clientOrderId")]
        public string ClientOrderId { get; set; }
    }

    
}
