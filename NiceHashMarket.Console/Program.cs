using NiceHashMarket.Core;
using System.Linq;

namespace NiceHashMarket.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new ApiClient();
            var algoList = new Algorithms();

            var orders = client.GetOrders(algoList.First(a => a.Id == 23));

            foreach (var order in orders)
            {
                System.Console.WriteLine(order.Price);
            }

            System.Console.WriteLine();
            System.Console.ReadKey();
        }
    }
}
