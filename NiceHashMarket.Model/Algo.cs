using NiceHashMarket.Model.Interfaces;

namespace NiceHashMarket.Model
{
    public class Algo : IAlgo
    {
        public byte Id { get; set; }
        public string Name { get; set; }
    }
}
