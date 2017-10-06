﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.POCO;
using NiceHashBotLib;
using NiceHashMarket.Core;
using NiceHashMarket.Core.Helpers;
using NiceHashMarket.Logger;
using NiceHashMarket.Model;
using NiceHashMarket.Model.Enums;
using NiceHashMarket.Services;
using NiceHashMarket.WpfClient.Interfaces;
using NiceHashMarket.WpfClient.Messages;
using NiceHashMarket.WpfClient.Properties;
using Order = NiceHashMarket.Model.Order;

namespace NiceHashMarket.WpfClient.ViewModels
{
    [POCOViewModel]
    public class AlgoMarketViewModel : IHaveMyOrders, IHaveOrdersStorage, IDataCallBacks, ICanJump, ICanAutoStart, ICanAddOrder
    {
        private const string LbrySuprnovaUrl = "https://lbry-api.suprnova.cc";
        private const string LbryCoinmineUrl = "https://www2.coinmine.pl/lbc";
        private const double OrderAmount = 0.01;
        private const int TimeWaitBetweenApiCalls = 500;
        private const int LowLevelOfDiff = 300000;
        private const int HeighLevelOfDiff = 600000;

        #region | Fields |

        private readonly Timer _timer;
        private readonly Timer _tryDecreaseTime;
        private readonly Random _random;
        private int _timerCounter;
        private OrdersStorage _ordersStorage;
        private Algorithms _algoList;
        private AlgoNiceHashEnum _currentAlgo;
        private CoinsWhatToMineEnum _currentCoin;
        private BindingList<BlockInfo> _lastBlocksSuprNova;
        private BindingList<BlockInfo> _lastBlocksCoinMinePl;

        #endregion

        #region | Properties |

        public virtual bool EnableAutoStart { get; set; }

        public virtual bool AutoStartActivated { get; set; }

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

        public virtual int CurrentDifficulty { get; set; } = -1;
        public virtual int MinDifficulity { get; set; }
        public virtual int MaxDifficulity { get; set; }
        public virtual int LowLevelDifficulty { get; set; } = LowLevelOfDiff;
        public virtual int HeighLevelDifficulty { get; set; } = HeighLevelOfDiff;
        public virtual BindingList<DashboardPoolResult> DashboardResults { get; set; } = new BindingList<DashboardPoolResult>();
        public virtual ConcurrentDictionary<int, ApiCall> LastApiCalls { get; set; } = new ConcurrentDictionary<int, ApiCall>();

        public virtual int ProgressSuprNova { get; set; } = -1;

        public virtual int ProgressCoinMinePl { get; set; } = -1;

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
            _random = new Random((int)DateTime.Now.TimeOfDay.TotalMilliseconds);
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

            //NiceBot.ApiKeys = new Dictionary<ServerEnum, List<string>>
            //{
            //    {ServerEnum.Europe, new List<string> { Settings.Default.NiceApiIdYandex, Settings.Default.NiceApiKeyYandex}},
            //    {ServerEnum.Usa, new List<string> { Settings.Default.NiceApiIdGmail, Settings.Default.NiceApiKeyGmail}}
            //};

            APIWrapper.EuropeApiId = Settings.Default.NiceApiIdYandex;
            APIWrapper.EuropeApiKey = Settings.Default.NiceApiKeyYandex;

            APIWrapper.UsaApiId = Settings.Default.NiceApiIdGmail;
            APIWrapper.UsaApiKey = Settings.Default.NiceApiKeyGmail;



            APIWrapper.Initialize(Settings.Default.NiceApiIdYandex, Settings.Default.NiceApiKeyYandex);

            GetMyOrders();

            #endregion

            _timerCounter = 0;
            _timer = new Timer(WhatToTimeTimerHandler, null, 0, 3000);

            //_tryDecreaseTime = new Timer(TryDecreaseMyOrders, null, 0, 5000);

        }

        private void TryDecreaseMyOrders(object state)
        {
            if (AutoStartActivated) return;

            MyOrders.Where(o => !LastApiCalls.ContainsKey(o.Id) || LastApiCalls.ContainsKey(o.Id) 
                && (!LastApiCalls[o.Id].LastTryDecreaseSuccess
                    && Math.Abs((LastApiCalls[o.Id].LastTryDecrease - DateTime.Now).TotalMilliseconds) > 3 * 60 * 1000
                    || Math.Abs((LastApiCalls[o.Id].LastTryDecrease - DateTime.Now).TotalMilliseconds) > 10 * 60 * 1000))
                .ForEach(myOrder =>
                {
                    Task.Factory.StartNew(() =>
                        {
                            PriceDecreaseOrder(myOrder);
                        });
                });
        }

        private void GetMyOrders()
        {
            GetMyOrdersOnServer(ServerEnum.Europe);
            GetMyOrdersOnServer(ServerEnum.Usa);
        }

        private void GetMyOrdersOnServer(ServerEnum server)
        {
            Task.Factory.StartNew(() => server.GetMyOrders(CurrentAlgo))
                .ContinueWith(myOrders =>
                {
                    if (myOrders.Result == null || !myOrders.Result.Any()) return;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        myOrders.Result.ForEach(o =>
                        {
                            var newOrder = new Order(o.ID, (decimal)o.Price, (decimal)o.SpeedLimit, (decimal)o.Speed, o.Workers, o.OrderType, o.Alive ? 0 : 1, server);
                            if (MyOrders.Any(or => or.Id == newOrder.Id))
                                MyOrders.Remove(MyOrders.First(or => or.Id == newOrder.Id));

                            MyOrders.Add(newOrder);
                        });
                    });
                });
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

                    CheckAutoStartConditions();
                    CheckAutoStopConditions();
                });
            };
            miningPortalApiInstance.BlockUpdated += (sender, block) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var updatedBlock = blocks.First(b => b.Id == block.Id);
                    var index = blocks.IndexOf(updatedBlock);
                    blocks[index].Percent = block.Percent;

                    CheckAutoStartConditions();
                    CheckAutoStopConditions();
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

                    CheckAutoStartConditions();
                    CheckAutoStopConditions();
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
                    if (dashBoardResult.Host == LbryCoinmineUrl)
                    {
                        ProgressCoinMinePl = (int) dashBoardResult.RoundProgress;
                    }
                    else if (dashBoardResult.Host == LbrySuprnovaUrl)
                    {
                        ProgressSuprNova = (int) dashBoardResult.RoundProgress;
                    }

                    CheckAutoStartConditions();
                    CheckAutoStopConditions();
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

        public DateTime DoJump(Order targetOrder)
        {
            var newPrice = targetOrder.Price + _random.Next(1, 4) * 0.0001m;

            var myOrderForJump = MyOrders.OrderBy(o => o.Price) //OrderBy(o => o.Speed).ThenBy(o => o.Workers).ThenBy(o => o.Price)
                .FirstOrDefault(o => o.Server == targetOrder.Server && o.Price < targetOrder.Price + 0.0001m);

            if (myOrderForJump == null || _ordersStorage.GetOrderById(myOrderForJump.Id) == null)
            {
                GetMyOrders();
                return DateTime.Now;
            }

            var deltaTime = GetLastApiCallDateTimeByServer(myOrderForJump) - DateTime.Now;
            if ((decimal)(WhattomineResult.MaxPrice24 + WhattomineResult.MaxPrice24 / 100 * 30) < newPrice
                || deltaTime != null && Math.Abs(deltaTime.Value.TotalMilliseconds) < TimeWaitBetweenApiCalls)
                return DateTime.Now;

            var jumpGuid = Guid.NewGuid();

            MarketLogger.Information($"{DateTime.Now} ms: {deltaTime?.TotalMilliseconds ?? -1} JumpGuid:{jumpGuid} server:{targetOrder.Server} order:{myOrderForJump.Id} price:{myOrderForJump.Price} jumpTo:{newPrice}");

            AddOrUpdateLastApiCallDateTime(myOrderForJump);

            Task.Factory.StartNew(() => myOrderForJump.SetPrice(CurrentAlgo, (double) newPrice))
                .ContinueWith(t =>
                {
                    var priceResult = t.Result;

                    if (Math.Abs(priceResult - -1) < 0.00001)
                    {
                        MarketLogger.Error(
                            $"{DateTime.Now} JumpGuid:{jumpGuid} server:{targetOrder.Server} id:{myOrderForJump.Id} price NOT changed!");
                    }
                    else
                    {
                        MarketLogger.Information(
                            $"{DateTime.Now} JumpGuid:{jumpGuid} server:{targetOrder.Server} id:{myOrderForJump.Id} price changed, new price:{priceResult}");
                    }
                });

            return GetLastApiCallDateTime(myOrderForJump.Id);
        }

        private DateTime? GetLastApiCallDateTime()
        {
            var call = LastApiCalls.OrderByDescending(ac => ac.Value?.LastCall).FirstOrDefault();
            MarketLogger.Information("GetLastApiCallDateTime {@call} {@LastApiCalls}", call, LastApiCalls);
            return call.Value?.LastCall;
        }

        private DateTime GetLastApiCallDateTime(int orderId)
        {
            return !LastApiCalls.ContainsKey(orderId) ? DateTime.Now.AddHours(-1) : LastApiCalls[orderId].LastCall;
        }

        private DateTime? GetLastApiCallDateTimeByServer(Order order)
        {
            return !LastApiCalls.ContainsKey(order.Id) ? DateTime.Now.AddHours(-1) : LastApiCalls.Where(ac => ac.Value.Order.Server == order.Server).OrderByDescending(ac => ac.Value?.LastCall).FirstOrDefault().Value?.LastCall;
        }

        private DateTime AddOrUpdateLastApiCallDateTime(Order order)
        {
            var currentDateTime = DateTime.Now;

            LastApiCalls.AddOrUpdate(order.Id, new ApiCall{Order = order, LastCall = currentDateTime},
                (id, apiCall) => 
                {
                    apiCall.LastCall = currentDateTime;
                    return apiCall;
                });

            return currentDateTime;
        }

        private void CheckAutoStopConditions()
        {
            //if (AutoStartActivated && (ProgressSuprNova > 250 || CurrentDifficulty > HeighLevelDifficulty))
            //{
            //    Messenger.Default.Send(new CheckAutoStartMessage { Checked = false });

            //    AutoStartActivated = false;

            //    MarketLogger.Information($"AutoStop !!! progressSuprNova:{ProgressSuprNova} currentLevelDiff:{CurrentDifficulty} heighLevelDiff:{HeighLevelDifficulty}");

            //    MyOrders.ForEach(myOrder =>
            //    {
            //        Task.Factory
            //            .StartNew(() =>
            //            {
            //                //lock (this)
            //                {
            //                    ChangeLimitMyOrder(myOrder, 0.01);
            //                    PriceDecreaseOrder(myOrder);
            //                }
            //            });
            //    });
            //}
        }

        private void ChangeLimitMyOrder(Order myOrder, double newLimit, byte iterationNumber = 0)
        {
            iterationNumber++;

            MarketLogger.Information($"Изменение лимита {newLimit} iteration: {iterationNumber} idOrder:{myOrder}");

            WaitAllowApiCommand(TimeWaitBetweenApiCalls, myOrder);

            var changedLimit = myOrder.SetLimit(CurrentAlgo, newLimit);

            AddOrUpdateLastApiCallDateTime(myOrder);

            if (changedLimit < 0)
            {
                MarketLogger.Error($"Ошибка изменения ЛИМИТА для ордера iteration: {iterationNumber} idOrder:{myOrder} newLimit:{newLimit}");

                if (iterationNumber < 3)
                {
                    ChangeLimitMyOrder(myOrder, newLimit, iterationNumber);
                }
            }
            else
                MarketLogger.Information(
                    $"ЛИМИТ ордера успешно изменен до {newLimit} iteration: {iterationNumber} idOrder:{myOrder} changedLimit:{changedLimit}");
        }

        private void PriceDecreaseOrder(Order myOrder, byte iterationNumber = 0)
        {
            iterationNumber++;

            MarketLogger.Information($"Понижение цены iteration: {iterationNumber} idOrder:{myOrder}");

            WaitAllowApiCommand(TimeWaitBetweenApiCalls, myOrder);

            var lastApiCallForOrder = LastApiCalls.FirstOrDefault(c => c.Key == myOrder.Id).Value;

            var newPrice = myOrder.SetPriceDecrease(CurrentAlgo);

            AddOrUpdateLastApiCallDateTime(myOrder);

            if (lastApiCallForOrder == null)
                lastApiCallForOrder = LastApiCalls.FirstOrDefault(c => c.Key == myOrder.Id).Value;

            if (newPrice < 0)
            {
                MarketLogger.Error($"Ошибка понижения ЦЕНЫ для ордера idOrder:{myOrder}");
                lastApiCallForOrder.LastTryDecrease = DateTime.Now;
                lastApiCallForOrder.LastTryDecreaseSuccess = false;
            }
            else
            {
                MarketLogger.Information($"ЦЕНА ордера успешно понижена до {newPrice} idOrder:{myOrder}");
                lastApiCallForOrder.LastTryDecrease = DateTime.Now;
                lastApiCallForOrder.LastTryDecreaseSuccess = true;
            }
        }

        private void WaitAllowApiCommand(int milliseconds, Order order)
        {
            var iterationCounter = 0;
            DateTime? lastCall;
            do
            {
                iterationCounter++;
                Thread.Sleep(100);
                //MarketLogger.Information($"WaitAllowApiCommand iterationCounter:{iterationCounter} / milliseconds:{milliseconds} ");
                lastCall = GetLastApiCallDateTimeByServer(order);
            }

            while (lastCall != null && Math.Abs((lastCall - DateTime.Now).Value.TotalMilliseconds) < milliseconds);
        }

        private void CheckAutoStartConditions()
        {
            if (!EnableAutoStart || AutoStartActivated || CurrentDifficulty < 0 || CurrentDifficulty > LowLevelDifficulty /*|| ProgressSuprNova < 0 || ProgressSuprNova > 30*/)
                return;

            MarketLogger.Information($"AutoStart !!! progressSuprNova:{ProgressSuprNova} currentLevelDiff:{CurrentDifficulty} lowLevelDiff:{LowLevelDifficulty} heighLevelDiff:{HeighLevelDifficulty}");

            AutoStartActivated = true;
            Messenger.Default.Send(new CheckAutoStartMessage { Checked = true });

            var workLimit = 5;

            MyOrders.Where(o => o.Amount < workLimit)
                .ForEach(myOrder =>
            {
                Task.Factory.StartNew(() =>
                {
                    //lock (this)
                    {
                        ChangeLimitMyOrder(myOrder, workLimit);
                    }
                });
            });
        }

        public void AddNewOrder(ServerEnum server, decimal minPriceOnServer)
        {
            var balance = server.GetBalance();
            if (balance.Confirmed < 0.005)
            {
                MarketLogger.Error($"Недостаточно средств на NiceHash кошельке: {balance.Confirmed}. Минимальный объем 0.005");
                return;
            }

            var amount = balance.Confirmed < OrderAmount + 0.005 ? balance.Confirmed : OrderAmount;

            var pool = new NiceHashBotLib.Pool{Label = "LBC SuprNova", Host = "lbry.suprnova.cc", Port = 6257, User = "wchasik.nice1", Password = "x"};
            var limit = _random.Next(3, 8) + _random.Next(1, 99) / 100.0;

            //var pool = new NiceHashBotLib.Pool { Label = "LBC CoinMine", Host = "lbc.coinmine.pl", Port = 8788, User = "wchasik.nice1", Password = "x" };
            //var limit = 0.02;

            var price = (double) (minPriceOnServer + _random.Next(1, 99) / 10000.0m);

            Task.Factory.StartNew(() => server.OrderCreate(CurrentAlgo, amount, price, limit, pool)
            ).ContinueWith(t =>
            {
                if (t.Result == 0)
                {
                    MarketLogger.Error($"Add new order failed! {server} price: {price} limit:{limit}");
                }
                else
                {
                    MarketLogger.Information($"Added new order success! {server} price: id:{t.Result} {price} limit:{limit}");
                    GetMyOrders();
                }
            });
        }

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