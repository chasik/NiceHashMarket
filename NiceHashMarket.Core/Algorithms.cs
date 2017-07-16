using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using NiceHashMarket.Core.Interfaces;
using NiceHashMarket.Model;
using NiceHashMarket.Model.Interfaces;

namespace NiceHashMarket.Core
{
    public class Algorithms : IHaveAlgorithms, IEnumerable<IAlgo>
    {
        private List<IAlgo> _algoList;
        private const string ResourceName = "NiceHashMarket.Core.Algorithms.txt";

        public List<IAlgo> AlgoList
        {
            get => _algoList ?? (_algoList = new List<IAlgo>());
            set => _algoList = value;
        }

        public Algorithms()
        {
            LoadFromResources();
        }

        public void LoadFromResources()
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (var stream = assembly.GetManifestResourceStream(ResourceName))
            {
                if (stream == null)
                    throw new Exception("Не могу загрузить ресурс Algorithms.txt");

                using (var reader = new StreamReader(stream))
                {
                    var result = reader.ReadToEnd();

                    var regex = new Regex(@"^(?<index>\d{1,3})\s*=\s*(?<name>[a-zA-Z1-9]*)", RegexOptions.Multiline);

                    var matches = regex.Matches(result);

                    foreach (Match match in matches)
                    {
                        AlgoList.Add(new Algo{Id = byte.Parse(match.Groups["index"].Value), Name = match.Groups["name"].Value});
                    }
                }
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
