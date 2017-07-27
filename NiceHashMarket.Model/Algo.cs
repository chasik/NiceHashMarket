using System.Runtime.Serialization;
using NiceHashMarket.Model.Interfaces;

namespace NiceHashMarket.Model
{
    [DataContract]
    public class Algo : IAlgo
    {
        [DataMember]
        public byte Id { get; set; }

        [DataMember]
        public string Name { get; set; }
    }
}
