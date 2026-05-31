using UnityEngine;
using System;

namespace Havengard.Resources
{
    /// <summary>
    /// Secondary mystical resource. Gained from defeated enemies and daily solar resonance.
    /// Used for towers, Havenstone upgrades, and global effects.
    /// </summary>
    public class CelestiumSystem : MonoBehaviour
    {
        public static CelestiumSystem Instance { get; private set; }

        public event Action<int> OnCelestiumChanged;

        [SerializeField] private int startingCelestium = 0;
        public int Current { get; private set; }
        
        // ADD ALIAS FOR SAVE SYSTEM COMPATIBILITY
        public int CurrentCelestium => Current;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Current = startingCelestium;
        }

        public void AddCelestium(int amount)
        {
            Current += Mathf.Max(0, amount);
            OnCelestiumChanged?.Invoke(Current);
        }

        public bool SpendCelestium(int amount)
        {
            if (Current < amount) return false;
            Current -= amount;
            OnCelestiumChanged?.Invoke(Current);
            return true;
        }
        
        // ADD THIS METHOD FOR SAVE SYSTEM
        /// <summary>
        /// Set celestium amount directly (for loading saves)
        /// </summary>
        public void SetCelestium(int amount)
        {
            Current = Mathf.Max(0, amount);
            OnCelestiumChanged?.Invoke(Current);
            Debug.Log($"[CelestiumSystem] Celestium set to: {Current}");
        }
    }
}
