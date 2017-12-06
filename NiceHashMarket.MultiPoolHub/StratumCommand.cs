using System.Collections;
using System.Runtime.Serialization;

namespace NiceHashMarket.MultiPoolHub
{
    [DataContract]
    public class StratumCommand
    {
        [DataMember(Name = "method")]
        public string Method;
        
        [DataMember(Name = "id")]
        public int? Id;
        
        [DataMember(Name = "params")]
        public ArrayList Parameters;
    }
}
