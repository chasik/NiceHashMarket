using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using NiceHashMarket.Core.Helpers;
using NiceHashMarket.Model.Interfaces;

namespace NiceHashMarket.Core
{
    public delegate void AlgoChangedDelegate<T>(DataStorage<T> sender, IAlgo oldAlgo, IAlgo newAlgo) 
        where T : IHaveId, INotifyPropertyChanged;

    public abstract class DataStorage<T> : IEnumerable<T>
        where T : IHaveId, INotifyPropertyChanged
    {
        public event AlgoChangedDelegate<T> AlgoChanging;
        public event AlgoChangedDelegate<T> AlgoChanged;

        private Timer _timer;
        private readonly int _frequencyQueryMilliseconds;
        private readonly SynchronizationContext _syncContext;

        public IAlgo Algo { get; set; }
        public ApiClient ApiClient { get; set; }
        public NiceBindingList<T> Entities { get; set; }

        public abstract void JsonQueryExecute();

        protected DataStorage(IAlgo algo, int frequencyQueryMilliseconds)
        {
            _syncContext = SynchronizationContext.Current;
            _frequencyQueryMilliseconds = frequencyQueryMilliseconds;

            Algo = algo;

            Entities = new NiceBindingList<T>();

            ApiClient = new ApiClient();

            _timer = new Timer(TimerOnElapsed, null, 0, _frequencyQueryMilliseconds);
        }

        private void TimerOnElapsed(object state)
        {
            if (_syncContext == null)
                JsonQueryExecute();
            else
                _syncContext.Send(s => JsonQueryExecute(), state);
        }

        public virtual void SelectAnotherAlgo(IAlgo newAlgo)
        {
            var oldAlgo = Algo;
            AlgoChanging?.Invoke(this, oldAlgo, newAlgo);

            Entities = new NiceBindingList<T>();

            Algo = newAlgo;

            AlgoChanged?.Invoke(this, oldAlgo, newAlgo);
        }

        public void UpdateBindingList(IEnumerable<T> newEntities)
        {
            var newEntitiesArray = newEntities as T[] ?? newEntities.ToArray();

            if (newEntitiesArray.Any())
            {
                var entitiesIds = newEntitiesArray.Select(e => e.Id).ToList();
                var closedEntities = Entities.Where(e => !entitiesIds.Contains(e.Id)).ToArray();

                foreach (var closedEntity in closedEntities)
                {
                    Entities.Remove(closedEntity);
                }
            }

            foreach (var entity in newEntitiesArray)
            {
                var existEntity = Entities.FirstOrDefault(x => x.Id == entity.Id);

                if (existEntity == null)
                {
                    Entities.Add(entity);
                    continue;
                }

                entity.CopyProperties(existEntity);
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
