using System;
using NiceHashMarket.YiiMiningPool.Interfaces;

namespace NiceHashMarket.YiiMiningPool
{
    public static class YiiFactory
    {
        public static IYiiPool Create(YiiPoolEnum pool)
        {
            IYiiPool result;

            switch (pool)
            {
                case YiiPoolEnum.NiceHash:
                    result = new NiceHash();
                    break;
                case YiiPoolEnum.Zpool:
                    result = new Zpool();
                    break;
                case YiiPoolEnum.Ahashpool:
                    result = new Ahashpool();
                    break;
                case YiiPoolEnum.HashRefinery:
                    result = new HashRefinery();
                    break;
                case YiiPoolEnum.BlazePool:
                    result = new BlazePool();
                    break;
                case YiiPoolEnum.BlockMasters:
                    result = new BlockMasters();
                    break;
                case YiiPoolEnum.ZergPool:
                    result = new ZergPool();
                    break;
                case YiiPoolEnum.YiiMp:
                    result = new YiiMp();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pool), pool, null);
            }

            result.PoolType = pool;

            return result;
        }
    }
}
