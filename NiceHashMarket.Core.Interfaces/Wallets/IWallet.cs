using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using NiceHashMarket.Core.Interfaces.Blocks;
using NiceHashMarket.Core.Interfaces.Transactions;

namespace NiceHashMarket.Core.Interfaces.Wallets
{
    public interface IWallet
    {
        event EventHandler<double> DifficultyChanged;

        event EventHandler<long> NewBlockIdFounded;

        event EventHandler<IBlock> NewBlockCreated;

        double Difficulty { get; set; }

        long BlockId { get; set; }

        string BlockHash { get; set; }

        long LastBlockId();

        void WaitNewBlock(int period);

        IBlock Block(long blockId);

        IBlock BlockById(long blockId);

        IBlock BlockByHash(string hash);

        ObservableCollection<IBlock> Blocks(long startInterval, long finishInterval);

        ITransaction TransactionByTxId(string txId);

        ITransaction RewardTransaction(IBlock block);

        IEnumerable<ITransaction> Transactions(IBlock block);

        #region | Async methods |

        Task<long> LastBlockIdAsync();

        Task<IBlock> BlockAsync(long blockId);

        Task<IBlock> BlockByIdAsync(long blockId);

        #endregion
    }
}
