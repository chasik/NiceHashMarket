using NiceHashMarket.Model.Enums;

namespace NiceHashMarket.MultiPoolHub
{
    public class StratumConnection
    {
        public AlgoNiceHashEnum Algo { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public string UserOrWallet { get; set; }

        public string Worker { get; set; }

        public string Password { get; set; }

        public string UserName => $"{UserOrWallet}.{Worker}";

        public static StratumConnection Parse(AlgoNiceHashEnum algo, string stratumConnectionString)
        {

            var values = stratumConnectionString.Split(',');

            var connection = new StratumConnection{ Algo = algo, Host=values[0], Port = int.Parse(values[1]), UserOrWallet = values[2], Worker = values[3], Password = values[4]};

            return connection;
        }

        public override string ToString()
        {
            return Algo.ToString();
        }
    }
}
