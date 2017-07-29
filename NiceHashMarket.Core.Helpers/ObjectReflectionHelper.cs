using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NiceHashMarket.Core.Helpers
{
    public static class ObjectReflectionHelper
    {
        private static readonly IEnumerable<string> DontCopyProperties = new List<string>
        {
            "DeltaPercentPrice",
            "DeltaPercentAmount",
            "DeltaPercentSpeed",
            "DeltaPercentWorkers",
            "PriceChanged",
            "History",
            "LastUpdate",
            "LastUpdateBackTenMinutes"
        };

        /// <summary>
        /// Copy properties values from source to destination
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        public static void CopyProperties(this object source, object destination)
        {
            if (source == null || destination == null)
                throw new Exception("Source or/and Destination Objects are null");

            var typeDest = destination.GetType();
            var typeSrc = source.GetType();

            var srcProps = typeSrc.GetProperties();
            foreach (var srcProp in srcProps)
            {
                if (!srcProp.CanRead) continue;

                var targetProperty = typeDest.GetProperty(srcProp.Name);

                if (targetProperty == null) continue;

                if (!targetProperty.CanWrite) continue;

                if (targetProperty.GetSetMethod(true) != null && targetProperty.GetSetMethod(true).IsPrivate) continue;

                if ((targetProperty.GetSetMethod().Attributes & MethodAttributes.Static) != 0) continue;

                if (!targetProperty.PropertyType.IsAssignableFrom(srcProp.PropertyType)) continue;

                var sourceValue = srcProp.GetValue(source, null);
                var destinationValue = targetProperty.GetValue(destination, null);

                if (!Equals(sourceValue, destinationValue) 
                    && !DontCopyProperties.Any(p => string.Equals(p, targetProperty.Name, StringComparison.InvariantCultureIgnoreCase)))
                {
                    targetProperty.SetValue(destination, sourceValue, null);
                    //MarketLogger.Information("set property reflection {@propertyName}: {@orderId} {@sourceValue} {@destinationValue}", targetProperty.Name, (destination as IHaveId)?.Id, sourceValue, destinationValue);
                }
            }
        }
    }
}
