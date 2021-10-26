using Contracts.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Interfaces
{
    public interface ICandleFeed
    {
        TimeSpan Interval { get; set; }
        public event EventHandler<bool> FeedAvailabilityChanged;
        public event EventHandler<Candlestick> ReceivedData;
    }
}
