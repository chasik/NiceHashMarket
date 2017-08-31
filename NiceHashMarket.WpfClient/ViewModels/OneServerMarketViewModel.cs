using System;
using System.ComponentModel;
using System.Linq;
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

namespace NiceHashMarket.WpfClient.ViewModels
{
    [POCOViewModel]
    public class OneServerMarketViewModel : ISupportParentViewModel, ISupportParameter
    {
        private DateTime _lastJumpOnServerDateTime;
        public virtual ServerEnum Server { get; set; }
        public virtual NiceBindingList<Order> Orders { get; set; }

        public virtual object Parameter { get; set; }

        public virtual object ParentViewModel { get; set; }

        public virtual Order OrderUpJumpLevel { get; set; }
        public virtual NiceBindingList<Order> JumpedOrders { get; set; } = new NiceBindingList<Order>();

        public virtual bool CatchUp { get; set; }
        public virtual decimal CatchUpMaxPrice { get; set; }


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
            ordersStorage.AlgoChanged += OrdersStorageOnAlgoChanged;

            OrdersStorageOnAlgoChanged(ordersStorage, null, null);

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

        private void OrdersStorageOnAlgoChanged(DataStorage<Order> sender, IAlgo oldalgo, IAlgo newalgo)
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
        }

        private void CalcLevelForJump()
        {
            OrderUpJumpLevel = null;
            JumpedOrders.Clear();
            var speedLimit = 0.1m;
            var workersLimit = 800;
            var speedSumm = 0m;
            var workersSumm = 0;

            Orders.Where(o => o.Workers > 0
                && o.Active && o.Type == OrderTypeEnum.Standart
                && (ParentViewModel as IHaveMyOrders == null || ((IHaveMyOrders)ParentViewModel).MyOrders.All(myOrder => myOrder.Id != o.Id)))
                .OrderBy(o => o.Price).ForEach(o =>
                {
                    if (OrderUpJumpLevel != null)
                        return;

                    JumpedOrders.Add(o);

                    speedSumm += o.Speed;
                    workersSumm += o.Workers;

                    if (/*speedSumm > speedLimit || */workersSumm > workersLimit)
                        OrderUpJumpLevel = o;
                });

            this.RaisePropertyChanged(vm => vm.JumpedOrders);

            if (OrderUpJumpLevel == null || Math.Abs((_lastJumpOnServerDateTime - DateTime.Now).TotalMilliseconds) < 100)
                return;

            //MarketLogger.Information(
            //    $"OneServerMarketViewModel JumpedOrders.Count:{JumpedOrders.Count} {Server} JumpTo OrderId: {OrderUpJumpLevel.Id} OrderPrice:{OrderUpJumpLevel.Price} speedSumBelow:{speedSumm}");

            if (CatchUp)
                DoJump(OrderUpJumpLevel);
            else
                _lastJumpOnServerDateTime = DateTime.Now;
        }

        private void DoJump(Order order)
        {
            _lastJumpOnServerDateTime = (ParentViewModel as ICanJump)?.DoJump(order) ?? DateTime.Now;
        }
    }
}