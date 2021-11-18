using Contracts.Concrete;
using Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Extensions
{
    public static class ExchangeOrderExtensionMethods
    {
        public static string GetTimeInForceAsBinanceString(this ExchangeOrder order)
        {
            switch (order?.TimeInForce ?? Enums.TimeInForce.None)
            {
                case Enums.TimeInForce.GoodTillCanceled:
                    return "GTC";
                case Enums.TimeInForce.FillOrKill:
                    return "FOK";
                case Enums.TimeInForce.ImmediateOrCancel:
                    return "IOC";
                default:
                    return null;
            }
        }
        public static void SetBinanceOrderStatus(this ExchangeOrderResult result, string status)
        {
            switch (status)
            {
                case "NEW":
                    result.OrderStatus = OrderStatus.New;
                    break;
                case "PARTIALLY_FILLED":
                    result.OrderStatus = OrderStatus.PartiallyFilled;
                    break;
                case "FILLED":
                    result.OrderStatus = OrderStatus.Filled;
                    break;
                case "CANCELED":
                    result.OrderStatus = OrderStatus.Canceled;
                    break;
                case "PENDING_CANCEL":
                    result.OrderStatus = OrderStatus.PendingCancel;
                    break;
                case "REJECTED":
                    result.OrderStatus = OrderStatus.Rejected;
                    break;
                case "EXPIRED":
                    result.OrderStatus = OrderStatus.Expired;
                    break;
                case "RESPONSE":
                    result.OrderStatus = OrderStatus.Response;
                    break;
                case "EXEC_STARTED":
                    result.OrderStatus = OrderStatus.ExecutingStarted;
                    break;
                case "ALL_DONE":
                    result.OrderStatus = OrderStatus.AllDone;
                    break;
                case "EXECUTING":
                    result.OrderStatus = OrderStatus.Executing;
                    break;
                case "REJECT":
                    result.OrderStatus = OrderStatus.Reject;
                    break;
                default:
                    result.OrderStatus = OrderStatus.NotAvailable;
                    break;
            }
        }
    }
}
