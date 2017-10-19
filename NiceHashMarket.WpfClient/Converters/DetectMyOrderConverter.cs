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
            return int.TryParse(values[0].ToString(), out var orderId) &&
                   (!(values[1] is NiceBindingList<Order> myOrders) ||
                    myOrders.ToList().Any(order => order.Id == orderId));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}