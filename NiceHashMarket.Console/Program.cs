using System.ComponentModel;
using NiceHashMarket.Core;
using System.Linq;
using NiceHashMarket.Model;

namespace NiceHashMarket.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new ApiClient();
            var algoList = new Algorithms();

            
            var ordersStorage = new OrdersStorage(client, algoList.First(a => a.Id == 23), 2000);
            ordersStorage.Entities.ListChanged += EntitiesOnListChanged;

            System.Console.WriteLine();
            System.Console.ReadKey();
        }

        private static void EntitiesOnListChanged(object sender, ListChangedEventArgs listChangedEventArgs)
        {
            var storage = sender as BindingList<Order>;
            if (storage == null) return;

            var order = storage[listChangedEventArgs.NewIndex];

            //if (listChangedEventArgs.PropertyDescriptor?.Name == "DeltaPrice")
            //    System.Console.WriteLine($"(LIST CHANGED) {listChangedEventArgs.ListChangedType}\t{listChangedEventArgs.PropertyDescriptor?.Name}\t{order}");

            //MarketLogger.Information(
            //    "(LIST CHANGED: {@orderId}) {@changeType} {@property} " + order, order.Id, listChangedEventArgs.ListChangedType, listChangedEventArgs.PropertyDescriptor?.Name);
        }

    }
}
