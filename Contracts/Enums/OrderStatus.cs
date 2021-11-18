using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Enums
{
    public enum OrderStatus
    {
        NotAvailable = -1,
        New = 0,
        ExecutingStarted = 0,
        Executing = 1,
        PartiallyFilled = 1,
        Filled = 2,
        AllDone = 2,
        PendingCancel = 3,
        Response = 3,
        Canceled = 4,
        Reject = 4,
        Rejected = 4,
        Expired = 4,
    }
}
