using System.Collections.ObjectModel;
using System.Threading.Tasks;
using NiceHashMarket.Core.Interfaces.Blocks;
using NiceHashMarket.Core.Interfaces.Transactions;

namespace NiceHashMarket.Core.Interfaces.Wallets
{
    public interface IWallet
    {
        long LastBlockId();

        IBlock BlockById(long blockId);

        ObservableCollection<IBlock> Blocks(long startInterval, long finishInterval);

        ITransaction TransactionByTxId(string txId);

        #region | Async methods |

        Task<long> LastBlockIdAsync();

        Task<IBlock> BlockByIdAsync(long blockId);

        #endregion
    }
}
