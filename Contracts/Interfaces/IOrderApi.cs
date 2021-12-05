using Contracts.Concrete;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Contracts.Interfaces
{
    public interface IOrderApi
    {
        long ProcessingMaxMilliseconds { get; set; }

        bool CanPlaceOcoOrder();
        bool CanPlaceOrder();
        Task<ExchangeOrderResult> DeleteOrder(ExchangeOrder order);
        Task<ExchangeOrderResult> GetOrder(ExchangeOrder order);
        Task<List<ExchangeOrderResult>> PutOcoOrder(ExchangeOrder limitOrder, ExchangeOrder stopLossOrder, decimal? stopLossLimit = null);
        Task<ExchangeOrderResult> PutOrder(ExchangeOrder order);
    }
}