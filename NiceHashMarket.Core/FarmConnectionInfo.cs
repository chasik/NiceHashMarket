using System.Collections.Generic;

namespace NiceHashMarket.Core
{
    public class FarmConnectionInfo
    {
        #region | Properties |

        public string Host { get; set; }

        public int Port { get; set; }

        public string Login { get; set; }

        public string Password { get; set; }

        public string Worker { get; set; }

        #endregion

        public FarmConnectionInfo(IReadOnlyList<string> connectionItems)
        {
            Host = connectionItems[0];
            Port = int.Parse(connectionItems[1]);
            Login = connectionItems[2];
            Password = connectionItems[3];
            Worker = connectionItems[4];
        }

        public static FarmConnectionInfo Parse(string paramsWithDelemiter)
        {
            var connectionItems = paramsWithDelemiter.Split(',');

            if (connectionItems.Length < 5)
                return null;

            var farm = new FarmConnectionInfo(connectionItems);

            if (farm.Host.Contains("#"))
                return null;

            return farm;
        }
    }
}
