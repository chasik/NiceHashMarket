using System.ServiceModel;
using NiceHashMarket.Model.Interfaces;

namespace NiceHashMarket.Services
{
    [ServiceContract(CallbackContract = typeof(IDataCallBacks), SessionMode = SessionMode.Required)]
    public interface IDataService
    {
        [OperationContract(IsOneWay = true)]
        void ListenAlgo(IAlgo algo);
    }

}
