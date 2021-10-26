using Contracts.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Interfaces
{
    public interface ICandleLoad
    {
        public Task<IEnumerable<Candlestick>> GetLatestCandlesAsync(int quantity, TimeSpan interval);
    }
}
