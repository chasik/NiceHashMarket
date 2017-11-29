namespace NiceHashMarket.Core.Interfaces.Transactions
{
    public interface IOutput
    {
        string Address { get; set; }

        double Value { get; set; }
    }
}
