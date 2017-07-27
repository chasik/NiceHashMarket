using System.ServiceModel;
using NiceHashMarket.Model;

namespace NiceHashMarket.Services
{
    [ServiceContract]
    public interface IDataCallBacks
    {
        [OperationContract(IsOneWay = true)]
        void OrderAdded(Order order);

        [OperationContract(IsOneWay = true)]
        void OrderChanged(Order order);
    }
}
