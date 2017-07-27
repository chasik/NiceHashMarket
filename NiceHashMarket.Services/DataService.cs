using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel;
using NiceHashMarket.Core;
using NiceHashMarket.Core.Helpers;
using NiceHashMarket.Model;
using NiceHashMarket.Model.Interfaces;

namespace NiceHashMarket.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class DataService : IDataService
    {
        private static Dictionary<IDataCallBacks, byte> _subscribers;
        private static Dictionary<byte, OrdersStorage> _ordersStorages;

        static DataService()
        {
            _subscribers = new Dictionary<IDataCallBacks, byte>();
            _ordersStorages = new Dictionary<byte, OrdersStorage>();
        }

        public void ListenAlgo(IAlgo algo)
        {
            var callbackChannel = OperationContext.Current.GetCallbackChannel<IDataCallBacks>();

            if (!_subscribers.ContainsKey(callbackChannel))
                _subscribers.Add(callbackChannel, algo.Id);

            if (!_ordersStorages.ContainsKey(algo.Id))
            {
                var orderStorageForAlgo = new OrdersStorage(algo, 1000);
                
                orderStorageForAlgo.Entities.ListChanged += Entities_ListChanged;
                //orderStorageForAlgo.Entities.BeforeRemove += Entities_BeforeRemove;

                _ordersStorages.Add(algo.Id, orderStorageForAlgo);
            }

        }

        private void Entities_ListChanged(object sender, ListChangedEventArgs e)
        {
            foreach (var subscriber in _subscribers)
            {
                var storage = _ordersStorages.FirstOrDefault(x => x.Key == subscriber.Value).Value;

                if (storage == null) continue;

                if (((ICommunicationObject) subscriber.Key).State == CommunicationState.Opened)
                {
                    var orders = sender as BindingList<Order>;
                    var orderNewIndex = e.NewIndex > -1 && e.NewIndex < orders?.Count ? orders[e.NewIndex] : null;

                    switch (e.ListChangedType)
                    {
                        case ListChangedType.Reset:
                            break;
                        case ListChangedType.ItemAdded:
                            subscriber.Key.OrderAdded((Order)orderNewIndex?.Clone());
                            break;
                        case ListChangedType.ItemDeleted:
                            break;
                        case ListChangedType.ItemMoved:
                            break;
                        case ListChangedType.ItemChanged:
                            subscriber.Key.OrderChanged((Order)orderNewIndex?.Clone());
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
                else
                {
                    _subscribers.Remove(subscriber.Key);
                }

            }
        }
    }
}
