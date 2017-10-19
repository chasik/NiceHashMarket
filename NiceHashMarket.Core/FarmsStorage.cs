using System.Collections.Generic;
using System.IO;

namespace NiceHashMarket.Core
{
    public class FarmsStorage
    {
        public static IList<FarmConnectionInfo> LoadFromFile(string pathAndFileName)
        {
            var result = new List<FarmConnectionInfo>();

            using (var reader = new StreamReader(pathAndFileName))
            {
                string inputString;
                while ((inputString = reader.ReadLine()) != null)
                {
                    var farmConnection = FarmConnectionInfo.Parse(inputString);

                    if (farmConnection != null)
                        result.Add(farmConnection);
                }
            }

            return result;
        }
    }
}
