using System.Globalization;

namespace NiceHashMarket.Model
{
    public class BlockInfo
    {
        public BlockInfo(string id, string percent)
        {
            Id = id;

            double parsePercent;

            if (double.TryParse(percent, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out parsePercent))
                Percent = parsePercent;

        }

        public string Id { get; set; }

        public double Percent { get; set; }
    }
}
