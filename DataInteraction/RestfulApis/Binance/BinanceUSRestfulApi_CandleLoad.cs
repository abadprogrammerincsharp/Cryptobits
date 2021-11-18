using Contracts.Concrete;
using Contracts.Extensions;
using Contracts.Interfaces;
using DataInteraction.RestfulEntities.Binance;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataInteraction.RestfulApis.Binance
{
   public partial class BinanceUSRestfulApi : ICandleLoad
    {
        const string KlineInfoEndpoint = "/api/v3/klines";
        public int MinCandleQuantity { get { return 1000; } }
        public int MaxCandleQuantity { get { return 1000; } }

        //ICandleLoad
        public async Task<IEnumerable<Candlestick>> GetLatestCandlesAsync(TradingPair tradingPair, int quantity)
        {
            BinanceKlineRequestEntity request = new BinanceKlineRequestEntity()
            {
                Interval = tradingPair.GetBinanceIntervalString(),
                Limit = quantity > MaxCandleQuantity ? MaxCandleQuantity : quantity,
                Symbol = tradingPair.GetUppercaseSymbolPair()
            };

            var response = await SendRequestAsync(request, KlineInfoEndpoint);
            return TransformResponseToCandlesticks(tradingPair, response);
        }
        public bool CanQueryCandlestickData()
        {
            return CanMakeWeightedApiCall(10);
        }
        private List<Candlestick> TransformResponseToCandlesticks(TradingPair pair, string response)
        {
            dynamic jsonArray = JArray.Parse(response);
            List<Candlestick> candlesticks = new List<Candlestick>();
            for (int i = 0; i < jsonArray.Count; i++)
            {
                var element = jsonArray[i];
                candlesticks.Add(new Candlestick(
                    tradingPair: pair,
                    start: element[0],
                    open: element[1],
                    high: element[2],
                    low: element[3],
                    close: element[4],
                    tradeVolume: element[5],
                    end: element[6]
                    ));

            }
            return candlesticks;
        }
    }
}
