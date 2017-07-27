using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using NiceHashMarket.Core.Helpers;
using NiceHashMarket.Model.Interfaces;

namespace NiceHashMarket.Core
{
    public delegate void AlgoChangedDelegate<T>(DataStorage<T> sender, IAlgo oldAlgo, IAlgo newAlgo) 
        where T : IHaveId, INotifyPropertyChanged;

    public class DataStorage<T> : IEnumerable<T>
        where T : IHaveId, INotifyPropertyChanged
    {
        private Timer _timer;
        private readonly Dispatcher _currentDispatcher;
        private readonly int _frequencyQueryMilliseconds;

        public event AlgoChangedDelegate<T> AlgoChanged;

        public IAlgo Algo { get; set; }
        public ApiClient ApiClient { get; set; }
        public NiceBindingList<T> Entities { get; set; }

        public DataStorage(ApiClient apiClient, IAlgo algo, int frequencyQueryMilliseconds, Dispatcher currentDispatcher)
        {
            _currentDispatcher = currentDispatcher;

            Entities = new NiceBindingList<T>();

            Algo = algo;

            ApiClient = apiClient;

            _frequencyQueryMilliseconds = frequencyQueryMilliseconds;

            _timer = new Timer(TimerOnElapsed, null, 0, _frequencyQueryMilliseconds);
        }

        private void TimerOnElapsed(object state)
        {
            if (_currentDispatcher == null)
                ApiQueryExecute();
            else
                _currentDispatcher.Invoke(ApiQueryExecute);
        }

        public virtual void SelectAnotherAlgo(IAlgo newAlgo)
        {
            Entities = new NiceBindingList<T>();
            var oldAlgo = Algo;
            Algo = newAlgo;

            AlgoChanged?.Invoke(this, oldAlgo, newAlgo);
        }

        public virtual void ApiQueryExecute()
        {
        }

        public void UpdateBindingList(IEnumerable<T> entities)
        {
            if (entities.Any())
            {
                var entitiesIds = entities.Select(e => e.Id).ToList();
                var closedEntities = Entities.Where(e => !entitiesIds.Contains(e.Id)).Select(e => e.Id).ToList();

                closedEntities.ForEach(eid => Entities.Remove(Entities.First(e => e.Id == eid)));
            }

            foreach (var entity in entities)
            {
                var knownEntity = Entities.FirstOrDefault(x => x.Id == entity.Id);

                if (knownEntity == null)
                {
                    Entities.Add(entity);
                    continue;
                }

                entity.CopyProperties(knownEntity);
            }
        }

        public T this[int index]
        {
            get => Entities[index];
            set => Entities[index] = value;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Entities.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
