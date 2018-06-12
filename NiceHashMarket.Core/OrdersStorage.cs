using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using NiceHashMarket.Model;
using NiceHashMarket.Model.Enums;
using NiceHashMarket.Model.Interfaces;

namespace NiceHashMarket.Core
{
    public delegate void TargetLevelChanged(short targetPercent, decimal price);

    public class OrdersStorage : DataStorage<Order>
    {
        public event TargetLevelChanged TargetLevelChanged;

        private IDictionary<short, decimal> _targetLevels;

        public OrdersStorage(IAlgo algo, int frequencyQueryMilliseconds, IEnumerable<short> targetLevels) 
            : base(algo, frequencyQueryMilliseconds)
        {
            if (targetLevels == null) return;
            _targetLevels = new Dictionary<short, decimal>();

            foreach (var targetLevel in targetLevels.OrderBy(tl => tl))
            {
                _targetLevels.Add(targetLevel, decimal.MinusOne);
            }
        }

        public override async void JsonQueryExecute()
        {
            var algo = Algo;
            var orders = await Task.Run(() => ApiClient.GetOrders(algo));

            var ordersArray = orders as Order[] ?? orders.ToArray();

            if (!ordersArray.Any() || algo != Algo)
                return;

            UpdateBindingList(ordersArray);

            CalcTargetLevels();
        }

        private void CalcTargetLevels()
        {
            if (_targetLevels == null || !_targetLevels.Any()) return;

            var ordersForCalc = Entities
                .Where(o =>
                    o.Active
                    && o.Type == OrderTypeEnum.Standart
                    && o.Workers > 0
                    && (o.Amount > 0.1m || o.Amount < 0.01m /* if less than 0.01 volume - it's unlimited order */)
                    //&& (myOrders == null || !myOrders.Any() || myOrders.All(myOrder => myOrder.Id != o.Id))
                    )
                .OrderBy(o => o.Price)
                .ToList();

            var workersAll = ordersForCalc.Sum(o => o.Workers);
            if (workersAll == 0) return;

            foreach (var targetLevel in _targetLevels.Keys.ToArray())
            {
                var workersSumm = 0;
                foreach (var o in ordersForCalc)
                {
                    if (workersSumm == int.MinValue) break;

                    workersSumm += o.Workers;

                    if (workersSumm * 100 / workersAll >= targetLevel)
                    {
                        if (_targetLevels[targetLevel] != o.Price)
                        {
                            _targetLevels[targetLevel] = o.Price;
                            OnTargetLevelChanged(targetLevel, _targetLevels[targetLevel]);

                            workersSumm = int.MinValue;
                        }
                    }
                }
            }
        }

        public Order GetOrderById(int orderId)
        {
            var order = Entities.FirstOrDefault(o => o.Id == orderId);

            return order;
        }

        protected virtual void OnTargetLevelChanged(short targetpercent, decimal price)
        {
            TargetLevelChanged?.Invoke(targetpercent, price);
        }
    }
}
