using System;
using System.Collections.Generic;
using NiceHashMarket.Core.Interfaces.Transactions;

namespace NiceHashMarket.Core.Interfaces.Blocks
{
    public interface IBlock
    {
        long Id { get; set; }

        double Difficulty { get; set; }

        DateTime Created { get; set; }

        ITransaction RewardTransaction { get; set; }

        IList<ITransaction> Transactions { get; set; }
    }
}