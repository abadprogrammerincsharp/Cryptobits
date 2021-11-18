using Contracts.Concrete;
using Contracts.Interfaces;
using DataInteraction.RestfulEntities.Binance;
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
using Newtonsoft.Json.Linq;
using Contracts.Enums;
using System.Reflection;

namespace DataInteraction.RestfulApis.Binance
{
    public partial class BinanceUSRestfulApi 
    {
        const string BinanceUSApiServer = "https://api.binance.us";
        const string Get = "GET", Put = "PUT", Post = "POST", Delete = "DELETE";
        const string Second = "SECOND", Minute = "MINUTE", Hour = "HOUR", Day = "DAY";
        const string RequestWeight = "REQUEST_WEIGHT", Orders = "ORDERS", RawRequests = "RAW_REQUESTS";
        private const string ExchangeInfoEndpoint = "/api/v3/exchangeInfo";
        List<ApiLimit> _apiLimits = new List<ApiLimit>();
        private ApiSecret _apiSecret;
        List<TradeSymbol> _tradeSymbols = new List<TradeSymbol>();
        DateTimeOffset _nextRequestTime = DateTimeOffset.MinValue;
        static HttpClient apiClient = new HttpClient();
        
        public ILogger Log { get; set; }

        public BinanceUSRestfulApi(ApiSecret secret = null) { _apiSecret = secret; }

        //IApiExchange
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
        private async Task<Tuple<BinanceExchangeInfoResponseEntity, HttpResponseMessage>> SendExchangeInfoRequest()
        {
            var endpoint = BinanceUSApiServer + ExchangeInfoEndpoint;
            HttpResponseMessage response = await apiClient.GetAsync(endpoint);
            var responseString = await CheckResponse(response, false);
            var responseAsEntity = JsonConvert.DeserializeObject<BinanceExchangeInfoResponseEntity>(responseString);
            return Tuple.Create(responseAsEntity, response);
        }
             
        //Generic Requests
        private async Task<string> SendRequestAsync(string endpoint, string requestType = Get, bool usesApiSecret = false)
        {
            var finalEndpoint = BinanceUSApiServer + (endpoint.StartsWith("/") ? endpoint : "/" + endpoint);
            HttpResponseMessage response = null;
            using (var apiMessage = new HttpRequestMessage())
            {
                apiMessage.RequestUri = new Uri(finalEndpoint);
                if (usesApiSecret)
                    apiMessage.Headers.Add("X-MBX-APIKEY", _apiSecret.ApiKey);

                switch (requestType)
                {
                    case Get:
                        apiMessage.Method = HttpMethod.Get;
                        break;
                    case Put:
                        apiMessage.Method = HttpMethod.Put;
                        break;
                    case Post:
                        apiMessage.Method = HttpMethod.Post;
                        break;
                    case Delete:
                        apiMessage.Method = HttpMethod.Delete;
                        break;
                }
                response = await apiClient.SendAsync(apiMessage);
            }
            if (response is null)
                return null;

            var responseAsString = await CheckResponse(response);
            return responseAsString;
        }
        private async Task<string> SendRequestAsync<TRequest>(TRequest requestEntity, string endpoint, string requestType = Get, bool usesApiSecret = false)
        {
            HttpResponseMessage response = null;
            var finalEndpoint = BuildQueryString(endpoint, requestEntity, usesApiSecret);
            using (var apiMessage = new HttpRequestMessage())
            {
                apiMessage.RequestUri = new Uri(finalEndpoint);
                if (usesApiSecret)
                    apiMessage.Headers.Add("X-MBX-APIKEY", _apiSecret.ApiKey);

                switch (requestType)
                {
                    case Get:
                        apiMessage.Method = HttpMethod.Get;
                        break;
                    case Put:
                        apiMessage.Method = HttpMethod.Put;
                        break;
                    case Post:
                        apiMessage.Method = HttpMethod.Post;
                        break;
                    case Delete:
                        apiMessage.Method = HttpMethod.Delete;
                        break;
                }
                response = await apiClient.SendAsync(apiMessage);
            }
            if (response is null)
                return null;

            var responseAsString = await CheckResponse(response);
            return responseAsString;
        }
        private string BuildQueryString<T>(string endpoint, T entity, bool usesApiSecret)
        {
            StringBuilder builder = new StringBuilder($"{BinanceUSApiServer}{(endpoint.StartsWith("/") ? endpoint : "/" + endpoint)}");
            StringBuilder queryString = new StringBuilder();
            int parameterCount = 0;
            try
            {
                var propertiesWithAttributes = entity.GetType().GetProperties().Where(x => Attribute.IsDefined(x, typeof(ApiParameterAttribute)));

                if (propertiesWithAttributes != null)
                {
                    for (int i = 0; i < propertiesWithAttributes.Count(); i++)
                    {
                        var property = propertiesWithAttributes.ElementAt(i);
                        parameterCount = AppendQueryParameter(entity, builder, queryString, property, parameterCount);
                    }
                    
                    var query = queryString.ToString();
                    if (usesApiSecret)
                        queryString.Append("&signature=" + _apiSecret.GetHmacSha256(query));
                    builder.Append(queryString.ToString());
                }
            }
            catch { Log?.Add("Unable to get properties on entity " + entity?.GetType().ToString() ?? "(null object)"); }

            return builder.ToString();
        }
        private static int AppendQueryParameter<T>(T entity, StringBuilder builder, StringBuilder queryString, PropertyInfo property, int parameterCount)
        {            
            var propertyName = (ApiParameterAttribute)property.GetCustomAttributes(typeof(ApiParameterAttribute), true)[0];
            var propertyValue = property.GetValue(entity);

            if (!string.IsNullOrWhiteSpace(propertyValue.ToString()))
            {
                parameterCount += 1;
                if (parameterCount > 1)
                    queryString.Append("&");
                else if (parameterCount == 1)
                    builder.Append("?");

                queryString.Append(propertyName.ParameterName + "=" + propertyValue.ToString());
            }

            return parameterCount;
        }

        //Generic Respones
        private async Task<string> CheckResponse(HttpResponseMessage response, bool checkApiLimits = true)
        {
            if (checkApiLimits)
                UpdateApiLimits(response.Headers);
            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests || (int)response.StatusCode == 418) //Server will return "I'm a teapot" if banned.
                HandleFourTwentyNineError(response.Headers);

           return await response.Content.ReadAsStringAsync();
        }

        //API Limits
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
        private bool CanMakeAnyApiCall()
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
        private bool CanMakeOrderApiCall(int orderCount = 1, int weight = 1)
        {
            bool canPlaceOrder = true;
            var orderLimits = _apiLimits.Where(x => x.LimitType == Orders);

            if ((orderLimits?.Count() ?? 0) > 0)
            {
                var reachedMax = orderLimits.Where(x => x.CurrentCount + orderCount >= x.Limit);
                if ((reachedMax?.Count() ?? 0) > 0)
                    canPlaceOrder = false;
            }

            return canPlaceOrder && CanMakeWeightedApiCall(weight);
        }
        private bool CanMakeWeightedApiCall(int weight)
        {
            bool canPlaceApiCall = true;
            var apiRequestLimits = _apiLimits.Where(x => x.LimitType == RequestWeight);

            if ((apiRequestLimits?.Count() ?? 0) > 0)
            {
                var reachedMax = apiRequestLimits.Where(x => x.CurrentCount + weight >= x.Limit);
                if ((reachedMax?.Count() ?? 0) > 0)
                    canPlaceApiCall = false;
            }

            return canPlaceApiCall && CanMakeAnyApiCall();
        }        
    }
    
}
