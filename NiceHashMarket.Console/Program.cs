using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using NiceHashMarket.Core;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NiceHashBotLib;
using NiceHashMarket.Console.Properties;
using NiceHashMarket.Core.Factories;
using NiceHashMarket.Core.Helpers;
using NiceHashMarket.Core.Interfaces.Blocks;
using NiceHashMarket.Core.Interfaces.Wallets;
using NiceHashMarket.Core.Wallets;
using NiceHashMarket.Model;
using NiceHashMarket.Model.Enums;
using NiceHashMarket.Model.Interfaces;
using NiceHashMarket.MultiPoolHub;
using Renci.SshNet;
using Renci.SshNet.Common;
using RestSharp;
using Order = NiceHashBotLib.Order;

namespace NiceHashMarket.Console
{
    class Program
    {
        private static IList<FarmConnectionInfo> _farms;

        private static List<BlockInfo> _blocksInfo { get; set; } = new List<BlockInfo>();

        static void Main(string[] args)
        {
            var serversList = new List<StratumConnection>
            {

                StratumConnection.Parse(AlgoNiceHashEnum.DaggerHashimoto, "europe.ethash-hub.miningpoolhub.com,12020,wchasik,vegaKlet,x"),
                //groestl - StratumConnection.Parse("hub.miningpoolhub.com,12004,wchasik,home,x");
                //myr-gr - StratumConnection.Parse("hub.miningpoolhub.com,12005,wchasik,home,x");

                StratumConnection.Parse(AlgoNiceHashEnum.CryptoNight, "europe.cryptonight-hub.miningpoolhub.com,12024,wchasik,vegaKlet,x"),
                StratumConnection.Parse(AlgoNiceHashEnum.Equihash, "europe.equihash-hub.miningpoolhub.com,12023,wchasik,vegaKlet,x"),

                StratumConnection.Parse(AlgoNiceHashEnum.X11,"hub.miningpoolhub.com,12007,wchasik,vegaKlet,x"),
                StratumConnection.Parse(AlgoNiceHashEnum.X13,"hub.miningpoolhub.com,12008,wchasik,vegaKlet,x"),
                StratumConnection.Parse(AlgoNiceHashEnum.X15,"hub.miningpoolhub.com,12009,wchasik,vegaKlet,x"),
                StratumConnection.Parse(AlgoNiceHashEnum.NeoScrypt,"hub.miningpoolhub.com,12012,wchasik,vegaKlet,x"),
                StratumConnection.Parse(AlgoNiceHashEnum.Qubit,"hub.miningpoolhub.com,12014,wchasik,vegaKlet,x"),
                StratumConnection.Parse(AlgoNiceHashEnum.Quark,"hub.miningpoolhub.com,12015,wchasik,vegaKlet,x"),
                // skein - StratumConnection.Parse("hub.miningpoolhub.com,12016,wchasik,home,x");
                StratumConnection.Parse(AlgoNiceHashEnum.Lyra2REv2,"hub.miningpoolhub.com,12018,wchasik,vegaKlet,x")
                // vanilla - StratumConnection.Parse("hub.miningpoolhub.com,12019,wchasik,home,x");
            };

            serversList.Take(1).ToList().ForEach(server =>
            {
                var pinger = new PingServer(server, 10000);

                pinger.PingResultChanged += result =>
                {
                    System.Console.BackgroundColor = result.Success ? ConsoleColor.Cyan : ConsoleColor.Red;

                    System.Console.WriteLine($"minutes: {(DateTime.Now - result.DateTime).TotalMinutes:#######.##} ; {result.Success} ; {result.Result}");
                };
            });



            System.Console.ReadLine();
        }

        static void Main11111111111111212111111111(string[] args)
        {
            var wallet = CoinsWhatToMineEnum.Lbc.CreateWallet();
            var lastBlockId = wallet.LastBlockId();

            const int blocksCount = 2000;
            var blocks = wallet.Blocks(lastBlockId - blocksCount, lastBlockId);

            var counterStep = 0;

            blocks.CollectionChanged += (sender, eventArgs) =>
            {
                if (eventArgs.Action != NotifyCollectionChangedAction.Add) return;
                foreach (IBlock block in eventArgs.NewItems)
                {
                    System.Console.WriteLine($"{++counterStep}  {block.Id} - {block.Created} - {block.Difficulty}");
                }

            };

            System.Console.ReadLine();

            var orderBlocks = blocks.OrderBy(b => b?.Created).ToList();
            var orderBlocks2 = blocks.OrderBy(b => b?.Created).ToList();

            var allBefore = new[] {5, 10, 15, 20, 25, 30};
            var allAfter = new[] {5, 10, 15, 20, 25, 30};

            foreach (var bef in allBefore)
            {
                foreach (var aft in allAfter)
                {
                    var before = new TimeSpan(0, bef, 0);
                    var after = new TimeSpan(0, aft, 0);

                    var randomOffset = new Random((int)DateTime.Now.TimeOfDay.TotalSeconds);

                    var lines = new List<string>();
                    orderBlocks2.ForEach(b =>
                    {
                        if (b == null) return;

                        var interval = orderBlocks.Where(bl =>
                            bl != null && bl.Created > b.Created - before && bl.Created < b.Created + after);

                        var beforeCount = interval.Count(bl => bl.Created < b.Created);
                        var afterCount = interval.Count(bl => bl.Created > b.Created);

                        var beforeValue = beforeCount - (double)randomOffset.Next(30) / 100;
                        var afterValue = afterCount - (double)randomOffset.Next(30) / 100;

                        System.Console.WriteLine($"{beforeValue} - {b.Created} - {afterValue}");

                        lines.Add($"{b.Id};{b.Created};{(int)b.Difficulty};{beforeValue};{afterValue}");
                    });

                    File.WriteAllLines($@"C:\temp\data-{blocksCount}-before_{before.TotalMinutes}-after_{after.TotalMinutes}.csv", lines);
                }
            }

            System.Console.ReadLine();
        }

        static void Main_______(string[] args)
        {
            try
            {
                var lastStart = DateTime.MinValue;
                var lastStop = DateTime.MinValue;

                _farms = FarmsStorage.LoadFromFile("FarmsConnectionInfo.txt");

                if (args.Any(a => string.Equals(a, "s", StringComparison.InvariantCultureIgnoreCase)))
                {
                    var progress = new Progress<ScriptOutputLine>(s => { System.Console.WriteLine(s.Line); });
                    var token = new CancellationToken();
                    ScreenPoolSsh(progress, token);
                    System.Console.ReadLine();
                    return;
                }

                if (args.Any(a => string.Equals(a, "d", StringComparison.InvariantCultureIgnoreCase)))
                {
                    StopMiningToPool();
                    return;
                }

                if (args.Any(a => string.Equals(a, "c", StringComparison.InvariantCultureIgnoreCase)))
                {
                    StartMiningToPool();
                    return;
                }

                var pool = new MiningPortalApi("https://lbry-api.suprnova.cc", 4000, +3, MetricPrefixEnum.Mega,
                    Settings.Default.LbrySuprnovaApiKey, Settings.Default.LbrySuprnovaUserId);

                //var pool = new MiningPortalApi("https://www2.coinmine.pl/lbc", 4000, +1, MetricPrefixEnum.Tera,
                //    Settings.Default.LbryCoinMineApiKey, Settings.Default.LbryCoinMineUserId);

                pool.RoundProgressChanged += (sender, result) =>
                {
                    System.Console.WriteLine($"RoundProgress: {result.RoundProgress} %  diff: {result.Difficulty}");
                    if (result.RoundProgress < 0) return;

                    if (result.RoundProgress < 50 && result.Difficulty < 260000
                        && Math.Abs((lastStart - DateTime.Now).TotalMilliseconds) > 30000)
                    {
                        lastStart = DateTime.Now;
                        Task.Factory.StartNew(StartMiningToPool);
                        return;
                    }

                    if ((result.RoundProgress > 100 || result.Difficulty > 400000)
                        && Math.Abs((lastStop - DateTime.Now).TotalMilliseconds) > 30000)
                    {
                        lastStop = DateTime.Now;
                        Task.Factory.StartNew(StopMiningToPool);
                    }
                };

                pool.BlockUpdated += (sender, info) => { System.Console.WriteLine($"BlockUpdated: {info.Id} - {info.Percent}"); };

                pool.NewBlockFounded += (sender, info) => { System.Console.WriteLine($"NewBlockFounded: {info.Id} {info.Percent}"); };

                pool.DifficultyChanged += (sender, result) => { System.Console.WriteLine($"DifficultyChanged: {result.Difficulty}"); };

                System.Console.ReadLine();
            }
            catch (Exception ex)
            {
                    
            }
        }

        private static void ScreenPoolSsh(IProgress<ScriptOutputLine> progress, CancellationToken token)
        {
            foreach (var farm in _farms)
            {
                System.Console.WriteLine($"+++ started: {farm.Host}");
                var connectionInfo = new ConnectionInfo(farm.Host, farm.Port, farm.Login,
                    new PasswordAuthenticationMethod(farm.Login, farm.Password), new PrivateKeyAuthenticationMethod("rsa.key"));

                var sshclient = new SshClient(connectionInfo);
                
                sshclient.Connect();

                //var commandSsh = sshclient.CreateCommand($"screen -x miner");
                //var commandSsh = sshclient.CreateCommand($"ssh -t {farm.Login}@{farm.Host}:{farm.Port} screen -x miner");

                //sshclient.CreateShellStream("screent", 80,24,800,600,1024, new Dictionary<TerminalModes, uint>{n})

                //commandSsh.ExecuteAsync(progress, token);
            }
        }
    

        private static void StartMiningToPool()
        {
            foreach (var farm in _farms)
            {
                System.Console.WriteLine($"+++ started: {farm.Host}");
                var connectionInfo = new ConnectionInfo(farm.Host, farm.Port, farm.Login,
                    new PasswordAuthenticationMethod(farm.Login, farm.Password), new PrivateKeyAuthenticationMethod("rsa.key"));

                using (var sshclient = new SshClient(connectionInfo))
                {
                    sshclient.Connect();

                    //var commandSsh = sshclient.CreateCommand(
                    //    $"echo 'ccminer -a lbry -o stratum+tcp://lbry.suprnova.cc:6256 -u {farm.Worker} -p x' > /root/MiningPoolHub/manual-command.txt");
                    //var commandSsh = sshclient.CreateCommand(
                    //    $"echo 'ccminer -a lbry -o stratum+tcp://lbc.coinmine.pl:8787 -u {farm.Worker} -p x' > /root/MiningPoolHub/manual-command.txt");

                    //var commandSsh = sshclient.CreateCommand(
                    //    $"echo 'miner --server zec-eu.suprnova.cc --port 2142 --user {farm.Worker} --pass x --solver 0 --fee 0 --cuda_devices' > /root/MiningPoolHub/manual-command.txt");

                    var commandSsh = sshclient.CreateCommand(
                        $"echo 'miner --server btg.suprnova.cc --port 8816 --user {farm.Worker} --pass x --solver 0 --fee 0 --cuda_devices' > /root/MiningPoolHub/manual-command.txt");

                    //var commandSsh = sshclient.CreateCommand(
                    //    $"echo 'ccminer -a lyra2v2 -o stratum+tcp://lyra2rev2.eu.nicehash.com:3347 -u 3EmSA4xHw1p7gNvMeFCY5BG5e2zpve12ba.moscow -p x' > /root/MiningPoolHub/manual-command.txt");

                    //var commandSsh = sshclient.CreateCommand(
                    //    $"echo 'ccminer -a sib -o stratum+tcp://sib.suprnova.cc:3458 -u {farm.Worker} -p x' > /root/MiningPoolHub/manual-command.txt");

                    //var commandSsh = sshclient.CreateCommand(
                    //    $"echo 'ccminer -a lyra2v2 -o stratum+tcp://hub.miningpoolhub.com:20507 -u {farm.Worker} -p x' > /root/MiningPoolHub/manual-command.txt");

                    //var commandSsh = sshclient.CreateCommand(
                    //    $"echo 'miner --server equihash.eu.nicehash.com --user 3EmSA4xHw1p7gNvMeFCY5BG5e2zpve12ba.home --pass x --port 3357 --solver 0 --fee 0 --cuda_devices' > /root/MiningPoolHub/manual-command.txt");

                    //var commandSsh = sshclient.CreateCommand(
                    //    $"echo 'ccminer -a cryptonight -o stratum+tcp://cryptonight.eu.nicehash.com:3355 -u 3EmSA4xHw1p7gNvMeFCY5BG5e2zpve12ba.home -p x' > /root/MiningPoolHub/manual-command.txt");

                    commandSsh.Execute();

                    sshclient.Disconnect();
                }
            }
        }

        private static void StopMiningToPool()
        {
            foreach (var farm in _farms)
            {
                System.Console.WriteLine($"+++ stopped: {farm.Host}");

                var connectionInfo = new ConnectionInfo(farm.Host, farm.Port, farm.Login,
                    new PasswordAuthenticationMethod(farm.Login, farm.Password), new PrivateKeyAuthenticationMethod("rsa.key"));

                using (var client = new SftpClient(connectionInfo))
                {

                    try
                    {
                        client.Connect();
                        client.DeleteFile("/root/MiningPoolHub/manual-command.txt");
                        client.Disconnect();
                    }
                    catch
                    {

                    }
                }
            }
        }


        static void Main_13_10_2017(string[] args)
        {
            //var sn = new MiningPortalApi("https://lbry.suprnova.cc", 5000);

            //sn.RowOfBlockParsed += (sender, block) => { };

            //sn.NewBlockFounded += (sender, block) => { System.Console.WriteLine($"Added ({DateTime.Now}) ID:{block.Id} Percent:{block.Percent}"); };
            //sn.BlockUpdated += (sender, block) => { System.Console.WriteLine($"Changed ({DateTime.Now}) ID:{block.Id} Percent:{block.Percent}"); };

            APIWrapper.Initialize(Settings.Default.NiceApiId, Settings.Default.NiceApiKey);
            var o = APIWrapper.GetMyOrders(0, (byte) AlgoNiceHashEnum.Equihash);

            o.ForEach(ord => System.Console.WriteLine(ord.ID));

            System.Console.ReadKey();
        }

        static void MainOld3(string[] args)
        {
            APIWrapper.Initialize(Properties.Settings.Default.NiceApiId, Properties.Settings.Default.NiceApiKey);

            var balance = APIWrapper.GetBalance();
            //APIWrapper.OrderCreate()

            System.Console.WriteLine($"- {balance.Confirmed} - {balance.Pending} -");
            System.Console.ReadKey();
        }

        static void MainOld2(string[] args)
        {
            while (!System.Console.KeyAvailable)
            {
                var maxPrice = HandlerClass.HandleOrder(CoinsWhatToMineEnum.Lbc, 0);
                // Example how to print some data on console...
                System.Console.WriteLine($"{maxPrice:F4}");
                Thread.Sleep(20000);
            }

            System.Console.ReadLine();
        }

        static void MainOld(string[] args)
        {
            var algoList = new Algorithms();

            
            var ordersStorage = new OrdersStorage(algoList.First(a => a.Id == (byte)AlgoNiceHashEnum.Lbry), 2000);
            ordersStorage.Entities.ListChanged += EntitiesOnListChanged;

            System.Console.WriteLine();
            System.Console.ReadKey();
        }

        private static void EntitiesOnListChanged(object sender, ListChangedEventArgs listChangedEventArgs)
        {
            var storage = sender as BindingList<Order>;
            if (storage == null) return;

            var order = storage[listChangedEventArgs.NewIndex];
        }
    }
}
