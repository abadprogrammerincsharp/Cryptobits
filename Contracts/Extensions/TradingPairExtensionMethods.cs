using Contracts.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Extensions
{
    public static class TradingPairExtensionMethods
    {
        public static string GetBinanceIntervalString(this TradingPair tradingPair)
        {
            var candlestickInterval = tradingPair.CandlestickInterval;

            var minutes = (int)candlestickInterval.TotalMinutes;
            var days = (int)candlestickInterval.TotalDays;
            string interval = null;
            switch (minutes)
            {
                case 1:
                    interval = "1m";
                    break;
                case 3:
                    interval = "3m";
                    break;
                case 5:
                    interval = "5m";
                    break;
                case 15:
                    interval = "15m";
                    break;
                case 30:
                    interval = "30m";
                    break;
                case 60:
                    interval = "1h";
                    break;
                case 120:
                    interval = "2h";
                    break;
                case 240:
                    interval = "4h";
                    break;
                case 360:
                    interval = "6h";
                    break;
                case 480:
                    interval = "8h";
                    break;
                case 720:
                    interval = "12h";
                    break;
                default:
                    switch (days)
                    {
                        case 1:
                            interval = "1d";
                            break;
                        case 3:
                            interval = "3d";
                            break;
                        case 7:
                            interval = "1w";
                            break;
                        case 28:
                        case 29:
                        case 30:
                        case 31:
                            interval = "1M";
                            break;
                        default:
                            throw new InvalidOperationException("Interval is not supported by Binance US");
                    }
                    break;

            }

            return interval;
        }
        public static string GetUppercaseSymbolPair(this TradingPair tradingPair, string separator ="")
        {
            return tradingPair.BaseAsset.ToUpper().Trim() + separator + tradingPair.QuoteAsset.ToUpper().Trim();
        }
        public static string GetLowercaseSymbolPair(this TradingPair tradingPair, string separator = "")
        {
            return tradingPair.BaseAsset.ToLower().Trim() + separator + tradingPair.QuoteAsset.ToLower().Trim();
        }
    }
}
