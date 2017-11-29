using System.Collections.Generic;
using NiceHashMarket.Core.Interfaces.Transactions;

namespace NiceHashMarket.Core.Transactions
{
    public class Transaction : ITransaction
    {
        public string TxId { get; set; }

        public ICollection<IInput> Inputs { get; set; }
        public ICollection<IOutput> Outputs { get; set; }
    }
}
