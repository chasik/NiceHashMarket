using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace NiceHashMarket.YiiMiningPool.Interfaces
{
    public interface IYiiPool
    {
        event EventHandler<JEnumerable<JToken>> StatusReceived;

        string ApiUrl { get; set; }

        DateTime LastQuery { get; set; }

        Dictionary<string, double> PrecisionAlgosCorrections { get; set; }

        YiiPoolEnum PoolType { get; set; }

        Dictionary<string, IYiiAlgo> PoolsAlgos { get; set; }

        void LoopCommand(int period, YiiCommandEnum command, IDictionary<string, string> commandParams = null);
    }
}
