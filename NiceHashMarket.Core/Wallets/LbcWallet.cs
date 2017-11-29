using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;
using NiceHashMarket.Core.Factories;
using NiceHashMarket.Core.Interfaces.Blocks;
using NiceHashMarket.Model.Enums;
using RestSharp;
using NiceHashMarket.Core.Helpers;
using NiceHashMarket.Core.Interfaces.Transactions;
using NiceHashMarket.Core.Transactions;

namespace NiceHashMarket.Core.Wallets
{
    public class LbcWallet : Wallet
    {
        public LbcWallet(string connectionUrl, string connectionUri) 
            : base(CoinsWhatToMineEnum.Lbc, connectionUrl, connectionUri)
        {
        }

        public override long LastBlockId()
        {
            var jsonResponse = CreateRpcJsonRequest(new JsonObject { ["method"] = "status" });

            long.TryParse(jsonResponse["result"]["blockchain_status"]["blocks"].ToString(), out var lastBlockId);

            return lastBlockId;
        }

        public override ObservableCollection<IBlock> Blocks(long startInterval, long finishInterval)
        {
            var blocks = new ObservableCollection<IBlock>();

            if (startInterval <= finishInterval)
            {
                for (var i = startInterval; i <= finishInterval; i++)
                {
                    BlockByIdAsync(i)
                        .ContinueWith(t =>
                        {
                            var loadedTransaction = t.Result.Transactions.Take(1)
                                .Select(transaction =>
                                {
                                    try
                                    {
                                        return TransactionByTxId(transaction.TxId);
                                    }
                                    catch (Exception ex)
                                    {
                                        Trace.WriteLine(ex.Message);
                                        return transaction;
                                    }
                                }).ToList();

                            t.Result.Transactions = loadedTransaction;

                            return t.Result;
                        })
                        .ContinueWith(t => blocks.Add(t.Result));
                }
            }
            else
            {
                for (var i = finishInterval; i >= startInterval; i--)
                {
                    BlockByIdAsync(i).ContinueWith(t => blocks.Add(t.Result));
                }
            }

            return blocks;
        }

        public override IBlock BlockById(long blockId)
        {
            var jsonResponse = CreateRpcJsonRequest(new JsonObject { ["method"] = "block_show", ["params"] = new JsonObject { ["height"] = blockId } });

            var block = _coin.CreateBlock();

            block.Id = long.Parse(jsonResponse["result"]["height"].ToString(), CultureInfo.InvariantCulture);
            block.Difficulty = double.Parse(jsonResponse["result"]["difficulty"].ToString(), NumberStyles.AllowDecimalPoint | NumberStyles.Float);

            var createdUnix = long.Parse(jsonResponse["result"]["time"].ToString(), CultureInfo.InvariantCulture);
            block.Created = createdUnix.ToDateTime(DateTimeKind.Local);

            block.Transactions = new List<ITransaction>();

            if (!(jsonResponse["result"]["tx"] is JArray transactions)) return block;

            foreach (var jTransaction in transactions)
            {
                var transaction = _coin.CreateTransaction();
                transaction.TxId = jTransaction.ToString();

                block.Transactions.Add(transaction);
            }

            return block;
        }

        public override ITransaction TransactionByTxId(string txId)
        {
            var jsonResponse = CreateRpcJsonRequest(new JsonObject { ["method"] = "transaction_show", ["params"] = new JsonObject { ["txid"] = txId } });

            if (jsonResponse.TryGetValue("error", out var errorResponse))
            {
                throw new Exception($"TransactionById error: {errorResponse.ToString()}");
            }

            var inputs = jsonResponse["result"]["inputs"] as JArray;
            var outputs = jsonResponse["result"]["outputs"] as JArray;

            var transaction = _coin.CreateTransaction();
            transaction.TxId = txId;
            transaction.Inputs = new List<IInput>();
            transaction.Outputs = new List<IOutput>();

            foreach (var input in inputs)
            {
                var lbcInput = _coin.CreateInput();

                lbcInput.IsCoinBase = input["is_coinbase"].Value<bool>();

                if (!lbcInput.IsCoinBase)
                {
                    lbcInput.Address = input["address"].ToString();
                }

                transaction.Inputs.Add(lbcInput);
            }
            
            foreach (var output in outputs)
            {
                var lbcOutput = _coin.CreateOutput();
                lbcOutput.Address = output["address"].ToString();
                lbcOutput.Value = output["value"].Value<long>() / 100000000;
                transaction.Outputs.Add(lbcOutput);
            }

            return transaction;
        }
    }
}
