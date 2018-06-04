using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using NiceHashMarket.YiiMiningPool.Interfaces;

namespace NiceHashMarket.YiiMiningPool
{
    public class YiiCommand
    {
        public YiiCommandEnum Command { get; set; }
        public Action<JObject> Action { get; set; }
        public IDictionary<string, string> CommandParams { get; set; }
    }
}
