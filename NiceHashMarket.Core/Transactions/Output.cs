using NiceHashMarket.Core.Interfaces.Transactions;

namespace NiceHashMarket.Core.Transactions
{
    public class Output : IOutput
    {
        public string Address { get; set; }
        public double Value { get; set; }
    }
}
