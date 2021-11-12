using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Concrete
{
    public class ApiLimit
    {
        public TimeSpan Interval { get; set; }
        public string LimitHeader { get; set; }
        public int CurrentCount { get; set; }
        public int Limit { get; set; }
        public DateTimeOffset NextReset { get; set; }
        public string LimitType { get; set; }
    }
}
