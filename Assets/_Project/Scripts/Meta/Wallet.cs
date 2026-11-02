using System;

namespace CrateBoss.Meta
{
    /// <summary>CB Coin, the only currency (Ana Belge §13). It is earned in play only; nothing can be bought with real money.</summary>
    public sealed class Wallet
    {
        private readonly SaveData data;

        public Wallet(SaveData data) => this.data = data;

        public int Balance => data.coins;

        /// <summary>Raised with the new balance after every change.</summary>
        public event Action<int> Changed;

        public void Add(int amount)
        {
            if (amount <= 0) return;
            data.coins += amount;
            Changed?.Invoke(data.coins);
        }

        /// <summary>Spends only when the balance covers the price; otherwise nothing changes.</summary>
        public bool TrySpend(int amount)
        {
            if (amount < 0 || data.coins < amount) return false;
            data.coins -= amount;
            Changed?.Invoke(data.coins);
            return true;
        }
    }
}
