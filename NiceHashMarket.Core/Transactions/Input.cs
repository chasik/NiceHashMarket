using NiceHashMarket.Core.Interfaces.Transactions;

namespace NiceHashMarket.Core.Transactions
{
    public class Input : IInput
    {
        public bool IsCoinBase { get; set; }
        public string Address { get; set; }
    }
}
