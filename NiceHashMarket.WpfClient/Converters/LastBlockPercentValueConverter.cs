using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;
using NiceHashMarket.Model;

namespace NiceHashMarket.WpfClient.Converters
{
    public class LastBlockPercentValueConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var blocks = value as BindingList<BlockInfo>;

            var lastBlock = blocks?.OrderByDescending(b => b.Created).FirstOrDefault();

            if (lastBlock == null) return "-1";

            return $"ID:{lastBlock.Id} {lastBlock.Percent} %\t";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
                _converter = new LastBlockPercentValueConverter();

            return _converter;
        }

        public static LastBlockPercentValueConverter _converter { get; set; }
    }
}
