using System;
using System.Threading.Tasks;
using DataInteraction.RestfulApis;
using DataInteraction.Streams;
using DataProcessing.Indicators;
using Contracts.Concrete;
using Contracts.Interfaces;
using Newtonsoft.Json;
using DataInteraction.RestfulEntities;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ConsoleApps.InteractiveDebugging
{
    public class StreamTester
    {
        public static async Task Main(string[] args)
        {
            var api = new BinanceUSRestfulApi();
            var pair = new TradingPair()
            {
                BaseAsset = "BTC",
                QuoteAsset = "USD",
                CandlestickInterval = TimeSpan.FromMinutes(1),
                Exchange = "Binance US"
            };
            var stream = new BinanceUSStreamReader();
            var consoleLogger = new Logging.ConsoleLogging.ConsoleLogger();

            api.Log = consoleLogger;
            stream.Log = consoleLogger;


            ICandlestickIndicator rawIndicator = new RawDataIndicator(pair) { DataFeed = stream, DataLoad = api };
            rawIndicator.IndicatorChanged += (sender, e) => IndicatorChanged(sender, e, rawIndicator, "RAW");
            ICandlestickIndicator macdIndicator = new MacdCandlestickIndicator(pair, 12, 26, 9) { DataFeed = stream, DataLoad = api };
            macdIndicator.IndicatorChanged += (sender, e) => IndicatorChanged(sender, e, macdIndicator, "MACD");


            await rawIndicator.StartDataFeedAsync();
            await macdIndicator.StartDataFeedAsync();

            await Task.Delay(-1);
        }

        private static void IndicatorChanged(object sender, EventArgs e, ICandlestickIndicator indicator, string indicatorType)
        {
            Console.WriteLine();
            Console.WriteLine($"---Indicator Type: {indicatorType} " + indicator.ToString());
            Console.WriteLine();
        }

    }
}
