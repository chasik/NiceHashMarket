using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NiceHashMarket.Core.Interfaces.Blocks;
using NiceHashMarket.Core.Interfaces.Transactions;
using NiceHashMarket.Core.Interfaces.Wallets;
using NiceHashMarket.Model.Enums;
using RestSharp;

namespace NiceHashMarket.Core.Wallets
{
    public class Wallet : IWallet
    {
        private readonly string _connectionUrl;
        private readonly string _connectionUri;

        protected readonly CoinsWhatToMineEnum _coin;

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

        public virtual IBlock BlockById(long blockId)
        {
            throw new NotImplementedException();
        }

        public virtual ObservableCollection<IBlock> Blocks(long startInterval, long finishInterval)
        {
            throw new NotImplementedException();
        }

        public virtual ITransaction TransactionByTxId(string txId)
        {
            throw new NotImplementedException();
        }

        #region | Async methods |
       
        public Task<IBlock> BlockByIdAsync(long blockId)
        {
            return Task.Factory.StartNew(() => BlockById(blockId));

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
    }
}
