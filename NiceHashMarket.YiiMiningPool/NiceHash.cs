using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace NiceHashMarket.YiiMiningPool
{
    public class NiceHash : YiiPool
    {
        public override string ApiUrl { get; set; } = "https://api.nicehash.com/";

        public NiceHash()
        {
            PrecisionAlgosCorrections = new Dictionary<string, double>
            {
                {"equihash", 0.000001},
                {"lbry", 0.001},
                {"lyra2v2", 0.001},
                {"lyra2z", 0.001},
                {"neoscrypt", 0.001},
                {"nist5", 0.001},
                {"skunk", 0.001},
                {"sha256", 1000000}
            };
        }

        public override YiiAlgo JtokenParser(JToken jtoken)
        {
            var niceAlgo = jtoken.ToObject<YiiAlgoNiceHash>();

            if (niceAlgo.Name.Contains("lyra2rev2"))
                niceAlgo.Name = "lyra2v2";

            return new YiiAlgo(niceAlgo);
        }

        public override JEnumerable<JToken> GetJTokensFromResponse(string content)
        {
            return JObject.Parse(content)["result"]["simplemultialgo"].Children();
        }
    }
}
