using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Contracts.Concrete;


namespace Contracts.Extensions
{
    public static class ApiLimitExtensionMethods
    {
        public static void UpdateLimit(this ApiLimit limit, HttpResponseHeaders headers)
        {
            var limitHeader = headers.SingleOrDefault(x => x.Key.ToLower() == (limit.LimitHeader.ToLower()));
            if (!limitHeader.Equals(default(KeyValuePair<string, IEnumerable<string>>)))
                limit.CurrentCount = Convert.ToInt32(limitHeader.Value.FirstOrDefault() ?? $"{limit.CurrentCount}");

            if (DateTimeOffset.Now > limit.NextReset)
                while (limit.NextReset < DateTimeOffset.Now)
                    limit.NextReset += limit.Interval;
        }
        public static void IncrementLimit(this ApiLimit limit, int count = 1)
        {
            limit.CurrentCount += count;
            if (DateTimeOffset.Now > limit.NextReset)
            {
                while (limit.NextReset < DateTimeOffset.Now)
                    limit.NextReset += limit.Interval;
                limit.CurrentCount = count;
            }
        }
    }
}
