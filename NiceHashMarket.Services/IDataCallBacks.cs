using System.ServiceModel;
using NiceHashMarket.Model;

namespace NiceHashMarket.Services
{
    [ServiceContract]
    public interface IDataCallBacks
    {
        [OperationContract]
        void OrderAdded(Order order);
    }
}
