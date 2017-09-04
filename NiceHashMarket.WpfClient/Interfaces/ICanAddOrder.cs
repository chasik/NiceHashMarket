using NiceHashMarket.Model.Enums;

namespace NiceHashMarket.WpfClient.Interfaces
{
    public interface ICanAddOrder
    {
        void AddNewOrder(ServerEnum server, decimal minPriceOnServer);
    }
}
