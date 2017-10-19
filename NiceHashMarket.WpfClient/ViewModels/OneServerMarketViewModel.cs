using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.POCO;
using NiceHashMarket.Core;
using NiceHashMarket.Core.Helpers;
using NiceHashMarket.Logger;
using NiceHashMarket.Model;
using NiceHashMarket.Model.Enums;
using NiceHashMarket.Model.Interfaces;
using NiceHashMarket.WpfClient.Interfaces;
using NiceHashMarket.WpfClient.Messages;

namespace NiceHashMarket.WpfClient.ViewModels
{
    [POCOViewModel]
    public class OneServerMarketViewModel : ISupportParentViewModel, ISupportParameter
    {
        private DateTime _lastJumpOnServerDateTime;
        private Timer _timer;
        public virtual ServerEnum Server { get; set; }
        public virtual NiceBindingList<Order> Orders { get; set; }

        public virtual object Parameter { get; set; }

        public virtual object ParentViewModel { get; set; }

        public virtual Order OrderUpJumpLevel { get; set; }
        public virtual NiceBindingList<Order> JumpedOrders { get; set; } = new NiceBindingList<Order>();

        public virtual bool CatchUp { get; set; }

        public virtual decimal SpeedOnServer { get; set; }
        public virtual decimal MySpeedOnServer { get; set; }

        public virtual int WorkersOnServer { get; set; }
        public virtual int MyWorkersOnServer { get; set; }

        public OneServerMarketViewModel()
        {
            Messenger.Default.Register<CheckAutoStartMessage>(this, m =>
            {
                if (m.Checked)
                {
                    CalcLevelForJump();
                    CalcServersPowers();
                }

                CatchUp = m.Checked;

                MarketLogger.Information($"!!! AutoStart message {m.Checked}!!!");
            });

            _timer = new Timer(state =>
            {
                CalcLevelForJump();
            }, null, 0, 500);
        }

        protected void OnParameterChanged()
        {
            Orders = new NiceBindingList<Order>();

            Server = (ServerEnum)Enum.Parse(typeof(ServerEnum), Parameter.ToString());
        }

        protected void OnParentViewModelChanged()
        {
            var ordersStorage = (ParentViewModel as IHaveOrdersStorage)?.OrdersStorage;

            if (ordersStorage == null)
                throw new ArgumentNullException("Orders Storage не может быть равным null!");

            ordersStorage.AlgoChanging += OrdersStorage_AlgoChanging;
            ordersStorage.AlgoChanged += OrdersStorage_AlgoChanged;

            OrdersStorage_AlgoChanged(ordersStorage, null, null);

            ordersStorage.Entities
                .Where(o => o.Server == Server)
                .ForEach(o =>
                {
                    if (Orders.All(oo => o.Id != oo.Id)) Orders.Add((Order) o.Clone());
                });
        }

        private void OrdersStorage_AlgoChanging(DataStorage<Order> sender, IAlgo oldAlgo, IAlgo newAlgo)
        {
            sender.Entities.ListChanged -= Entities_ListChanged;
            sender.Entities.BeforeRemove -= Entities_BeforeRemove;
        }

        private void OrdersStorage_AlgoChanged(DataStorage<Order> sender, IAlgo oldalgo, IAlgo newalgo)
        {
            sender.Entities.ListChanged += Entities_ListChanged;
            sender.Entities.BeforeRemove += Entities_BeforeRemove;

            Orders.Clear();
        }

        private void Entities_BeforeRemove(Order deletedItem)
        {
            Orders.RemoveIfExistById(deletedItem.Id);
        }

        private void Entities_ListChanged(object sender, ListChangedEventArgs e)
        {
            var orders = sender as BindingList<Order>;
            var orderNewIndex = e.NewIndex > -1 && e.NewIndex < orders?.Count ? orders[e.NewIndex] : null;

            if (orderNewIndex == null || orderNewIndex.Server != Server) return;

            switch (e.ListChangedType)
            {
                case ListChangedType.Reset:
                    break;
                case ListChangedType.ItemAdded:
                    Orders.Add((Order)orderNewIndex.Clone());

                    break;
                case ListChangedType.ItemDeleted:
                    // TODO remove from my orders
                    //var myOrders = (ParentViewModel as IHaveMyOrders)?.MyOrders;

                    break;
                case ListChangedType.ItemMoved:
                    break;
                case ListChangedType.ItemChanged:
                    var orderChanged = Orders.FirstOrDefault(o => o.Id == orderNewIndex.Id);

                    if (orderChanged == null)
                    {
                        Orders.Add((Order)orderNewIndex.Clone());
                        break;
                    }

                    orderChanged.History.AddValue(e.PropertyDescriptor.Name, orderChanged, orderNewIndex);

                    orderNewIndex.CopyProperties(orderChanged);

                    var myOrders = (ParentViewModel as IHaveMyOrders)?.MyOrders;
                    myOrders?.Where(mo => mo.Id == orderChanged.Id)
                        .ForEach(mo =>
                        {
                            mo.Price = orderChanged.Price;
                            mo.Amount = orderChanged.Amount;
                            mo.Workers = orderChanged.Workers;
                            mo.Speed = orderChanged.Speed;

                            CalcMyCurrentPowers(myOrders);
                        });
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

            CalcLevelForJump();
            CalcServersPowers();
        }

        private void CalcServersPowers()
        {
            SpeedOnServer = Orders.Sum(o => o.Speed);
            WorkersOnServer = Orders.Sum(o => o.Workers);
        }

        private void CalcMyCurrentPowers(NiceBindingList<Order> myOrders)
        {
            MySpeedOnServer = myOrders.Where(o => o.Server == Server).Sum(o => o.Speed);
            MyWorkersOnServer = myOrders.Where(o => o.Server == Server).Sum(o => o.Workers);
            //MyAmountOnServer = myOrders.Sum(o => o.Amount);
        }

        private void CalcLevelForJump()
        {
            if (Orders == null || !Orders.Any())
                return;

                OrderUpJumpLevel = null;
            JumpedOrders.Clear();
            //var speedLimit = 0.1m;
            var workersPercentLimit = 60;
            //var speedSumm = 0m;
            var workersSumm = 0;

            var calculatedOrders = Orders.Where(o => o.Workers > 0
                && o.Active && o.Type == OrderTypeEnum.Standart
                && (!(ParentViewModel is IHaveMyOrders) || ((IHaveMyOrders)ParentViewModel).MyOrders.All(myOrder => myOrder.Id != o.Id)))
                .OrderBy(o => o.Price).ToList();

            var workersAll = calculatedOrders.Sum(o => o.Workers);

            calculatedOrders.ForEach(o =>
                {
                    if (OrderUpJumpLevel != null)
                        return;

                    JumpedOrders.Add(o);

                    //speedSumm += o.Speed;
                    workersSumm += o.Workers;

                    if (workersSumm * 100 / workersAll >= workersPercentLimit /*speedSumm > speedLimit || */)
                        OrderUpJumpLevel = o;
                });

            this.RaisePropertyChanged(vm => vm.JumpedOrders);

            if (OrderUpJumpLevel == null || Math.Abs((_lastJumpOnServerDateTime - DateTime.Now).TotalMilliseconds) < 60)
                return;

            //MarketLogger.Information(
            //    $"OneServerMarketViewModel JumpedOrders.Count:{JumpedOrders.Count} {Server} JumpTo OrderId: {OrderUpJumpLevel.Id} OrderPrice:{OrderUpJumpLevel.Price} speedSumBelow:{speedSumm}");

            _lastJumpOnServerDateTime = CatchUp 
                ? DoJump(OrderUpJumpLevel) 
                : DateTime.Now;
        }

        private DateTime DoJump(Order targetOrder)
        {
            return (ParentViewModel as ICanJump)?.DoJump(targetOrder) ?? DateTime.Now;
        }

        public void AddNewOrder()
        {
            var minPriceOrder = Orders.Where(o => o.Active && o.Type == OrderTypeEnum.Standart && o.Workers < 1)
                .OrderBy(o => o.Price).FirstOrDefault();

            if (minPriceOrder == null) return;

            (ParentViewModel as ICanAddOrder)?.AddNewOrder(Server, minPriceOrder.Price);
        }
    }
}