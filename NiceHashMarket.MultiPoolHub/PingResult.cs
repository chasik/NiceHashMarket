using System;

namespace NiceHashMarket.MultiPoolHub
{
    public class PingResult
    {
        public PingResult(bool success, string result)
        {
            Success = success;
            Result = result;

            DateTime = DateTime.Now;
        }

        public bool Success { get; set; }

        public string Result { get; set; }

        public DateTime DateTime { get; set; }
    }
}
