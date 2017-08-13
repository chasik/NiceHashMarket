using NiceHashMarket.Core;

namespace NiceHashMarket.WpfClient.Interfaces
{
    public interface IHaveOrdersStorage
    {
        OrdersStorage OrdersStorage { get; set; }
    }
}
