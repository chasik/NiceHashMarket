using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace NiceHashMarket.Model
{
    public class HistoryDictionary : Dictionary<string, ObservableCollection<HistoryChange<object>>>
    {
        private readonly Dictionary<string, PropertyInfo> _properties = new Dictionary<string, PropertyInfo>();

        /// <summary>
        /// Add history item
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="sourceOrder"></param>
        /// <param name="destiantionOrder"></param>
        public void AddValue(string propertyName, Order oldOrder, Order newOrder)
        {
            if (!_properties.ContainsKey(propertyName))
            {
                _properties.Add(propertyName, typeof(Order).GetProperty(propertyName));
            }

            if (!Keys.Contains(propertyName))
                Add(propertyName, new ObservableCollection<HistoryChange<object>>());

            this[propertyName]
                .Add(new HistoryChange<object>(_properties[propertyName].GetValue(oldOrder),
                    _properties[propertyName].GetValue(newOrder)));
        }
    }
}
