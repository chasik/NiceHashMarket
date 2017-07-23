using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using DevExpress.Mvvm.DataAnnotations;
using NiceHashMarket.Core;
using NiceHashMarket.Model;

namespace NiceHashMarket.WpfClient.ViewModels
{
    [POCOViewModel]
    public class AlgoMarketViewModel
    {
        private readonly OrdersStorage _ordersStorage;

        public virtual BindingList<Order> OrdersEurope { get; set; } = new BindingList<Order>();
        public virtual BindingList<Order> OrdersUsa { get; set; } = new BindingList<Order>();

        public AlgoMarketViewModel()
        {
            var client = new ApiClient();
            var algoList = new Algorithms();

            _ordersStorage = new OrdersStorage(client, algoList.First(a => a.Id == 23), 1000, Application.Current.Dispatcher);

            _ordersStorage.Entities.ListChanged += Entities_ListChanged;
        }

        private void Entities_ListChanged(object sender, ListChangedEventArgs e)
        {
            var orders = sender as BindingList<Order>;

            if (orders == null)
                return;

            switch (e.ListChangedType)
            {
                case ListChangedType.Reset:
                    break;
                case ListChangedType.ItemAdded:
                    var order = orders[e.NewIndex];

                    if (order.Server == ServerEnum.Europe)
                        OrdersEurope.Add(order);
                    else if (order.Server == ServerEnum.Usa)
                        OrdersUsa.Add(order);

                    break;
                case ListChangedType.ItemDeleted:
                    break;
                case ListChangedType.ItemMoved:
                    break;
                case ListChangedType.ItemChanged:
                    break;
                case ListChangedType.PropertyDescriptorAdded:
                    break;
                case ListChangedType.PropertyDescriptorDeleted:
                    break;
                case ListChangedType.PropertyDescriptorChanged:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

    }
}