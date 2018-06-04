using Newtonsoft.Json;

namespace NiceHashMarket.YiiMiningPool
{
    public class YiiAlgoNiceHash
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("paying")]
        public double EstimateCurrent { get; set; }
        [JsonProperty("port")]
        public int Port { get; set; }
    }
}
