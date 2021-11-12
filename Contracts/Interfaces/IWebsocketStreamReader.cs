using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Interfaces
{
    public interface IWebsocketStreamReader
    {
        public ILogger Log { get; set; }
        public Task<bool> TryStartStream();
        public void StopStream();
    }
}
