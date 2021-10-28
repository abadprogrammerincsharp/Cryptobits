using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Concrete
{
    public class IndicatorResult
    {        
        public Dictionary<string, decimal> ResultSet { get; set; }
        public DateTimeOffset LastUpdated { get; set; }

        public IndicatorResult()
        {
            ResultSet = new Dictionary<string, decimal>();
        }

        public List<string> GetCsvHeaders()
        {
            return ResultSet.Keys.ToList();
        }

        public string GetCsvData(string header)
        {
            return $"{ResultSet[header]}";
        }

    }
}
