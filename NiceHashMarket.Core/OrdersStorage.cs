using System.ComponentModel;
using NiceHashMarket.Model;
using NiceHashMarket.Model.Interfaces;

namespace NiceHashMarket.Core
{
    public class OrdersStorage : DataStorage<Order>
    {
        public OrdersStorage(ApiClient apiClient, IAlgo algo, int frequencyQueryMilliseconds, PropertyChangedEventHandler propertyChangedHandler) 
            : base(apiClient, algo, frequencyQueryMilliseconds, propertyChangedHandler)
        {

        }

        public override void ApiQueryExecute(object state)
        {
            var orders = ApiClient.GetOrders(Algo);

            UpdateBindingList(orders);

            base.ApiQueryExecute(state);
        }
    }
}
