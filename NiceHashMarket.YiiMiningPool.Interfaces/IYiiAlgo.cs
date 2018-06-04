namespace NiceHashMarket.YiiMiningPool.Interfaces
{
    public interface IYiiAlgo
    {
        string Name { get; set; }
        int Port { get; set; }
        int Coins { get; set; }
        int Fees { get; set; }
        long HashRate { get; set; }
        int Workers { get; set; }
        double EstimateCurrent { get; set; }
        double EstimateLast24H { get; set; }
        double ActualLast24H { get; set; }
        float HashRateLast24H { get; set; }
    }
}
