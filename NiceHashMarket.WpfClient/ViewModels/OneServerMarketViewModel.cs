using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm;

namespace NiceHashMarket.WpfClient.ViewModels
{
    [POCOViewModel]
    public class OneServerMarketViewModel : ISupportParameter
    {
        public virtual object Parameter { get; set; }

        protected void OnParameterChanged()
        {
            
        }
    }
}