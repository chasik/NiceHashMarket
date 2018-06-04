using System.Collections.Generic;

namespace NiceHashMarket.YiiMiningPool
{
    public class Zpool : YiiPool
    {
        public override string ApiUrl { get; set; } = "http://www.zpool.ca/api/";

        public Zpool()
        {
            PrecisionAlgosCorrections = new Dictionary<string, double>
            {
                {"x13", 0.001}
            };
        }
    }
}
