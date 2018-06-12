using System.Collections.Generic;
using NiceHashMarket.Model.Enums;
using NiceHashBotLib;

namespace NiceHashMarket.Core.Helpers
{
    public static class NiceBot
    {
        public static APIBalance GetBalance(this ServerEnum server)
        {
            return APIWrapper.GetBalance(server == ServerEnum.Europe ? 0 : 1);
        }

        public static List<Order> GetMyOrders(this ServerEnum server, AlgoNiceHashEnum currentAlgo)
        {
            return APIWrapper.GetMyOrders(server == ServerEnum.Europe ? 0 : 1, (byte) currentAlgo);
        }

        public static int OrderCreate(this ServerEnum server, AlgoNiceHashEnum currentAlgo, double amount, double price, double limit, Pool pool)
        {
            return APIWrapper.OrderCreate(server == ServerEnum.Europe ? 0 : 1, (int) currentAlgo, amount, price, limit, pool);
        }

        public static double SetPrice(this Model.Order order, AlgoNiceHashEnum currentAlgo, double newPrice)
        {
            return APIWrapper.OrderSetPrice(order.Server == ServerEnum.Europe ? 0 : 1, (int) currentAlgo, order.Id, newPrice);
        }

        public static double SetPriceDecrease(this Model.Order order, AlgoNiceHashEnum currentAlgo)
        {
            return APIWrapper.OrderSetPriceDecrease(order.Server == ServerEnum.Europe ? 0 : 1, (int) currentAlgo, order.Id);
        }

        public static double SetLimit(this Model.Order order, AlgoNiceHashEnum currentAlgo, double newLimit)
        {
            return APIWrapper.OrderSetLimit(order.Server == ServerEnum.Europe ? 0 : 1, (int) currentAlgo, order.Id, newLimit);
        }
    }
}
