using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using NiceHashMarket.Core;
using NiceHashMarket.Model;

namespace NiceHashMarket.WpfClient.Converters
{
    public class DetectMyOrderConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var myOrders = values[1] as NiceBindingList<Order>;

            if (!int.TryParse(values[0].ToString(), out int orderId) || myOrders != null && myOrders.All(order => order.Id != orderId))
                return false;

            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}