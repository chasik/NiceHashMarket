namespace NiceHashMarket.Core.Interfaces.Transactions
{
    public interface IInput
    {
        bool IsCoinBase { get; set; }

        string Address { get; set; }
    }
}
