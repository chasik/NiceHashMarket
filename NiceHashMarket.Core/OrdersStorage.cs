using System.Windows.Threading;
using NiceHashMarket.Model;
using NiceHashMarket.Model.Interfaces;

namespace NiceHashMarket.Core
{
    public class OrdersStorage : DataStorage<Order>
    {
        public OrdersStorage(ApiClient apiClient, IAlgo algo, int frequencyQueryMilliseconds, Dispatcher currentDispatcher = null) 
            : base(apiClient, algo, frequencyQueryMilliseconds, currentDispatcher)
        {

        }

        public override void ApiQueryExecute()
        {
            var orders = ApiClient.GetOrders(Algo);

            if (orders == null)
                return;

            UpdateBindingList(orders);

            base.ApiQueryExecute();
        }
    }
}
