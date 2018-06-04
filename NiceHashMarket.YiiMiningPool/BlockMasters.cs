using System.Collections.Generic;

namespace NiceHashMarket.YiiMiningPool
{
    public class BlockMasters : YiiPool
    {
        public override string ApiUrl { get; set; } = "http://blockmasters.co/api/";

        public BlockMasters()
        {
            PrecisionAlgosCorrections = new Dictionary<string, double>
            {
                {"scrypt", 1000},
                {"sha256", 1000000},
                {"yescryptR16", 0.001}
            };
        }
    }
}