using System.Windows;
using System.Windows.Controls;

namespace NiceHashMarket.WpfClient.TemplateSelectors
{
    public class OrderDetailTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DefaultTemplate { get; set; }
        public DataTemplate SelfOwnerTemplate { get; set; }


        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {

            return DefaultTemplate;
        }
    }
}
