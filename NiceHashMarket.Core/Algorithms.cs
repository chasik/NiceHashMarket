using System;
using System.Collections;
using System.Collections.Generic;
using NiceHashMarket.Core.Interfaces;
using NiceHashMarket.Model;
using NiceHashMarket.Model.Enums;
using NiceHashMarket.Model.Interfaces;

namespace NiceHashMarket.Core
{
    public class Algorithms : IHaveAlgorithms, IEnumerable<IAlgo>
    {
        private List<IAlgo> _algoList;

        public List<IAlgo> AlgoList
        {
            get => _algoList ?? (_algoList = new List<IAlgo>());
            set => _algoList = value;
        }

        public Algorithms(bool onlyNiceHash = true)
        {
            LoadFromEnum(onlyNiceHash);
        }

        public void LoadFromEnum(bool onlyNiceHash)
        {
            var algoType = typeof(AlgoNiceHashEnum);
            var algosIds = Enum.GetValues(algoType);

            foreach (var algoId in algosIds)
            {
                var id = Convert.ToByte(algoId);

                if (onlyNiceHash && id > 99)
                    break;

                AlgoList.Add(new Algo{Id = id, Name = Enum.GetName(algoType, id)});
            }
        }

        public IEnumerator<IAlgo> GetEnumerator()
        {
            return AlgoList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return AlgoList.GetEnumerator();
        }
    }
}
