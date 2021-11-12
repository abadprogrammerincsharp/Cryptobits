using Contracts.Enums;
using Contracts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logging.ConsoleLogging
{
    public class ConsoleLogger : ILogger
    {
        public event EventHandler<string> OnMessageReceived;

        public void Add(string message, LoggingLevel loggingLevel = LoggingLevel.Debugging)
        {
            Console.WriteLine(message);
        }
    }
}
