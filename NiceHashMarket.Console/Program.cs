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

            var ordersStorage = new OrdersStorage(client, algoList.First(a => a.Id == 23), 2000, PropertyChangedHandler);

            System.Console.WriteLine();
            System.Console.ReadKey();
        }

        private static void PropertyChangedHandler(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            System.Console.WriteLine($"(changed: {propertyChangedEventArgs.PropertyName}) {sender as Order}");
        }
    }
}
