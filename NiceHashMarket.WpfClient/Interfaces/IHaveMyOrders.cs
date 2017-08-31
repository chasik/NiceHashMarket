using NiceHashMarket.Core;
using NiceHashMarket.Model;

namespace NiceHashMarket.WpfClient.Interfaces
{
    public interface IHaveMyOrders
    {
        NiceBindingList<Order> MyOrders { get; set; }
    }
}
