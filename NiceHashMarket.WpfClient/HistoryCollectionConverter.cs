using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using NiceHashMarket.Model;

namespace NiceHashMarket.WpfClient
{
    public class HistoryCollectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var collectionName = string.Empty;

            var propertyId = System.Convert.ToByte(parameter);

            if (propertyId == 1)
                collectionName = "Amount";
            else if (propertyId == 2)
                collectionName = "Speed";
            else if (propertyId == 3)
                collectionName = "Workers";
            else
                return null;

            if (!(value as HistoryDictionary).Keys.Contains(collectionName))
                return null;

            var result = (value as HistoryDictionary)[collectionName];

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
