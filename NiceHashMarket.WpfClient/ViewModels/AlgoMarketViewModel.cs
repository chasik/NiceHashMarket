using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using DevExpress.Mvvm.DataAnnotations;
using NiceHashBotLib;
using NiceHashMarket.Core;
using NiceHashMarket.Core.Helpers;
using NiceHashMarket.Model.Enums;
using Order = NiceHashMarket.Model.Order;

namespace NiceHashMarket.WpfClient.ViewModels
{
    [POCOViewModel]
    public class AlgoMarketViewModel
    {
        private readonly OrdersStorage _ordersStorage;

        public virtual double MaxPermittedPrice { get; set; }

        public virtual NiceBindingList<Order> OrdersEurope { get; set; } = new NiceBindingList<Order>();
        public virtual NiceBindingList<Order> OrdersUsa { get; set; } = new NiceBindingList<Order>();

        public AlgoMarketViewModel()
        {
            var client = new ApiClient();
            var algoList = new Algorithms();

            _ordersStorage = new OrdersStorage(client, algoList.First(a => a.Id == (byte)AlgoNiceHashEnum.Equihash), 1000, Application.Current.Dispatcher);

            _ordersStorage.Entities.ListChanged += Entities_ListChanged;
            _ordersStorage.Entities.BeforeRemove += Entities_BeforeRemove;

            var timer = new Timer(t =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MaxPermittedPrice = HandlerClass.HandleOrder(CoinsWhatToMineEnum.Zec);
                });
            }, null, 0, 1000);
        }

        private void Entities_BeforeRemove(Order deletedItem)
        {
            OrdersEurope.RemoveIfExistById(deletedItem.Id);
            OrdersUsa.RemoveIfExistById(deletedItem.Id);
        }

        private void Entities_ListChanged(object sender, ListChangedEventArgs e)
        {
            var orders = sender as BindingList<Order>;
            var orderNewIndex = e.NewIndex > -1 && e.NewIndex < orders?.Count ? orders[e.NewIndex] : null;

            switch (e.ListChangedType)
            {
                case ListChangedType.Reset:
                    break;
                case ListChangedType.ItemAdded:
                    var orderClone = (Order)orderNewIndex?.Clone();

                    if (orderNewIndex.Server == ServerEnum.Europe)
                        OrdersEurope.Add(orderClone);
                    else if (orderNewIndex.Server == ServerEnum.Usa)
                        OrdersUsa.Add(orderClone);
                    break;
                case ListChangedType.ItemDeleted:
                    break;
                case ListChangedType.ItemMoved:
                    break;
                case ListChangedType.ItemChanged:
                    var orderChanged = OrdersEurope.FirstOrDefault(o => o.Id == orderNewIndex?.Id) 
                          ?? OrdersUsa.FirstOrDefault(o => o.Id == orderNewIndex?.Id);

                    if (orderChanged == null)
                        return;

                    orderNewIndex.CopyProperties(orderChanged);
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