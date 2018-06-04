using System.Collections.Generic;

namespace NiceHashMarket.YiiMiningPool
{
    public class HashRefinery : YiiPool
    {
        public override string ApiUrl { get; set; } = "https://pool.hashrefinery.com/api/";

        public HashRefinery()
        {
            PrecisionAlgosCorrections = new Dictionary<string, double>
            {
                {"quark", 1000},
                {"qubit", 1000},
                {"scrypt", 1000},
                {"x11", 1000},
                {"yescrypt", 0.001}
            };
        }
    }
}
