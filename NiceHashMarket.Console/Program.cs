using System;
using System.Collections.Generic;
using System.ComponentModel;
using NiceHashMarket.Core;
using System.Linq;
using System.Threading;
using NiceHashBotLib;
using NiceHashMarket.Model;
using NiceHashMarket.Model.Enums;
using NiceHashMarket.Model.Interfaces;
using Order = NiceHashBotLib.Order;

namespace NiceHashMarket.Console
{
    class Program
    {
        private static List<BlockInfo> _blocksInfo { get; set; } = new List<BlockInfo>();

        static void Main(string[] args)
        {
            var sn = new SuprNovaApi("https://lbry.suprnova.cc", 2000);

            sn.OnRowOfBlockParsed += (sender, block) =>
            {
                var existedBlock = _blocksInfo.FirstOrDefault(b => b.Id == block.Id);

                if (existedBlock != null && Math.Abs((existedBlock?.Percent - block?.Percent).Value) < 0.001)
                    return;

                if (_blocksInfo.Any(b => b.Id == block.Id))
                {
                    _blocksInfo.First(b => b.Id == block.Id).Percent = block.Percent;
                    System.Console.WriteLine($"Changed ({DateTime.Now}) ID:{block.Id} Percent:{block.Percent}");
                    return;
                }

                _blocksInfo.Add(block);
                System.Console.WriteLine($"Added ({DateTime.Now}) ID:{block.Id} Percent:{block.Percent}");
            };

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
                var maxPrice = HandlerClass.HandleOrder(CoinsWhatToMineEnum.Lbc);
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
