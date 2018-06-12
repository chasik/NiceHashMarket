using System.Collections.Generic;
using NiceHashMarket.Model.Interfaces;

namespace NiceHashMarket.Core.Interfaces
{
    public interface IHaveAlgorithms
    {
        List<IAlgo> AlgoList { get; set; }

        void LoadFromEnum(bool onlyNiceHash);
    }
}