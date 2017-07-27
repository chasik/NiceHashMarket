using System;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Windows;
using DevExpress.Mvvm.DataAnnotations;
using NiceHashBotLib;
using NiceHashMarket.Core;
using NiceHashMarket.Core.Helpers;
using NiceHashMarket.Model.Enums;
using NiceHashMarket.Model.Interfaces;
using NiceHashMarket.Services;
using Order = NiceHashMarket.Model.Order;

namespace NiceHashMarket.WpfClient.ViewModels
{
    [POCOViewModel]
    public class AlgoMarketViewModel : IDataCallBacks
    {
        #region | Fields |

        private readonly Timer _timer;
        private readonly OrdersStorage _ordersStorage;
        private Algorithms _algoList;
        private AlgoNiceHashEnum _currentAlgo;
        private CoinsWhatToMineEnum _currentCoin;

        #endregion

        #region | Properties |

        public virtual double MaxPermittedPrice { get; set; }

        public virtual AlgoNiceHashEnum CurrentAlgo
        {
            get => _currentAlgo;
            set
            {
                if (_ordersStorage != null)
                {
                    _ordersStorage.Entities.ListChanged -= Entities_ListChanged;
                    _ordersStorage.Entities.BeforeRemove -= Entities_BeforeRemove;

                    _ordersStorage.SelectAnotherAlgo(_algoList.First(a => a.Id == (byte)value));
                }

                _currentAlgo = value;
            }
        }

        public virtual CoinsWhatToMineEnum CurrentCoin
        {
            get => _currentCoin;
            set
            {
                _currentCoin = value;
                WhatToTimeTimerHandler(null);
            }
        }

        public virtual NiceBindingList<Order> MyOrders { get; set; } = new NiceBindingList<Order>();

        public virtual NiceBindingList<Order> OrdersEurope { get; set; } = new NiceBindingList<Order>();
        public virtual NiceBindingList<Order> OrdersUsa { get; set; } = new NiceBindingList<Order>();

        #endregion

        public AlgoMarketViewModel()
        {
            _algoList = new Algorithms();
            CurrentCoin = CoinsWhatToMineEnum.Lbc;
            CurrentAlgo = AlgoNiceHashEnum.Lbry;

            #region | local DataSource |

            _ordersStorage = new OrdersStorage(_algoList.First(a => a.Id == (byte)CurrentAlgo), 1000, Application.Current.Dispatcher);

            OrdersStorageOnAlgoChanged(_ordersStorage, null, null);
            _ordersStorage.AlgoChanged += OrdersStorageOnAlgoChanged;

            #endregion

            #region | Wcf service DataSource |

            //var factory = new DuplexChannelFactory<IDataService>(this, "netTcpBinding_dataService");

            //var client = factory.CreateChannel();

            //client.ListenAlgo(_algoList.First(a => a.Id == (byte)CurrentAlgo));            

            #endregion


            _timer = new Timer(WhatToTimeTimerHandler, null, 0, 3000);
        }

        #region | Private methods |

        private void OrdersStorageOnAlgoChanged(DataStorage<Order> sender, IAlgo oldalgo, IAlgo newalgo)
        {
            sender.Entities.ListChanged += Entities_ListChanged;
            sender.Entities.BeforeRemove += Entities_BeforeRemove;

            OrdersEurope.Clear();
            OrdersUsa.Clear();
        }

        private void WhatToTimeTimerHandler(object state)
        {
            var maxPermittedPrice = HandlerClass.HandleOrder(CurrentCoin);

            Application.Current.Dispatcher.Invoke(() =>
            {
                MaxPermittedPrice = maxPermittedPrice;
            });
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

        #endregion

        void IDataCallBacks.OrderAdded(Order order)
        {
            //var orderClone = (Order)order?.Clone();

            if (order.Server == ServerEnum.Europe)
                OrdersEurope.Add(order);
            else if (order.Server == ServerEnum.Usa)
                OrdersUsa.Add(order);
        }

        void IDataCallBacks.OrderChanged(Order order)
        {
            var orderChanged = OrdersEurope.FirstOrDefault(o => o.Id == order?.Id)
                               ?? OrdersUsa.FirstOrDefault(o => o.Id == order?.Id);

            if (orderChanged == null)
                return;

            order.CopyProperties(orderChanged);
        }
    }
}