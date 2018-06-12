using System.ServiceModel;
using NiceHashMarket.Model;
using NiceHashMarket.Model.Interfaces;

namespace NiceHashMarket.Services
{
    [ServiceKnownType(typeof(Algo))]
    [ServiceContract(CallbackContract = typeof(IDataCallBacks), SessionMode = SessionMode.Required)]
    public interface IDataService
    {
        [OperationContract(IsOneWay = true)]
        void ListenAlgo(IAlgo algo);
    }
}
