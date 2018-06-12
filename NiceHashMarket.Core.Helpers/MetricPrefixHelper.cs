using System;
using NiceHashMarket.Model.Enums;

namespace NiceHashMarket.Core.Helpers
{
    public static class MetricPrefixHelper
    {
        public static long MetricConvert(this MetricPrefixEnum metrixFrom, MetricPrefixEnum metricTo, double value)
        {
            long fullValue;

            switch (metrixFrom)
            {
                case MetricPrefixEnum.Kilo:
                    fullValue = (long)(value * 1000);
                    break;
                case MetricPrefixEnum.Mega:
                    fullValue = (long)(value * 1000000);
                    break;
                case MetricPrefixEnum.Giga:
                    fullValue = (long)(value * 1000000000);
                    break;
                case MetricPrefixEnum.Tera:
                    fullValue = (long)(value * 1000000000000);
                    break;
                case MetricPrefixEnum.Peta:
                    fullValue = (long)(value * 1000000000000000);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(metrixFrom), metrixFrom, null);
            }

            long convertedValue;
            switch (metricTo)
            {
                case MetricPrefixEnum.Kilo:
                    convertedValue = fullValue / 1000;
                    break;
                case MetricPrefixEnum.Mega:
                    convertedValue = fullValue / 1000000;
                    break;
                case MetricPrefixEnum.Giga:
                    convertedValue = fullValue / 1000000000;
                    break;
                case MetricPrefixEnum.Tera:
                    convertedValue = fullValue / 1000000000000;
                    break;
                case MetricPrefixEnum.Peta:
                    convertedValue = fullValue / 1000000000000000;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(metricTo), metricTo, null);
            }

            return convertedValue;
        }
    }
}
