using Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Interfaces
{
    public interface ILogger
    {
        public void Add(string message, LoggingLevel loggingLevel = LoggingLevel.Debugging);
        public event EventHandler<string> OnMessageReceived;
    }
}
