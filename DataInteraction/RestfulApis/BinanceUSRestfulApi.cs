using Contracts.Concrete;
using Contracts.Interfaces;
using DataInteraction.RestfulEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Contracts.Extensions;
using Newtonsoft.Json;
using Contracts.Generics;

namespace DataInteraction.RestfulApis
{
    public class BinanceUSRestfulApi
    {
        const string BinanceUSApiServer = "https://api.binance.us";
        const string Get = "GET", Put = "PUT", Post = "POST", Delete = "DELETE";
        const string Second = "SECOND", Minute = "MINUTE", Hour = "HOUR", Day = "DAY";
        const string RequestWeight = "REQUEST_WEIGHT", Orders = "ORDERS", RawRequests = "RAW_REQUESTS";
        private const string ExchangeInfoEndpoint = "/api/v3/exchangeInfo",
                             KlineInfoEndpoint = "/api/v3/klines";
        List<ApiLimit> _apiLimits = new List<ApiLimit>();
        List<TradeSymbol> _tradeSymbols = new List<TradeSymbol>();
        DateTimeOffset _nextRequestTime = DateTimeOffset.MinValue;
        static HttpClient apiClient = new HttpClient();
        
        public ILogger Log { get; set; }

        public async Task GetExchangeDetailsAsync()
        {
            var webRequest = await SendExchangeInfoRequest();
            var exchangeInformation = webRequest.Item1;
            var httpResponse = webRequest.Item2;
            var headers = httpResponse.Headers;

            _tradeSymbols.Clear();
            _tradeSymbols.AddRange(exchangeInformation.Symbols);

            var rateLimits = exchangeInformation.RateLimits;
            _apiLimits.Clear();
            foreach (var rateLimit in rateLimits)
                _apiLimits.Add(GetApiLimit(rateLimit));

            UpdateApiLimits(headers);
        }
        public async Task<IEnumerable<Candlestick>> GetLatestCandlesAsync(TradingPair tradingPair, int quantity)
        {
            BinanceUSKlineRequestEntity request = new BinanceUSKlineRequestEntity()
            {
                Interval = tradingPair.GetBinanceIntervalString(),
                Limit = quantity > 1000 ? 1000 : quantity,
                Symbol = tradingPair.BaseAssetSymbol.ToUpper() + tradingPair.QuoteAssetSymbol.ToUpper()
            };

            throw new NotImplementedException();

            //return await SendRequestAsync(request, KlineInfoEndpoint);
        }

        //SendGetRequest<T>
        private async Task<Tuple<BinanceExchangeInfoResponseEntity, HttpResponseMessage>> SendExchangeInfoRequest()
        {
            var endpoint = BinanceUSApiServer + ExchangeInfoEndpoint;
            HttpResponseMessage response = await apiClient.GetAsync(endpoint);
            var responseString = await CheckResponse(response, false);
            var responseAsEntity = JsonConvert.DeserializeObject<BinanceExchangeInfoResponseEntity>(responseString);
            return Tuple.Create(responseAsEntity, response);
        }
        private async Task<string> SendRequestAsync(string endpoint, string requestType = Get)
        {
            var finalEndpoint = BinanceUSApiServer + (endpoint.StartsWith("/") ? endpoint : "/" + endpoint);
            HttpResponseMessage response = null;
            switch (requestType)
            {
                case Get:
                    response = await apiClient.GetAsync(finalEndpoint);
                    break;
                case Put:
                    response = await apiClient.PutAsync(finalEndpoint, null);
                    break;
                case Post:
                    response = await apiClient.PostAsync(finalEndpoint, null);
                    break;
                case Delete:
                    response = await apiClient.DeleteAsync(finalEndpoint);
                    break;
            }
            if (response is null)
                return null;

            var responseAsEntity = await CheckResponse(response);
            return responseAsEntity;
        }
        private async Task<string> SendRequestAsync<TRequest>(TRequest requestEntity, string endpoint, string requestType = Get)
        {
            HttpResponseMessage response = null;
            var finalEndpoint = BuildQueryString(endpoint, requestEntity);

            switch (requestType)
            {
                case Get:
                    response = await apiClient.GetAsync(finalEndpoint);
                    break;
                case Put:
                    response = await apiClient.PutAsync(finalEndpoint, null);
                    break;
                case Post:
                    response = await apiClient.PostAsync(finalEndpoint, null);
                    break;
                case Delete:
                    response = await apiClient.DeleteAsync(finalEndpoint);
                    break;
            }
            if (response is null)
                return null;

            var responseAsEntity = await CheckResponse(response);
            return responseAsEntity;
        }
        private string BuildQueryString<T>(string endpoint, T entity)
        {
            StringBuilder builder = new StringBuilder($"{BinanceUSApiServer}{(endpoint.StartsWith("/") ? endpoint : "/" + endpoint)}");
            try
            {
                var propertiesWithAttributes = entity.GetType().GetProperties().Where(x => Attribute.IsDefined(x, typeof(ApiParameterAttribute)));

                if (propertiesWithAttributes != null)
                {
                    builder.Append("?");
                    for (int i = 0; i < propertiesWithAttributes.Count(); i++)
                    {
                        var property = propertiesWithAttributes.ElementAt(i);
                        var propertyName = (ApiParameterAttribute)property.GetCustomAttributes(typeof(ApiParameterAttribute), true)[0];
                        var propertyValue = property.GetValue(entity);
                        builder.Append(propertyName.ParameterName + "=" + propertyValue.ToString());
                        if (i + 1 < propertiesWithAttributes.Count())
                            builder.Append("&");
                    }
                }
            }
            catch { Log?.Add("Unable to get properties on entity " + entity?.GetType().ToString() ?? "(null object)"); }

            return builder.ToString();
        }

        //CheckResponse<T>
        private async Task<string> CheckResponse(HttpResponseMessage response, bool checkApiLimits = true)
        {
            if (checkApiLimits)
                UpdateApiLimits(response.Headers);
            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests || (int)response.StatusCode == 418) //Server will return "I'm a teapot" if banned.
                HandleFourTwentyNineError(response.Headers);

           return await response.Content.ReadAsStringAsync();            
        }

        private ApiLimit GetApiLimit(RateLimit rateLimit)
        {
            ApiLimit apiLimit = new ApiLimit();
            apiLimit.CurrentCount = 0;
            apiLimit.Limit = rateLimit.Limit;
            apiLimit.LimitType = rateLimit.RateLimitType;
            switch (rateLimit.RateLimitType)
            {
                case Orders:
                    apiLimit.LimitHeader = "x-mbx-order-count-";
                    break;
                case RequestWeight:
                    apiLimit.LimitHeader = "x-mbx-used-weight-";
                    break;
                default:
                    apiLimit.LimitHeader = string.Empty;
                    break;
            }
            switch (rateLimit.Interval)
            {
                case Second:
                    apiLimit.Interval = TimeSpan.FromSeconds(rateLimit.IntervalNum);
                    apiLimit.LimitHeader += $"{rateLimit.IntervalNum}S";
                    break;
                case Minute:
                    apiLimit.Interval = TimeSpan.FromMinutes(rateLimit.IntervalNum);
                    apiLimit.LimitHeader += $"{rateLimit.IntervalNum}M";
                    break;
                case Hour:
                    apiLimit.Interval = TimeSpan.FromHours(rateLimit.IntervalNum);
                    apiLimit.LimitHeader += $"{rateLimit.IntervalNum}H";
                    break;
                case Day:
                    apiLimit.Interval = TimeSpan.FromDays(rateLimit.IntervalNum);
                    apiLimit.LimitHeader += $"{rateLimit.IntervalNum}D";
                    break;
            }
            
            apiLimit.NextReset = DateTimeOffset.Now + apiLimit.Interval;
            return apiLimit;
        }
        private void UpdateApiLimits(HttpResponseHeaders headers)
        {
            UpdateApiLimits(headers, _apiLimits);
        }
        private void UpdateApiLimits(HttpResponseHeaders headers, List<ApiLimit> limits)
        {
            for (int i = 0; i < limits.Count; i++)
                if (limits[i].LimitType == RawRequests)
                    limits[i].IncrementLimit();
                else
                    limits[i].UpdateLimit(headers);

        }

        private void HandleFourTwentyNineError(HttpResponseHeaders headers)
        {
           var retryAfter = headers.SingleOrDefault(x => x.Key.ToLower() == "retry-after");
           if (!retryAfter.Equals(default(KeyValuePair<string, IEnumerable<string>>)))
            {
                var secondsToWait = Convert.ToInt32(retryAfter.Value.First());
                _nextRequestTime = DateTimeOffset.Now + TimeSpan.FromSeconds(secondsToWait);
                Log?.Add("Received a 418/429 error. Cannot perform any API requests until " + _nextRequestTime.ToString(), Contracts.Enums.LoggingLevel.Critical);
            }
        }
        private bool CanPlaceRawRequest()
        {
            bool canPlaceRawRequest = true;
            var rawRequestLimits = _apiLimits.Where(x => x.LimitType == RawRequests);

            if ((rawRequestLimits?.Count() ?? 0) > 0)
            {
                var reachedMax = rawRequestLimits.Where(x => x.CurrentCount + 1 >= x.Limit);
                if ((reachedMax?.Count() ?? 0) > 0)
                    canPlaceRawRequest = false;
            }

            return canPlaceRawRequest && DateTimeOffset.UtcNow > _nextRequestTime;
        }
        private bool CanPlaceOrder()
        {
            bool canPlaceOrder = true;
            var orderLimits = _apiLimits.Where(x => x.LimitType == Orders);

            if ((orderLimits?.Count() ?? 0) > 0)
            {
                var reachedMax = orderLimits.Where(x => x.CurrentCount + 1 >= x.Limit);
                if ((reachedMax?.Count() ?? 0) > 0)
                    canPlaceOrder = false;
            }

            return canPlaceOrder;
        }
        private bool CanMakeApiCall(int weight)
        {
            bool canPlaceApiCall = true;
            var apiRequestLimits = _apiLimits.Where(x => x.LimitType == RequestWeight);

            if ((apiRequestLimits?.Count() ?? 0) > 0)
            {
                var reachedMax = apiRequestLimits.Where(x => x.CurrentCount + weight >= x.Limit);
                if ((reachedMax?.Count() ?? 0) > 0)
                    canPlaceApiCall = false;
            }

            return canPlaceApiCall;
        }

        private List<Candlestick> TransformResponseToCandlesticks (string response)
        {
            throw new NotImplementedException();
        }

        
    }
    
}
