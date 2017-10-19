using System;

namespace NiceHashMarket.Model
{
    public class ApiCall
    {
        public Order Order { get; set; }

        public ApiCallType Type { get; set; }

        public DateTime LastStartTime { get; set; }

        public DateTime LastFinishTime { get; set; }

        public DateTime LastTryDecreaseTime { get; set; }

        public bool LastTryDecreaseSuccess { get; set; }
    }

    public enum ApiCallType
    {
        Unknown = 0,
        InProcess = 1,
        Success = 2,
        Failed = 3
    }
}
