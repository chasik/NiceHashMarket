using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using NiceHashMarket.Core.Helpers;
using NiceHashMarket.Core.Interfaces;
using NiceHashMarket.Model.Interfaces;

namespace NiceHashMarket.Core
{
    public class DataStorage<T>
        where T : IHaveId, INotifyPropertyChanged
    {
        private Timer _timer;
        private int _frequencyQueryMilliseconds;


        public IAlgo Algo { get; set; }
        public ApiClient ApiClient { get; set; }
        public BindingList<T> Entities { get; set; }
        public PropertyChangedEventHandler PropertyChangedHandler { get; set; }

        public DataStorage(ApiClient apiClient, IAlgo algo, int frequencyQueryMilliseconds, PropertyChangedEventHandler propertyChangedHandler)
        {
            Entities = new BindingList<T>();

            Algo = algo;

            ApiClient = apiClient;

            PropertyChangedHandler = propertyChangedHandler;

            _frequencyQueryMilliseconds = frequencyQueryMilliseconds;

            _timer = new Timer(ApiQueryExecute, null, 0, _frequencyQueryMilliseconds);
        }

            public virtual void ApiQueryExecute(object state)
        {
        }

        public void UpdateBindingList(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                var knownEntity = Entities.FirstOrDefault(x => x.Id == entity.Id);

                if (knownEntity == null)
                {
                    Entities.Add(entity);
                    entity.PropertyChanged += PropertyChangedHandler;

                    continue;
                }

                entity.CopyProperties(knownEntity);
            }
        }
    }
}
