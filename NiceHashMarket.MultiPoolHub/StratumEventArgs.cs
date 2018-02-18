using System;
using NiceHashMarket.Model.Enums;

namespace NiceHashMarket.MultiPoolHub
{
    public class StratumEventArgs : EventArgs
    {
        public AlgoNiceHashEnum Algo;
        public object MiningEventArg;
    }
}
