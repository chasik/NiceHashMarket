using System.Collections;
using System.Runtime.Serialization;

namespace NiceHashMarket.MultiPoolHub
{
    [DataContract]
    public class StratumResponse
    {
        [DataMember(Name = "error")]
        public ArrayList Error;

        [DataMember(Name = "id")]
        public int? Id;

        [DataMember(Name = "result")]
        public object Result;
    }
}
