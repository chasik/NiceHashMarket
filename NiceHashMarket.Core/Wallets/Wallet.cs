using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NiceHashMarket.Core.Interfaces.Blocks;
using NiceHashMarket.Core.Interfaces.Transactions;
using NiceHashMarket.Core.Interfaces.Wallets;
using NiceHashMarket.Logger;
using NiceHashMarket.Model.Enums;
using RestSharp;

namespace NiceHashMarket.Core.Wallets
{
    public class Wallet : IWallet
    {
        private readonly string _connectionUrl;
        private readonly string _connectionUri;

        protected readonly CoinsWhatToMineEnum _coin;

        private double _difficulty;
        private long _lastBlockId;
        private string _lastBlockHash;

        private Timer _timer;

        public event EventHandler<double> DifficultyChanged;
        public event EventHandler<long> NewBlockIdFounded;
        public event EventHandler<IBlock> NewBlockCreated;

        public double Difficulty
        {
            get => _difficulty;
            set
            {
                if (Math.Abs(value - _difficulty) > 0.001)
                {
                    OnDifficultyChanged(value);
                }

                _difficulty = value;
            }
        }

        public long BlockId
        {
            get => _lastBlockId;
            set
            {
                if (value > _lastBlockId)
                {
                    OnNewBlockIdFounded(value);
                }

                _lastBlockId = value;
            }
        }

        public string BlockHash
        {
            get => _lastBlockHash;
            set
            {
                if (value != _lastBlockHash)
                {
                }

                _lastBlockHash = value;
            }
        }

        public Wallet(CoinsWhatToMineEnum coin, string connectionUrl, string connectionUri)
        {
            _coin = coin;
            _connectionUrl = connectionUrl;
            _connectionUri = connectionUri;
        }

        protected JObject CreateRpcJsonRequest(JsonObject jsonObject)
        {
            var restRequest = new RestRequest(_connectionUri, Method.POST) {RequestFormat = DataFormat.Json};

            restRequest.AddBody(jsonObject);

            var response = new RestClient(_connectionUrl).Execute(restRequest);

            return (JObject)JsonConvert.DeserializeObject(response.Content);
        }

        public virtual long LastBlockId()
        {
            throw new NotImplementedException();
        }

        public void WaitNewBlock(int period)
        {
            _timer = new Timer(state => { LastBlockId(); }, null, 0, period);
        }

        public virtual IBlock BlockById(long blockId)
        {
            throw new NotImplementedException();
        }

        public virtual IBlock BlockByHash(string hash)
        {
            throw new NotImplementedException();
        }

        public virtual IBlock Block(long blockId)
        {
            return Blocks(blockId, blockId).SingleOrDefault();
        }

        public virtual ObservableCollection<IBlock> Blocks(long startInterval, long finishInterval)
        {
            throw new NotImplementedException();
        }

        public virtual ITransaction TransactionByTxId(string txId)
        {
            throw new NotImplementedException();
        }

        public virtual ITransaction RewardTransaction(IBlock block)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<ITransaction> Transactions(IBlock block)
        {
            throw new NotImplementedException();
        }

        #region | Async methods |

        public Task<IBlock> BlockAsync(long blockId)
        {
            return Task.Factory.StartNew(() => Block(blockId));
        }

        public Task<IBlock> BlockByIdAsync(long blockId)
        {
            return Task.Factory.StartNew(() => BlockById(blockId));
        }

        public Task<IBlock> BlockByHashAsync(string hash)
        {
            return Task.Factory.StartNew(() => BlockByHash(hash));
        }

        public Task<long> LastBlockIdAsync()
        {
            return Task.Factory.StartNew(LastBlockId);
        }

        public Task<ITransaction> TransactionByTxIdAsync(string txId)
        {
            return Task.Factory.StartNew(() => TransactionByTxId(txId));
        }

        #endregion

        protected virtual void OnNewBlockIdFounded(long e)
        {
            NewBlockIdFounded?.Invoke(this, e);
        }

        protected virtual void OnNewBlockCreated(IBlock e)
        {
            NewBlockCreated?.Invoke(this, e);
        }

        protected virtual void OnDifficultyChanged(double e)
        {
            DifficultyChanged?.Invoke(this, e);
        }
    }
}
