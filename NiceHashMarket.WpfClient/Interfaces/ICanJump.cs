using System;
using NiceHashMarket.Model;

namespace NiceHashMarket.WpfClient.Interfaces
{
    public interface ICanJump
    {
        DateTime DoJump (Order targetOrder);
    }
}
