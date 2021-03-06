using Contracts.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataInteraction.RestfulEntities.Binance
{
    public class BinanceKlineRequestEntity
    {
        [ApiParameter("symbol")]
        public string Symbol { get; set; }
        
        [ApiParameter("interval")]
        public string Interval { get; set; }
        
        [ApiParameter("limit")]
        public int Limit { get; set; }
    }
}
