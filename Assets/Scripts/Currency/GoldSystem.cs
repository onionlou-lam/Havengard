using UnityEngine;
using System;

namespace Havengard.Resources
{
    /// <summary>
    /// Shared gold pool for recruiting, upgrades, and economy.
    /// Should be stored in a persistent GameObject (GameManager).
    /// </summary>
    public class GoldSystem : MonoBehaviour
    {
        public static GoldSystem Instance { get; private set; }

        public event Action<int> OnGoldChanged;

        [SerializeField] private int startingGold = 100;
        public int Current { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Current = startingGold;
        }

        public void AddGold(int amount)
        {
            Current += Mathf.Max(0, amount);
            OnGoldChanged?.Invoke(Current);
        }

        public bool SpendGold(int amount)
        {
            if (Current < amount) return false;
            Current -= amount;
            OnGoldChanged?.Invoke(Current);
            return true;
        }
    }
}
