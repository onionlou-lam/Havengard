using UnityEngine;
using System;

namespace Havengard.Resources
{
    public class GoldSystem : MonoBehaviour
    {
        public static GoldSystem Instance { get; private set; }

        public event Action<int> OnGoldChanged;

        [SerializeField] private int startingGold = 0;
        public int Current { get; private set; }
        
        // ADD ALIAS FOR SAVE SYSTEM COMPATIBILITY
        public int CurrentGold => Current;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
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
        
        // ADD THIS METHOD FOR SAVE SYSTEM
        /// <summary>
        /// Set gold amount directly (for loading saves)
        /// </summary>
        public void SetGold(int amount)
        {
            Current = Mathf.Max(0, amount);
            OnGoldChanged?.Invoke(Current);
            Debug.Log($"[GoldSystem] Gold set to: {Current}");
        }
    }
}