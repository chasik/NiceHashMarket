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
                collectionName = "Price";
            else if (propertyId == 2)
                collectionName = "DeltaPrice";
            else if (propertyId == 3)
                collectionName = "Amount";
            else if (propertyId == 4)
                collectionName = "DeltaPercentAmount";
            else if (propertyId == 5)
                collectionName = "Workers";
            else if (propertyId == 6)
                collectionName = "DeltaPercentWorkers";
            else if (propertyId == 7)
                collectionName = "Speed";
            else if (propertyId == 8)
                collectionName = "DeltaPercentSpeed";
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
