using System;
using Serilog;

namespace NiceHashMarket.Logger
{
    public static class MarketLogger
    {
        static MarketLogger()
        {
            Log.Logger = new LoggerConfiguration().WriteTo.Seq("http://localhost:5341").CreateLogger();
        }

        public static void Information(string messageTemplate)
        {
            Log.Information(messageTemplate);
        }

        public static void Information(string messageTemplate, params object[] propertyValues)
        {
            Log.Information(messageTemplate, propertyValues);
        }

        public static void Error(string messageTemplate)
        {
            Log.Error(messageTemplate);
        }

        public static void Error(Exception exception, string messageTemplate)
        {
            Log.Error(exception, messageTemplate);
        }

        public static void Error(string messageTemplate, params object[] propertyValues)
        {
            Log.Error(messageTemplate, propertyValues);
        }

        public static void Error(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Log.Error(exception, messageTemplate, propertyValues);
        }
    }
}
