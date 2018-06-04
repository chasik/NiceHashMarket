using System.Collections.Generic;

namespace NiceHashMarket.YiiMiningPool
{
    public class Ahashpool : YiiPool
    {
        public override string ApiUrl { get; set; } = "https://www.ahashpool.com/api/";

        public Ahashpool()
        {
            PrecisionAlgosCorrections = new Dictionary<string, double>
            {
                {"keccak", 1000},
                {"keccakc", 1000}
            };
        }
    }
}
