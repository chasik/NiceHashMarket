using System.Collections.Generic;

namespace NiceHashMarket.Core.Interfaces.Transactions
{
    public interface ITransaction
    {
        string TxId { get; set; }

        ICollection<IInput> Inputs { get; set; }

        ICollection<IOutput> Outputs { get; set; }
    }
}
