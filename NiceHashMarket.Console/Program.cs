using System.ComponentModel;
using NiceHashMarket.Core;
using System.Linq;
using System.Threading;
using NiceHashBotLib;
using NiceHashMarket.Model.Enums;

namespace NiceHashMarket.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            double maxPrice = 0;
            double limit = 0;

            while (!System.Console.KeyAvailable)
            {
                HandlerClass.HandleOrder(CoinsWhatToMineEnum.Lbc);
                Thread.Sleep(20000);
            }

            System.Console.ReadLine();
        }

        static void MainOld(string[] args)
        {
            var client = new ApiClient();
            var algoList = new Algorithms();

            
            var ordersStorage = new OrdersStorage(client, algoList.First(a => a.Id == (byte)AlgoNiceHashEnum.Lbry), 2000);
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
