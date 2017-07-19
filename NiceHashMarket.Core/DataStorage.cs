using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using NiceHashMarket.Model.Interfaces;

namespace NiceHashMarket.Core
{
    public class DataStorage<T> 
    {
        private Timer _timer;
        private IAlgo _algo;
        private ApiClient _apiClient;
        private int _frequencyQueryMilliseconds;

        public BindingList<T> Entities { get; set; }

        public DataStorage(ApiClient apiClient, IAlgo algo, int frequencyQueryMilliseconds)
        {
            Entities = new BindingList<T>();

            _algo = algo;

            _apiClient = apiClient;

            _frequencyQueryMilliseconds = frequencyQueryMilliseconds;

            _timer = new Timer(ApiQueryExecute, null, 0, _frequencyQueryMilliseconds);
        }

        private void ApiQueryExecute(object state)
        {
            var orders = _apiClient.GetOrders(_algo);
        }

        public void AddRange(IEnumerable<T> entities) 
        {
            
        }
    }
}
