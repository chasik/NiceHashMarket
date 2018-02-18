using System;
using NiceHashMarket.YiiMiningPool.Interfaces;

namespace NiceHashMarket.YiiMiningPool
{
    public static class YiiFactory
    {
        public static IYiiPool Create(YiiPoolEnum pool)
        {
            switch (pool)
            {
                case YiiPoolEnum.Zpool:
                    return new Zpool();
                case YiiPoolEnum.Ahashpool:
                    return new Ahashpool();
                default:
                    throw new ArgumentOutOfRangeException(nameof(pool), pool, null);
            }
        }
    }
}
