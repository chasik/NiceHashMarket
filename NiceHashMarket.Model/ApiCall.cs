using System;

namespace NiceHashMarket.Model
{
    public class ApiCall
    {
        public DateTime LastCall { get; set; }
        public DateTime LastTryDecrease { get; set; }
        public bool LastTryDecreaseSuccess { get; set; }
    }
}
