using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Navigation;
using NiceHashMarket.Core;
using NiceHashMarket.Model;

namespace NiceHashMarket.WpfClient.Converters
{
    public class DetectOrderInListConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var orderList = values[1] as NiceBindingList<Order>;

            return int.TryParse(values[0].ToString(), out var orderId) &&
                   orderList != null && OrderListContainsId(orderList, orderId);
        }

        private bool OrderListContainsId(NiceBindingList<Order> orderList, int orderId)
        {
            var counter = 0;

            while (counter < orderList.Count)
            {
                try
                {
                    if (orderList[counter]?.Id == orderId)
                        return true;
                }
                catch (Exception ex)
                {
                    return false;
                }

                counter++;
            }

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}