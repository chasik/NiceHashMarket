using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using NiceHashBotLib;
using NiceHashMarket.Core;
using NiceHashMarket.Model;
using NiceHashMarket.Model.Enums;
using NiceHashMarket.Services;
using NiceHashMarket.WpfClient.Interfaces;
using Order = NiceHashMarket.Model.Order;

namespace NiceHashMarket.WpfClient.ViewModels
{
    [POCOViewModel]
    public class AlgoMarketViewModel : IHaveOrdersStorage, IDataCallBacks
    {
        #region | Fields |

        private readonly Timer _timer;
        private int _timerCounter;
        private OrdersStorage _ordersStorage;
        private Algorithms _algoList;
        private AlgoNiceHashEnum _currentAlgo;
        private CoinsWhatToMineEnum _currentCoin;
        private BindingList<BlockInfo> _lastBlocksSuprNova;
        private BindingList<BlockInfo> _lastBlocksCoinMinePl;

        #endregion

        #region | Properties |

        public virtual WhattomineResult WhattomineResult { get; set; }

        public virtual OrdersStorage OrdersStorage
        {
            get => _ordersStorage;
            set => _ordersStorage = value;
        }

        public virtual AlgoNiceHashEnum CurrentAlgo
        {
            get => _currentAlgo;
            set
            {
                _ordersStorage?.SelectAnotherAlgo(_algoList.First(a => a.Id == (byte)value));

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

        public virtual BindingList<BlockInfo> LastBlocksSuprNova
        {
            get => _lastBlocksSuprNova ?? (_lastBlocksSuprNova = new BindingList<BlockInfo>());
            set => _lastBlocksSuprNova = value;
        }

        public virtual BindingList<BlockInfo> LastBlocksCoinMinePl
        {
            get => _lastBlocksCoinMinePl ?? (_lastBlocksCoinMinePl = new BindingList<BlockInfo>());
            set => _lastBlocksCoinMinePl = value;
        }

        public virtual DateTime MinDateTimeOfBlock { get; set; } = DateTime.MaxValue;
        public virtual DateTime MaxDateTimeOfBlock { get; set; } = DateTime.MinValue;

        #endregion

        public AlgoMarketViewModel()
        {
            _algoList = new Algorithms();
            CurrentCoin = CoinsWhatToMineEnum.Lbc;
            CurrentAlgo = AlgoNiceHashEnum.Lbry;

            #region | local DataSource |

            _ordersStorage = new OrdersStorage(_algoList.First(a => a.Id == (byte)CurrentAlgo), 1000, Application.Current.Dispatcher);

            #endregion

            #region | Wcf service DataSource |

            //var factory = new DuplexChannelFactory<IDataService>(this, "netTcpBinding_dataService");

            //var client = factory.CreateChannel();

            //client.ListenAlgo(_algoList.First(a => a.Id == (byte)CurrentAlgo));            

            #endregion

            #region | SuprNova api |

            GetLastBlocksFromPool(new SuprNovaApi("https://lbry.suprnova.cc/", 5000, +3), LastBlocksSuprNova);
            GetLastBlocksFromPool(new SuprNovaApi("https://www2.coinmine.pl/lbc", 5000, +1), LastBlocksCoinMinePl);

            #endregion

            #region | NiceHash api |

            //APIWrapper.Initialize(Properties.Settings.Default.NiceApiId, Properties.Settings.Default.NiceApiKey);
            //APIWrapper.GetMyOrders(0, (byte)CurrentAlgo).ForEach(o =>
            //{
            //    MyOrders.Add(new Order(o.ID, (decimal)o.Price, (decimal)o.SpeedLimit, (decimal)o.Speed, o.Workers, o.OrderType, o.Alive ? 0 : 1, ServerEnum.Europe));
            //});

            #endregion

            _timerCounter = 0;
            _timer = new Timer(WhatToTimeTimerHandler, null, 0, 3000);

        }

        private void GetLastBlocksFromPool(SuprNovaApi poolApiInstance, IList<BlockInfo> blocks)
        {
            poolApiInstance.RowOfBlockParsed += (sender, block) => { };
            poolApiInstance.NewBlockFounded += (sender, block) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    blocks.Add(block.Clone() as BlockInfo);
                    MinDateTimeOfBlock = new DateTime(Math.Min(MinDateTimeOfBlock.Ticks, block.Created.Ticks));
                    MaxDateTimeOfBlock = new DateTime(Math.Max(MaxDateTimeOfBlock.Ticks, block.Created.Ticks));
                });
            };
            poolApiInstance.BlockUpdated += (sender, block) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var updatedBlock = blocks.First(b => b.Id == block.Id);
                    var index = blocks.IndexOf(updatedBlock);
                    blocks[index].Percent = block.Percent;
                });
            };
        }

        #region | Private methods |

        private void WhatToTimeTimerHandler(object state)
        {
            _timerCounter++;

            var result = HandlerClass.HandleOrder(CurrentCoin);

            Application.Current.Dispatcher.Invoke(() =>
            {
                WhattomineResult = result;
                MaxDateTimeOfBlock = DateTime.Now;
                //MinDateTimeOfBlock = MaxDateTimeOfBlock.AddMinutes(-60);

                this.RaisePropertyChanged(vm => vm.LastBlocksSuprNova);
                this.RaisePropertyChanged(vm => vm.LastBlocksCoinMinePl);
            });
        }


        #endregion

        void IDataCallBacks.OrderAdded(Order order)
        {
            //if (order.Server == ServerEnum.Europe)
            //    OrdersEurope.Add(order);
            //else if (order.Server == ServerEnum.Usa)
            //    OrdersUsa.Add(order);
        }

        void IDataCallBacks.OrderChanged(Order order)
        {
            //var orderChanged = OrdersEurope.FirstOrDefault(o => o.Id == order?.Id)
            //                   ?? OrdersUsa.FirstOrDefault(o => o.Id == order?.Id);

            //if (orderChanged == null)
            //    return;

            //order.CopyProperties(orderChanged);
        }
    }
}