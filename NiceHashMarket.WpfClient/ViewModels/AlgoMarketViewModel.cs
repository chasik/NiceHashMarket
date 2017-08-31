using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using NiceHashBotLib;
using NiceHashMarket.Core;
using NiceHashMarket.Logger;
using NiceHashMarket.Model;
using NiceHashMarket.Model.Enums;
using NiceHashMarket.Services;
using NiceHashMarket.WpfClient.Interfaces;
using NiceHashMarket.WpfClient.Properties;
using Order = NiceHashMarket.Model.Order;

namespace NiceHashMarket.WpfClient.ViewModels
{
    [POCOViewModel]
    public class AlgoMarketViewModel : IHaveMyOrders, IHaveOrdersStorage, IDataCallBacks, ICanJump
    {
        private const string LbrySuprnovaUrl = "https://lbry-api.suprnova.cc";
        private const string LbryCoinmineUrl = "https://www2.coinmine.pl/lbc";

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

        public virtual int CurrentDifficulty { get; set; }
        public virtual int MinDifficulity { get; set; }
        public virtual int MaxDifficulity { get; set; }
        public virtual BindingList<DashboardPoolResult> DashboardResults { get; set; } = new BindingList<DashboardPoolResult>();


        public virtual BindingList<BindingList<DashboardPoolResult>> HashRates { get; set; } =
            new BindingList<BindingList<DashboardPoolResult>>
            {
                new BindingList<DashboardPoolResult>(),
                new BindingList<DashboardPoolResult>(),
                new BindingList<DashboardPoolResult>()
            };

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

            GetLastBlocksFromPool(
                new MiningPortalApi(LbrySuprnovaUrl, 5000, +3, MetricPrefixEnum.Mega, Settings.Default.LbrySuprnovaApiKey, Settings.Default.LbrySuprnovaUserId)
                    , LastBlocksSuprNova);

            GetLastBlocksFromPool(
                new MiningPortalApi(LbryCoinmineUrl, 5000, +1, MetricPrefixEnum.Tera, Settings.Default.LbryCoinMineApiKey, Settings.Default.LbryCoinMineUserId)
                    , LastBlocksCoinMinePl);

            #endregion

            #region | NiceHash api |

            APIWrapper.Initialize(Properties.Settings.Default.NiceApiId, Properties.Settings.Default.NiceApiKey);
            APIWrapper.GetMyOrders(0, (byte)CurrentAlgo).ForEach(o =>
            {
                MyOrders.Add(new Order(o.ID, (decimal)o.Price, (decimal)o.SpeedLimit, (decimal)o.Speed, o.Workers, o.OrderType, o.Alive ? 0 : 1, ServerEnum.Europe));
            });
            APIWrapper.GetMyOrders(1, (byte)CurrentAlgo).ForEach(o =>
            {
                MyOrders.Add(new Order(o.ID, (decimal)o.Price, (decimal)o.SpeedLimit, (decimal)o.Speed, o.Workers, o.OrderType, o.Alive ? 0 : 1, ServerEnum.Usa));
            });

            #endregion

            _timerCounter = 0;
            _timer = new Timer(WhatToTimeTimerHandler, null, 0, 3000);

        }

        private void GetLastBlocksFromPool(MiningPortalApi miningPortalApiInstance, IList<BlockInfo> blocks)
        {
            miningPortalApiInstance.RowOfBlockParsed += (sender, block) => { };
            miningPortalApiInstance.NewBlockFounded += (sender, block) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    blocks.Add(block.Clone() as BlockInfo);
                    MinDateTimeOfBlock = new DateTime(Math.Min(MinDateTimeOfBlock.Ticks, block.Created.Ticks));
                    MaxDateTimeOfBlock = new DateTime(Math.Max(MaxDateTimeOfBlock.Ticks, block.Created.Ticks));
                });
            };
            miningPortalApiInstance.BlockUpdated += (sender, block) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var updatedBlock = blocks.First(b => b.Id == block.Id);
                    var index = blocks.IndexOf(updatedBlock);
                    blocks[index].Percent = block.Percent;
                });
            };

            miningPortalApiInstance.DifficultyChanged += (sender, dashboardResult) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DashboardResults.Add(dashboardResult);

                    CurrentDifficulty = dashboardResult.Difficulty;
                    MinDifficulity = DashboardResults.OrderBy(x => x.Difficulty).FirstOrDefault()?.Difficulty ?? 0;
                    MaxDifficulity =
                        DashboardResults.OrderByDescending(x => x.Difficulty).FirstOrDefault()?.Difficulty ?? 0;

                    //MarketLogger.Information($"{DateTime.Now.TimeOfDay:g} DiffChanged {dashboardResult.Difficulty}");
                });
            };

            miningPortalApiInstance.PoolHashRateChanged += (sender, dashboardResult) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (dashboardResult.Host.Equals(LbrySuprnovaUrl))
                        HashRates[1].Add(dashboardResult);
                    else if (dashboardResult.Host.Equals(LbryCoinmineUrl))
                        HashRates[2].Add(dashboardResult);

                    this.RaisePropertyChanged(vm => vm.HashRates);
                });
            };

            miningPortalApiInstance.GlobalHashRateChanged += (sender, dashboardResult) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    HashRates[0].Add(dashboardResult);
                    this.RaisePropertyChanged(vm => vm.HashRates);
                });
            };

            miningPortalApiInstance.RoundProgressChanged += (sender, dashBoardResult) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    //MarketLogger.Information($"{DateTime.Now.TimeOfDay:g} {dashBoardResult.Host} {dashBoardResult.RoundProgress}");
                });
            };
        }

        #region | Private methods |

        private void WhatToTimeTimerHandler(object state)
        {
            _timerCounter++;

            var result = HandlerClass.HandleOrder(CurrentCoin, CurrentDifficulty);

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

        public DateTime DoJump(Order order)
        {
            var myOrderForJump = MyOrders.OrderByDescending(o => o.Price).FirstOrDefault(o => o.Server == order.Server);
            var newPrice = order.Price + 0.0001m;

            if (myOrderForJump == null 
                || myOrderForJump.Price >= newPrice 
                || (decimal)WhattomineResult.MaxPrice24 < newPrice
                || Math.Abs((LastOrderJumpDateTime - DateTime.Now).TotalMilliseconds) < 1000)
                return DateTime.Now;

            MarketLogger.Information($"{order.Server} Jump my order {myOrderForJump.Id} {myOrderForJump.Price} jump to:{newPrice}");

            LastOrderJumpDateTime = DateTime.Now;

            //Task.Factory.StartNew(() => -1/*APIWrapper.OrderSetPrice(myOrderForJump.Server == ServerEnum.Europe ? 0 : 1, (int)CurrentAlgo, myOrderForJump.Id, newPrice)*/ )
            Task.Factory.StartNew(() => APIWrapper.OrderSetPrice(myOrderForJump.Server == ServerEnum.Europe ? 0 : 1, (int)CurrentAlgo, myOrderForJump.Id, (double)newPrice))
                .ContinueWith(t =>
                {
                    var priceResult = t.Result;

                    if (priceResult == -1)
                        MarketLogger.Error($"{DateTime.Now} server:{order.Server} id:{myOrderForJump.Id} price NOT changed!");
                    else
                        MarketLogger.Information($"{DateTime.Now} server:{order.Server} id:{myOrderForJump.Id} price changed, new price:{priceResult}");

                });

            return LastOrderJumpDateTime;
        }

        public DateTime LastOrderJumpDateTime { get; set; }
    }
}