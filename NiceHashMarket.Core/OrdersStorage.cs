using System.Linq;
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
            var algo = Algo;
            var orders = ApiClient.GetOrders(algo);
        
            if (orders == null || !orders.Any() || algo != Algo)
                return;

            UpdateBindingList(orders);

            base.ApiQueryExecute();
        }
    }
}
