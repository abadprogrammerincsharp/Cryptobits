using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Concrete
{
    public class FeedAvailibilityEvent : EventArgs
    {
        public TradingPair TradingPair { get; set; }
        public bool IsAvailable { get; set; }
    }
}
