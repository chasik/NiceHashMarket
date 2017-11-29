using System;
using System.Collections.Generic;
using NiceHashMarket.Core.Interfaces.Blocks;
using NiceHashMarket.Core.Interfaces.Transactions;

namespace NiceHashMarket.Core.Blocks
{
    public class Block : IBlock
    {
        public long Id { get; set; }

        public double Difficulty { get; set; }

        public DateTime Created { get; set; }

        public IList<ITransaction> Transactions { get; set; }
    }
}
