using System.Collections.Generic;
using UnityEngine;
using Havengard.Core.HealthSystem;
using Havengard.Units;

namespace Havengard.Combat
{
    /// <summary>
    /// Centralized global manager that tracks all active IHealth units.
    /// Automatically cleans nulls, avoids GC, and supports team filtering.
    /// </summary>
    public static class UnitTargetManager
    {
        // Internal reusable list to prevent GC allocations
        private static readonly List<IHealth> registeredUnits = new List<IHealth>(256);
        public static IReadOnlyList<IHealth> RegisteredUnits => registeredUnits;

        // Optional categorized cache for faster team filtering
        private static readonly Dictionary<Faction, List<IHealth>> factionBuckets = new()
        {
            { Faction.Player, new List<IHealth>(64) },
            { Faction.Ally, new List<IHealth>(64) },
            { Faction.Enemy, new List<IHealth>(128) },
            { Faction.Neutral, new List<IHealth>(16) },
        };

        // ------------------------------------------------------------------------
        #region Registration
        // ------------------------------------------------------------------------

        /// <summary>
        /// Registers a new unit into the global registry.
        /// </summary>
        public static void Register(IHealth unit)
        {
            if (unit == null || registeredUnits.Contains(unit))
                return;

            registeredUnits.Add(unit);

            Faction f = unit.GetFaction();
            if (!factionBuckets.TryGetValue(f, out var list))
                factionBuckets[f] = list = new List<IHealth>();
            if (!list.Contains(unit))
                list.Add(unit);
        }

        /// <summary>
        /// Unregisters a unit from both lists.
        /// </summary>
        public static void Unregister(IHealth unit)
        {
            if (unit == null) return;
            registeredUnits.Remove(unit);

            Faction f = unit.GetFaction();
            if (factionBuckets.TryGetValue(f, out var list))
                list.Remove(unit);
        }

        #endregion
        // ------------------------------------------------------------------------

        /// <summary>
        /// Returns a clean enumerable of all valid (non-null) IHealth units.
        /// </summary>
        public static IEnumerable<IHealth> ActiveUnits
        {
            get
            {
                for (int i = registeredUnits.Count - 1; i >= 0; i--)
                {
                    var u = registeredUnits[i];
                    if (u == null || (u as MonoBehaviour)?.gameObject == null)
                    {
                        registeredUnits.RemoveAt(i);
                        continue;
                    }
                    yield return u;
                }
            }
        }

        /// <summary>
        /// Returns only enemies relative to a given faction.
        /// Fast version that uses pre-sorted buckets and no allocations.
        /// </summary>
        public static IEnumerable<IHealth> GetEnemiesOf(Faction faction)
        {
            foreach (var kv in factionBuckets)
            {
                var targetFaction = kv.Key;
                if (targetFaction == faction)
                    continue;

                var list = kv.Value;
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    var u = list[i];
                    if (u == null || (u as MonoBehaviour)?.gameObject == null)
                    {
                        list.RemoveAt(i);
                        continue;
                    }

                    if (FactionUtility.CanDamage(faction, u, false))
                        yield return u;
                }
            }
        }

        /// <summary>
        /// Cleans up null or destroyed entries in all lists.
        /// Call occasionally (e.g., every few seconds).
        /// </summary>
        public static void Cleanup()
        {
            for (int i = registeredUnits.Count - 1; i >= 0; i--)
            {
                if (registeredUnits[i] == null || (registeredUnits[i] as MonoBehaviour)?.gameObject == null)
                    registeredUnits.RemoveAt(i);
            }

            foreach (var list in factionBuckets.Values)
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (list[i] == null || (list[i] as MonoBehaviour)?.gameObject == null)
                        list.RemoveAt(i);
                }
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// For debugging in the Unity Editor.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            registeredUnits.Clear();
            foreach (var list in factionBuckets.Values)
                list.Clear();
        }
#endif
    }
}
