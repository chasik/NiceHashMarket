using System;

namespace NiceHashMarket.Model
{
    public class HistoryChange<T>
    {
        public HistoryChange(T oldValue, T newValue)
        {
            Created = DateTime.Now;

            OldValue = oldValue;
            NewValue = newValue;
        }

        public DateTime Created { get; set; }

        public T OldValue { get; set; }

        public T NewValue { get; set; }
    }
}
