using System.ComponentModel;
using NiceHashMarket.Core;
using System.Linq;
using System.Threading;
using NiceHashBotLib;
using NiceHashMarket.Model.Enums;
using NiceHashMarket.Model.Interfaces;

namespace NiceHashMarket.Console
{
    class Program
    {
        static void Main(string[] args)
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
