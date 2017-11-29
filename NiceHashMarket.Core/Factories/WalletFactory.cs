using System;
using NiceHashMarket.Core.Blocks;
using NiceHashMarket.Core.Interfaces.Blocks;
using NiceHashMarket.Core.Interfaces.Transactions;
using NiceHashMarket.Core.Interfaces.Wallets;
using NiceHashMarket.Core.Transactions;
using NiceHashMarket.Core.Wallets;
using NiceHashMarket.Model.Enums;

namespace NiceHashMarket.Core.Factories
{
    public static class WalletFactory
    {
        public static IWallet CreateWallet(this CoinsWhatToMineEnum coin)
        {
            switch (coin)
            {
                case CoinsWhatToMineEnum.Btc:
                    break;
                case CoinsWhatToMineEnum.Vert:
                    break;
                case CoinsWhatToMineEnum.Dash:
                    break;
                case CoinsWhatToMineEnum.Monero:
                    break;
                case CoinsWhatToMineEnum.Mona:
                    break;
                case CoinsWhatToMineEnum.Eth:
                    break;
                case CoinsWhatToMineEnum.Lbc:
                    return new LbcWallet("http://localhost:5279", "lbryapi");
                case CoinsWhatToMineEnum.Zec:
                    break;
                case CoinsWhatToMineEnum.Sib:
                    break;
                case CoinsWhatToMineEnum.Komodo:
                    break;
                case CoinsWhatToMineEnum.Music:
                    break;
                case CoinsWhatToMineEnum.Zen:
                    break;
                case CoinsWhatToMineEnum.Sigt:
                    break;
            }

            throw new ArgumentOutOfRangeException(nameof(coin), coin, "Создание экземпляра Wallet для данной монеты не определено!");
        }

        public static IBlock CreateBlock(this CoinsWhatToMineEnum coin)
        {
            switch (coin)
            {
                case CoinsWhatToMineEnum.Btc:
                    break;
                case CoinsWhatToMineEnum.Vert:
                    break;
                case CoinsWhatToMineEnum.Dash:
                    break;
                case CoinsWhatToMineEnum.Monero:
                    break;
                case CoinsWhatToMineEnum.Mona:
                    break;
                case CoinsWhatToMineEnum.Eth:
                    break;
                case CoinsWhatToMineEnum.Lbc:
                    return new LbcBlock();
                case CoinsWhatToMineEnum.Zec:
                    break;
                case CoinsWhatToMineEnum.Sib:
                    break;
                case CoinsWhatToMineEnum.Komodo:
                    break;
                case CoinsWhatToMineEnum.Music:
                    break;
                case CoinsWhatToMineEnum.Zen:
                    break;
                case CoinsWhatToMineEnum.Sigt:
                    break;
            }

            throw new ArgumentOutOfRangeException(nameof(coin), coin, "Создание экземпляра Block для данной монеты не определено!");
        }

        public static ITransaction CreateTransaction(this CoinsWhatToMineEnum coin)
        {
            switch (coin)
            {
                case CoinsWhatToMineEnum.Btc:
                    break;
                case CoinsWhatToMineEnum.Vert:
                    break;
                case CoinsWhatToMineEnum.Dash:
                    break;
                case CoinsWhatToMineEnum.Monero:
                    break;
                case CoinsWhatToMineEnum.Mona:
                    break;
                case CoinsWhatToMineEnum.Eth:
                    break;
                case CoinsWhatToMineEnum.Lbc:
                    return new LbcTransaction();
                case CoinsWhatToMineEnum.Zec:
                    break;
                case CoinsWhatToMineEnum.Sib:
                    break;
                case CoinsWhatToMineEnum.Komodo:
                    break;
                case CoinsWhatToMineEnum.Music:
                    break;
                case CoinsWhatToMineEnum.Zen:
                    break;
                case CoinsWhatToMineEnum.Sigt:
                    break;
            }

            throw new ArgumentOutOfRangeException(nameof(coin), coin, "Создание экземпляра Transaction для данной монеты не определено!");
        }

        public static IInput CreateInput(this CoinsWhatToMineEnum coin)
        {
            switch (coin)
            {
                case CoinsWhatToMineEnum.Btc:
                    break;
                case CoinsWhatToMineEnum.Vert:
                    break;
                case CoinsWhatToMineEnum.Dash:
                    break;
                case CoinsWhatToMineEnum.Monero:
                    break;
                case CoinsWhatToMineEnum.Mona:
                    break;
                case CoinsWhatToMineEnum.Eth:
                    break;
                case CoinsWhatToMineEnum.Lbc:
                    return new Input();
                case CoinsWhatToMineEnum.Zec:
                    break;
                case CoinsWhatToMineEnum.Sib:
                    break;
                case CoinsWhatToMineEnum.Komodo:
                    break;
                case CoinsWhatToMineEnum.Music:
                    break;
                case CoinsWhatToMineEnum.Zen:
                    break;
                case CoinsWhatToMineEnum.Sigt:
                    break;
            }

            throw new ArgumentOutOfRangeException(nameof(coin), coin, "Создание экземпляра Input для данной монеты не определено!");
        }

        public static IOutput CreateOutput(this CoinsWhatToMineEnum coin)
        {
            switch (coin)
            {
                case CoinsWhatToMineEnum.Btc:
                    break;
                case CoinsWhatToMineEnum.Vert:
                    break;
                case CoinsWhatToMineEnum.Dash:
                    break;
                case CoinsWhatToMineEnum.Monero:
                    break;
                case CoinsWhatToMineEnum.Mona:
                    break;
                case CoinsWhatToMineEnum.Eth:
                    break;
                case CoinsWhatToMineEnum.Lbc:
                    return new Output();
                case CoinsWhatToMineEnum.Zec:
                    break;
                case CoinsWhatToMineEnum.Sib:
                    break;
                case CoinsWhatToMineEnum.Komodo:
                    break;
                case CoinsWhatToMineEnum.Music:
                    break;
                case CoinsWhatToMineEnum.Zen:
                    break;
                case CoinsWhatToMineEnum.Sigt:
                    break;
            }

            throw new ArgumentOutOfRangeException(nameof(coin), coin, "Создание экземпляра Output для данной монеты не определено!");
        }
    }
}
