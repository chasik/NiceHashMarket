using System;
using System.Collections.Generic;
using NiceHashMarket.Model.Enums;
using NiceHashBotLib;

namespace NiceHashMarket.Core.Helpers
{
    public static class NiceBot
    {
        //public static Dictionary<ServerEnum, List<string>> ApiKeys { get; set; }

        private static void SetApiKey(ServerEnum server)
        {
            //APIWrapper.APIID = ApiKeys[server][0];
            //APIWrapper.APIKey = ApiKeys[server][1];


            //NiceHashMarket.Logger.MarketLogger.Information($"set APIID:{APIWrapper.APIID} APIKEY:{APIWrapper.APIKey}");
        }

        public static APIBalance GetBalance(this ServerEnum server)
        {
            SetApiKey(server);
            return APIWrapper.GetBalance();
        }

        public static List<Order> GetMyOrders(this ServerEnum server, AlgoNiceHashEnum currentAlgo)
        {
            SetApiKey(server);
            return APIWrapper.GetMyOrders(server == ServerEnum.Europe ? 0 : 1, (byte) currentAlgo);
        }

        public static int OrderCreate(this ServerEnum server, AlgoNiceHashEnum currentAlgo, double amount, double price, double limit, Pool pool)
        {
            SetApiKey(server);
            return APIWrapper.OrderCreate(server == ServerEnum.Europe ? 0 : 1, (int) currentAlgo, amount, price, limit, pool);
        }

        public static double SetPrice(this Model.Order order, AlgoNiceHashEnum currentAlgo, double newPrice)
        {
            SetApiKey(order.Server);
            return APIWrapper.OrderSetPrice(order.Server == ServerEnum.Europe ? 0 : 1, (int) currentAlgo, order.Id, newPrice);
        }

        public static double SetPriceDecrease(this Model.Order order, AlgoNiceHashEnum currentAlgo)
        {
            SetApiKey(order.Server);
            return APIWrapper.OrderSetPriceDecrease(order.Server == ServerEnum.Europe ? 0 : 1, (int) currentAlgo, order.Id);
        }

        public static double SetLimit(this Model.Order order, AlgoNiceHashEnum currentAlgo, double newLimit)
        {
            SetApiKey(order.Server);
            return APIWrapper.OrderSetLimit(order.Server == ServerEnum.Europe ? 0 : 1, (int) currentAlgo, order.Id, newLimit);
        }
    }
}
