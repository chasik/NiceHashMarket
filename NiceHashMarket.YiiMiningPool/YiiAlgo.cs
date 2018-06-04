using Newtonsoft.Json;
using NiceHashMarket.YiiMiningPool.Interfaces;

namespace NiceHashMarket.YiiMiningPool
{
    public class YiiAlgo : IYiiAlgo
    {
        public YiiAlgo()
        {
        }

        public YiiAlgo(YiiAlgoNiceHash niceAlgo)
        {
            Name = niceAlgo.Name;
            EstimateCurrent = niceAlgo.EstimateCurrent;
            Port = niceAlgo.Port;
        }

        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("port")]
        public int Port { get; set; }
        [JsonProperty("coins")]
        public int Coins { get; set; }
        [JsonProperty("fees")]
        public int Fees { get; set; }
        [JsonProperty("hashrate")]
        public long HashRate { get; set; }
        [JsonProperty("workers")]
        public int Workers { get; set; }
        [JsonProperty("estimate_current")]
        public double EstimateCurrent { get; set; }
        [JsonProperty("estimate_last24h")]
        public double EstimateLast24H { get; set; }
        [JsonProperty("actual_last24h")]
        public double ActualLast24H { get; set; }
        [JsonProperty("ashrate_last24h")]
        public float HashRateLast24H { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}