using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace NiceHashMarket.YiiMiningPool.Interfaces
{
    public interface IYiiPool
    {
        event EventHandler<JObject> StatusReceived;

        string ApiUrl { get; set; }

        void LoopCommand(int period, YiiCommandEnum command, IDictionary<string, string> commandParams = null);
    }
}
