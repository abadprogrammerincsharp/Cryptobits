using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataInteraction.StreamEntities
{
    public class BinanceCandleEntity
    {
        [JsonProperty("e")]
        public string EventType { get; set; }
        [JsonProperty("E")]
        public long EventTime { get; set; }
        [JsonProperty("s")]
        public string Symbol { get; set; }
        [JsonProperty("k", Required = Required.Always)]
        public Kline Kline { get; set; }

    }
    public class Kline
    {
        [JsonProperty("t")]
        public long StartTime { get; set; }
        [JsonProperty("T")]
        public long CloseTime { get; set; }
        [JsonProperty("s")]
        public string Symbol { get; set; }
        [JsonProperty("i")]
        public string Interval { get; set; }
        [JsonProperty("f")]
        public long FirstTradeID { get; set; }
        [JsonProperty("L")]
        public long LastTradeID { get; set; }
        [JsonProperty("o")]
        public decimal OpenPrice { get; set; }
        [JsonProperty("c")]
        public decimal ClosePrice { get; set; }
        [JsonProperty("h")]
        public decimal HighPrice { get; set; }
        [JsonProperty("l")]
        public decimal LowPrice { get; set; }
        [JsonProperty("v")]
        public string BaseAssetVolume { get; set; }
        [JsonProperty("n")]
        public decimal NumberOfTrades { get; set; }
        [JsonProperty("x")]
        public bool IsOpen { get; set; }
        [JsonProperty("q")]
        public string QuoteAssetVolume { get; set; }
        [JsonProperty("V")]
        public string TakerBaseAssetVolume { get; set; }
        [JsonProperty("Q")]
        public string TakerQuoteAssetVolume { get; set; }
        [JsonProperty("B")]
        public string Ignore { get; set; }
    }


}
